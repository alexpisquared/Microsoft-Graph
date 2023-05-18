using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DemoLibrary;
using LibVLCSharp.Shared;
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
    var graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
    {
      requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
      await Task.CompletedTask;
    }));

    var pic = "/Pictures/2017-02/wp_ss_20170223_0002.png";
    var vid = "/Pictures/2016-07/WP_20160710_12_43_38_Pro.mp4";
    var thm = "thumbnails,children($expand=thumbnails)";

    var me = await graphServiceClient.Me.Request().GetAsync();
    try
    {
      Image1.Source = GetBipmapFromStream(await graphServiceClient.Me.Photo.Content.Request().GetAsync());
      Image2.Source = GetBipmapFromStream(await graphServiceClient.Drive.Root.ItemWithPath(pic).Content.Request().GetAsync());
      var stream =( (await graphServiceClient.Drive.Root.ItemWithPath(vid).Content.Request().GetAsync()));

      var ms = new MemoryStream();
      stream.CopyTo(ms);
      Play_Clicked(ms);

      var driveItem1 = await graphServiceClient.Drive.Root.Request().Expand(thm).GetAsync();
      var driveItem2 = await graphServiceClient.Drive.Root.ItemWithPath("/Pictures").Request().Expand(thm).GetAsync();
      var driveItem3 = await graphServiceClient.Drive.Root.ItemWithPath(pic).Request().Expand(thm).GetAsync();
      var driveItem4 = await graphServiceClient.Drive.Root.ItemWithPath(vid).Request().Expand(thm).GetAsync();

      Image4.Source =new Uri( driveItem4.WebUrl); // Image3.Source = new Uri("https://onedrive.live.com/?authkey=undefined&cid=869AFB15787C9269&id=869AFB15787C9269%211118450&parId=869AFB15787C9269%21930167&o=OneUp");

      var items = await graphServiceClient.Me.Drive.Root.Children.Request().GetAsync(); //tu: onedrive root folder items == 16 dirs.
      var folderDetails = items.ToList()[12].Folder;
    }
    catch (Exception ex) { Report1.Text = ex.Message; }
  }

  private void Play_Clicked(MemoryStream ms)
  {
    var _libVLC = new LibVLC();
    Image3.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
    var tt = new Media(_libVLC, new StreamMediaInput(ms)); // new Media(_libVLC, "https://streams.videolan.org/streams/360/eagle_360.mp4", FromType.FromLocation);

    _ = Image3.MediaPlayer.Play(tt);
  }


  static BitmapImage GetBipmapFromStream(Stream? stream)
  {
    ArgumentNullException.ThrowIfNull(stream, nameof(stream));

    var ms = new MemoryStream();
    stream.CopyTo(ms);

    var bmp = new System.Windows.Media.Imaging.BitmapImage();
    bmp.BeginInit();
    bmp.StreamSource = ms;
    bmp.EndInit();

    return bmp;
  }
}