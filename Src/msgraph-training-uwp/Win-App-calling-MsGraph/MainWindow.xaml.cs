﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
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
    ResultText.Text = "";
    TokenInfoText.Text = "";

    var accounts = await App.PublicClientApp.GetAccountsAsync();
    AuthenticationResult? authResult = null;

    try
    {
      authResult = await App.PublicClientApp.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
    }
    catch (MsalUiRequiredException ex)
    {
      Debug.WriteLine($"MsalUiRequiredException: {ex.Message}"); // A MsalUiRequiredException happened on AcquireTokenSilent. This indicates you need to call AcquireTokenInteractive to acquire a token

      try
      {
        authResult = await App.PublicClientApp.AcquireTokenInteractive(scopes)
            .WithAccount(accounts.FirstOrDefault())
            .WithPrompt(Microsoft.Identity.Client.Prompt.SelectAccount)
            .ExecuteAsync();
      }
      catch (MsalException msalex)
      {
        ResultText.Text = $"Error Acquiring Token:   {msalex}";
      }
    }
    catch (Exception ex)
    {
      ResultText.Text = $"Error Acquiring Token Silently:   {ex}";
      throw;
    }

    ArgumentNullException.ThrowIfNull(authResult);

    string token = authResult.AccessToken;

    ResultText.Text = await GetHttpContentWithToken(graphAPIEndpoint, token);
    TokenInfoText.Text = $"Username: {authResult.Account.Username} \n Token Expires: {authResult.ExpiresOn.ToLocalTime()}";
    SignOutButton.Visibility = Visibility.Visible;

    await TrySimlplestTest(token);
    await TryOneDriveMeThingy(token);
  }

  async Task TrySimlplestTest(string token)
  {
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    ResultText.Text = (await httpClient.GetAsync(graphAPIEndpoint)).ToString(); // Call the web API.
  }

  async Task TryOneDriveMeThingy(string token)
  {
    var _graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
    {
      requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
      await Task.CompletedTask;
    }));

    var me = await _graphServiceClient.Me.Request().GetAsync();
    try
    {
      DriveItem folder = await _graphServiceClient.Drive.Root.Request().Expand("thumbnails,children($expand=thumbnails)").GetAsync();
      DriveItem folde2 = await _graphServiceClient.Drive.Root.ItemWithPath("/" + "Pictures").Request().Expand("thumbnails,children($expand=thumbnails)").GetAsync();

      IDriveItemChildrenCollectionPage iems = await _graphServiceClient.Me.Drive.Root.Children.Request().GetAsync(); //tu: onedrive root folder items == 16 dirs.
      var pic0 = iems.ToList()[12];
      //var pic1 = pic0.Name;

      var profilePhoto = await _graphServiceClient.Me.Photo.Content.Request().GetAsync();
      if (profilePhoto != null)
      {
        var ms = new MemoryStream();
        profilePhoto.CopyTo(ms);
        var buffer = ms.ToArray();
        var result = Convert.ToBase64String(buffer);
        var imgDataURL = string.Format("data:image/png;base64, {0}", result);

        var bmp = new System.Windows.Media.Imaging.BitmapImage();
        bmp.BeginInit();
        bmp.StreamSource = ms;
        bmp.EndInit();

        Image1.Source = bmp;
      }
      else
      {
        //ViewBag.ImageData = "";
      }
    }
    catch (Exception ex)
    {
      Trace.WriteLine($"\n*** Error getting events: {ex.Message}");
      ResultText.Text = ex.Message;
    }
  }

  Task<string> GetAccessTokenAsync() => throw new NotImplementedException();

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
  async void SignOutButton_Click(object sender, RoutedEventArgs e)
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

}