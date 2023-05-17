using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DemoLibrary;
using Microsoft.Graph;

namespace DemoApp;
public partial class MainWindow : Window
{
  public MainWindow() => InitializeComponent();

  async void Window_Loaded(object sender, RoutedEventArgs e)
  {
    var (success, report, result) = await new AuthUsagePOC().LogInAsync();

    Report1.Foreground = success ? Brushes.DarkGreen : Brushes.Red;
    Report1.Text = ($"{report}");

    await TryOneDriveMeThingy(result.AccessToken);
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
      var folder = await _graphServiceClient.Drive.Root.Request().Expand("thumbnails,children($expand=thumbnails)").GetAsync();
      var folde2 = await _graphServiceClient.Drive.Root.ItemWithPath("/" + "Pictures").Request().Expand("thumbnails,children($expand=thumbnails)").GetAsync();

      var iems = await _graphServiceClient.Me.Drive.Root.Children.Request().GetAsync(); //tu: onedrive root folder items == 16 dirs.
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
    }
    catch (Exception ex) { Report1.Text = ex.Message; }
  }
}