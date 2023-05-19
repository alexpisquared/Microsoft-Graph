using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DemoLibrary;
using LibVLCSharp.Shared;
using Microsoft.Graph;
using StandardLib.Helpers;

namespace DemoApp;
public partial class MainWindow : Window
{
  readonly Random rand = new(DateTime.Now.Second);
  GraphServiceClient? graphServiceClient;
  readonly LibVLC _libVLC;

  public MainWindow()
  {
    InitializeComponent();
    _libVLC = new LibVLC();
    VideoView1.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
  }

  async void Window_Loaded(object sender, RoutedEventArgs e)
  {
    var (success, report, result) = await new AuthUsagePOC().LogInAsync();

    Report1.Foreground = success ? Brushes.DarkGreen : Brushes.Red;
    Report1.Text = ($"{report}");

    ArgumentNullException.ThrowIfNull(result, nameof(result));

    graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
    {
      requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
      await Task.CompletedTask;
    }));

    await TryOneDriveMeThingy();
  }

  async Task TryOneDriveMeThingy()
  {
    try
    {
      var start1 = Stopwatch.GetTimestamp();
      var allFiles = await OneDrive.GetFileNamesAsync("*.*");
      Trace.WriteLine($"** {allFiles.Length,8:N0}  files in {Stopwatch.GetElapsedTime(start1).TotalSeconds,5:N1} sec.");

      var next = rand.Next(allFiles.Length);

      var thm = "thumbnails,children($expand=thumbnails)";
      var file = allFiles[next][OneDrive.Root.Length..];//.Replace("",""); = "/Pictures/2016-07/WP_20160710_12_43_38_Pro.mp4";  var pic = "/Pictures/2017-02/wp_ss_20170223_0002.png";
      //file = @"C:\Users\alexp\OneDrive\Pictures\Main\_New\2013-07-14 Lumia520\Lumia520 014.mp4"[OneDrive.Root.Length..]; //100mb
      //file = @"C:\Users\alexp\OneDrive\Pictures\Camera imports\2018-07\VID_20180610_191622.mp4"[OneDrive.Root.Length..]; //700mb takes ~1min to download on WiFi and only then starts playing.

      ArgumentNullException.ThrowIfNull(graphServiceClient, nameof(graphServiceClient));
      var driveItem = await graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(thm).GetAsync();
      Report2.Text = $"{driveItem.Name}  {driveItem.Size}";
      if (driveItem.Video is not null)
      {
        PlayMediaStream(await graphServiceClient.Drive.Root.ItemWithPath(file).Content.Request().GetAsync());
      }
      if (driveItem.Image is not null)
      {
        Image2.Source = await GetBipmapFromStream(await graphServiceClient.Drive.Root.ItemWithPath(file).Content.Request().GetAsync());
      }

      //var me = await graphServiceClient.Me.Request().GetAsync();
      Image1.Source = await GetBipmapFromStream(await graphServiceClient.Me.Photo.Content.Request().GetAsync());

      var driveItem1 = await graphServiceClient.Drive.Root.Request().Expand(thm).GetAsync();
      var driveItem2 = await graphServiceClient.Drive.Root.ItemWithPath("/Pictures").Request().Expand(thm).GetAsync();
      var driveItem4 = await graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(thm).GetAsync();

      Image4.Source = new Uri(driveItem4.WebUrl); // Image3.Source = new Uri("https://onedrive.live.com/?authkey=undefined&cid=869AFB15787C9269&id=869AFB15787C9269%211118450&parId=869AFB15787C9269%21930167&o=OneUp");

      var items = await graphServiceClient.Me.Drive.Root.Children.Request().GetAsync(); //tu: onedrive root folder items == 16 dirs.
      var folderDetails = items.ToList()[12].Folder;
    }
    catch (Exception ex) { Report4.Text = ex.Message; Trace.WriteLine($"** {ex.Message}  "); }
  }

  void PlayMediaStream(Stream stream)
  {
    var media = new Media(_libVLC, new StreamMediaInput(stream)); // new Media(_libVLC, "https://streams.videolan.org/streams/360/eagle_360.mp4", FromType.FromLocation);

    _ = VideoView1.MediaPlayer.Play(media);
  }

  static async Task<BitmapImage> GetBipmapFromStream(Stream? stream)
  {
    ArgumentNullException.ThrowIfNull(stream, nameof(stream));

    var ms = new MemoryStream();
    await stream.CopyToAsync(ms);
    _ = ms.Seek(0, SeekOrigin.Begin); //tu: a fix for JPG images!!!
    stream.Close();

    var bmp = new System.Windows.Media.Imaging.BitmapImage();
    bmp.BeginInit();
    bmp.StreamSource = ms;
    bmp.EndInit();

    return bmp;
  }

  void OnClose(object sender, RoutedEventArgs e) => Close();

  async void OnNext(object sender, RoutedEventArgs e) => await TryOneDriveMeThingy();
}