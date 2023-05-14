using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Identity.Client;
namespace Win_App_calling_MsGraph;
public partial class MainWindow : Window
{
  //Set the API Endpoint to Graph 'me' endpoint
  readonly string graphAPIEndpoint = "https://graph.microsoft.com/v1.0/me";

  //Set the scope for API call to user.read
  readonly string[] scopes = new string[] { "user.read"/*, "drive"*/ };

  public MainWindow() => InitializeComponent();

  /// <summary>
  /// Call AcquireToken - to acquire a token requiring user to sign-in
  /// </summary>
  private async void CallGraphButton_Click(object sender, RoutedEventArgs e)
  {
    AuthenticationResult? authResult = null;
    var app = App.PublicClientApp;
    ResultText.Text = string.Empty;
    TokenInfoText.Text = string.Empty;

    var accounts = await app.GetAccountsAsync();
    var firstAccount = accounts.FirstOrDefault();

    try
    {
      authResult = await app.AcquireTokenSilent(scopes, firstAccount).ExecuteAsync();
    }
    catch (MsalUiRequiredException ex)
    {
      System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}"); // A MsalUiRequiredException happened on AcquireTokenSilent. This indicates you need to call AcquireTokenInteractive to acquire a token

      try
      {
        authResult = await app.AcquireTokenInteractive(scopes)
            .WithAccount(accounts.FirstOrDefault())
            .WithPrompt(Prompt.SelectAccount)
            .ExecuteAsync();
      }
      catch (MsalException msalex)
      {
        ResultText.Text = $"Error Acquiring Token:{System.Environment.NewLine}{msalex}";
      }
    }
    catch (Exception ex)
    {
      ResultText.Text = $"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}";
      return;
    }

    if (authResult != null)
    {
      ResultText.Text = await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken);
      DisplayBasicTokenInfo(authResult);
      SignOutButton.Visibility = Visibility.Visible;
    }
    else
    {
      return;
    }


    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
    var rr  = await httpClient.GetAsync(graphAPIEndpoint); // Call the web API.


    var _graphServiceClient = new Microsoft.Graph.GraphServiceClient(new Microsoft.Graph.DelegateAuthenticationProvider(async (requestMessage) =>
    {
      var token = authResult.AccessToken;
      requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
      await Task.CompletedTask;
    }));

    var me = await _graphServiceClient.Me.Request().GetAsync();
    try
    {
      Microsoft.Graph.DriveItem folder;
      folder = await _graphServiceClient.Drive.Root.Request().Expand("thumbnails,children($expand=thumbnails)").GetAsync();
      folder = await _graphServiceClient.Drive.Root.ItemWithPath("/" + "path").Request().Expand("thumbnails,children($expand=thumbnails)").GetAsync();

      dynamic iems = await _graphServiceClient.Me.Drive.Root.Children.Request().GetAsync(); //tu: onedrive root folder items == 16 dirs.
      var pic0 = iems.Items[12];
      var pic1 = pic0.Name;

      var profilePhoto = await _graphServiceClient.Me.Photo.Content.Request().GetAsync();
      if (profilePhoto != null)
      {
        var ms = new MemoryStream();
        profilePhoto.CopyTo(ms);
        var buffer = ms.ToArray();
        var result = Convert.ToBase64String(buffer);
        var imgDataURL = string.Format("data:image/png;base64, {0}", result);
        //ViewBag.ImageData = imgDataURL;
      }
      else
      {
        //ViewBag.ImageData = "";
      }
    }
    catch (Exception ex)
    {
      Trace.WriteLine($"\n*** Error getting events: {ex.Message}");
    }
  }

  private Task<string> GetAccessTokenAsync() => throw new NotImplementedException();

  /// <summary>
  /// Perform an HTTP GET request to a URL using an HTTP Authorization header
  /// </summary>
  /// <param name="url">The URL</param>
  /// <param name="token">The token</param>
  /// <returns>String containing the results of the GET operation</returns>
  public async Task<string> GetHttpContentWithToken(string url, string token)
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

  /// <summary>
  /// Sign out the current user
  /// </summary>
  private async void SignOutButton_Click(object sender, RoutedEventArgs e)
  {
    var accounts = await App.PublicClientApp.GetAccountsAsync();

    if (accounts.Any())
    {
      try
      {
        await App.PublicClientApp.RemoveAsync(accounts.FirstOrDefault());
        ResultText.Text = "User has signed-out";
        CallGraphButton.Visibility = Visibility.Visible;
        SignOutButton.Visibility = Visibility.Collapsed;
      }
      catch (MsalException ex)
      {
        ResultText.Text = $"Error signing-out user: {ex.Message}";
      }
    }
  }

  /// <summary>
  /// Display basic information contained in the token
  /// </summary>
  private void DisplayBasicTokenInfo(AuthenticationResult authResult)
  {
    TokenInfoText.Text = "";
    if (authResult != null)
    {
      TokenInfoText.Text += $"Username: {authResult.Account.Username}" + Environment.NewLine;
      TokenInfoText.Text += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
    }
  }
}