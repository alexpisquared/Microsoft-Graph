using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Graph;
using Microsoft.Identity.Client;
namespace Win_App_calling_MsGraph;
public partial class MainWindow : Window
{
  readonly string graphAPIEndpoint = "https://graph.microsoft.com/v1.0/me";  //Set the API Endpoint to Graph 'me' endpoint
  readonly string[] scopes = new string[] { "user.read", /*"User.Read","MailboxSettings.Read","Calendars.ReadWrite",*/"Files.Read" }; // from C:\gh\s\onedrive-sample-photobrowser-uwp\OneDrivePhotoBrowser\AccountSelection.xaml.cs:  "onedrive.readonly", "wl.signin", "offline_access" };

  public MainWindow() => InitializeComponent();

  async void CallGraphButton_Click(object sender, RoutedEventArgs e)
  {
    ResultText.Foreground = System.Windows.Media.Brushes.Green; ResultText.Text = "";
    TokenInfoText.Text = "";

    var accounts = await App.PublicClientApp.GetAccountsAsync();
    AuthenticationResult? authResult = null;

    try
    {
      authResult = await App.PublicClientApp.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
    }
    catch (MsalUiRequiredException ex)
    {
      Trace.WriteLine($"MsalUiRequiredException: {ex.Message}"); // A MsalUiRequiredException happened on AcquireTokenSilent. This indicates you need to call AcquireTokenInteractive to acquire a token

      try
      {
        authResult = await App.PublicClientApp.AcquireTokenInteractive(scopes)
            .WithAccount(accounts.FirstOrDefault())
            .WithPrompt(Microsoft.Identity.Client.Prompt.SelectAccount)
            .ExecuteAsync();
      }
      catch (Exception msalex)
      {
        ResultText.Foreground = System.Windows.Media.Brushes.DarkRed; ResultText.Text = $"Error Acquiring Token:   {msalex}";
        return;
      }
    }
    catch (Exception ex)
    {
      ResultText.Foreground = System.Windows.Media.Brushes.DarkRed; ResultText.Text = $"Error Acquiring Token Silently:   {ex}";
      throw;
    }

    ArgumentNullException.ThrowIfNull(authResult);

    var token = authResult.AccessToken;

    ResultText.Foreground = System.Windows.Media.Brushes.Magenta; ResultText.Text = await GetHttpContentWithToken(graphAPIEndpoint, token);
    TokenInfoText.Text = $"Username: {authResult.Account.Username} \n Token Expires: {authResult.ExpiresOn.ToLocalTime()}";
    SignOutButton.Visibility = Visibility.Visible;

    await TrySimlplestTest(token);
    await TryOneDriveMePhoto(token);
  }

  async Task TrySimlplestTest(string token)
  {
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    ResultTex2.Text = (await httpClient.GetAsync(graphAPIEndpoint)).ToString(); // Call the web API.
  }

  async Task TryOneDriveMePhoto(string token)
  {
    var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
    {
      requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
      await Task.CompletedTask;
    }));

    try
    {
      //var folder = await graphClient.Drive.Root.Request().Expand("thumbnails,children($expand=thumbnails)").GetAsync();
      //var iems__ = await graphClient.Me.Drive.Root.Children.Request().GetAsync(); //tu: onedrive root folder items == 16 dirs.
      //var pic000 = iems__.ToList()[12];

      var file = "/Pictures/id.png";
      var driveItem = await graphClient.Drive.Root.ItemWithPath(file).Request().Expand("thumbnails,children($expand=thumbnails)").GetAsync();
      Debug.WriteLine($"{driveItem.Name}  {driveItem.Size}  ");
      if (driveItem != null)
      {
        var taskStream = graphClient.Drive.Root.ItemWithPath(file).Content.Request().GetAsync();
        var taskDelay = Task.Delay(111);
        await Task.WhenAll(taskStream, taskDelay);

        Image2.Source = (await GetBipmapFromStream(taskStream.Result)).bitmapImage;
      }

      var profilePhoto = await graphClient.Me.Photo.Content.Request().GetAsync();
      if (profilePhoto != null)
      {
        var ms = new MemoryStream();
        await profilePhoto.CopyToAsync(ms);
        var buffer = ms.ToArray();
        var result = Convert.ToBase64String(buffer);
        var imgDataURL = string.Format("data:image/png;base64, {0}", result);

        var bmp = new System.Windows.Media.Imaging.BitmapImage();
        bmp.BeginInit();
        bmp.StreamSource = ms;
        bmp.EndInit();

        Image1.Source = bmp;
      }
    }
    catch (Exception ex)
    {
      Trace.WriteLine($"\n*** Error getting events: {ex.Message}");
      ResultText.Foreground = System.Windows.Media.Brushes.DarkRed; ResultText.Text = ex.Message;
    }
  }

  static async Task<(BitmapImage bitmapImage, string report)> GetBipmapFromStream(Stream? stream)
  {
    ArgumentNullException.ThrowIfNull(stream, nameof(stream));

    var memoryStream = new MemoryStream();
    await stream.CopyToAsync(memoryStream);
    _ = memoryStream.Seek(0, SeekOrigin.Begin); //tu: JPG images fix!!!
    stream.Close();

    var bmp = new System.Windows.Media.Imaging.BitmapImage();
    bmp.BeginInit();
    bmp.StreamSource = memoryStream;
    bmp.EndInit();

    await Task.Yield();

    return (bmp, "++");
  }


  //Task<string> GetAccessTokenAsync() => throw new NotImplementedException();

  /// <summary>
  /// Perform an HTTP GET request to a URL using an HTTP Authorization header
  /// </summary>
  /// <param name="url">The URL</param>
  /// <param name="token">The token</param>
  /// <returns>String containing the results of the GET operation</returns>
  public static async Task<string> GetHttpContentWithToken(string url, string token)
  {
    var httpClient = new System.Net.Http.HttpClient();
    System.Net.Http.HttpResponseMessage response;
    try
    {
      var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
      //Add the token in Authorization header
      request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
      response = await httpClient.SendAsync(request);
      var content = await response.Content.ReadAsStringAsync();
      return content;
    }
    catch (Exception ex)
    {
      return ex.ToString();
    }
  }

  async void SignOutButton_Click(object sender, RoutedEventArgs e)
  {
    var accounts = await App.PublicClientApp.GetAccountsAsync();

    if (accounts.Any())
    {
      try
      {
        await App.PublicClientApp.RemoveAsync(accounts.FirstOrDefault());
        ResultText.Foreground = System.Windows.Media.Brushes.Blue; ResultText.Text = "User has signed-out";
        CallGraphButton.Visibility = Visibility.Visible;
        SignOutButton.Visibility = Visibility.Collapsed;

        Image1.Source = new BitmapImage(new Uri("pack://application:,,,/favicon.ico"));
        Image2.Source = new BitmapImage(new Uri("pack://application:,,,/favicon.ico"));
        Image3.Source = new BitmapImage(new Uri("pack://application:,,,/favicon.ico"));
      }
      catch (MsalException ex)
      {
        ResultText.Foreground = System.Windows.Media.Brushes.DarkRed; ResultText.Text = $"Error signing-out user: {ex.Message}";
      }
    }
  }

  void User_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
  {
    if (e.AddedItems.Count < 1) return;

    SignOutButton_Click(sender, e);
    App.NewMethod(((System.Windows.FrameworkElement)e.AddedItems[0]).Tag.ToString());
  }
}