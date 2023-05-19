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
  const string _allFilesTxt = @"C:\g\Microsoft-Graph\Src\msgraph-training-uwp\DemoApp\Stuff\AllFiles.txt", thm = "thumbnails,children($expand=thumbnails)";
  readonly Random _random = new(DateTime.Now.Second);
  GraphServiceClient? _graphServiceClient;
  string[] _allFilesArray;
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

    Report1.Text = ($"{report}");

    ArgumentNullException.ThrowIfNull(result, nameof(result));

    _graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
    {
      requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
      await Task.CompletedTask;
    }));

    var start1 = Stopwatch.GetTimestamp();
#if DEBUG
    _allFilesArray = System.IO.File.ReadAllLines(_allFilesTxt); //
#else
    allFiles = await OneDrive.GetFileNamesAsync("*.*"); // System.IO.File.WriteAllLines(_allFiles, allFiles);
#endif
    Trace.WriteLine($"** {_allFilesArray.Length,8:N0}  files in {Stopwatch.GetElapsedTime(start1).TotalSeconds,5:N1} sec.");

    await TryOneDriveMeThingy();
  }

  async Task TryOneDriveMeThingy()
  {
    try
    {
      var start1 = Stopwatch.GetTimestamp();
      var next = _random.Next(_allFilesArray.Length);

      var file = _allFilesArray[next][(OneDrive.Root.Length - Environment.UserName.Length + 5)..];
      //file = @"C:\Users\alexp\OneDrive\Pictures\Main\_New\2013-07-14 Lumia520\Lumia520 014.mp4"[OneDrive.Root.Length..]; //100mb
      //file = @"C:\Users\alexp\OneDrive\Pictures\Camera imports\2018-07\VID_20180610_191622.mp4"[OneDrive.Root.Length..]; //700mb takes ~1min to download on WiFi and only then starts playing.

      Report2.Text = $"{file}";

      ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
      var driveItem = await _graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(thm).GetAsync();
      Report2.Text = $"{driveItem.Name}  {.000001 * driveItem.Size,8:N2} / _._ = _._ mb/sec.";
      if (driveItem.Video is not null)
      {
        await StartPlayingMediaStream(await _graphServiceClient.Drive.Root.ItemWithPath(file).Content.Request().GetAsync());
      }
      if (driveItem.Image is not null)
      {
        Image2.Source = await GetBipmapFromStream(await _graphServiceClient.Drive.Root.ItemWithPath(file).Content.Request().GetAsync());
      }
      Report2.Text = $"{driveItem.Name}  {.000001 * driveItem.Size,8:N2} / {Stopwatch.GetElapsedTime(start1).TotalSeconds:N1} = {.000001 * driveItem.Size / Stopwatch.GetElapsedTime(start1).TotalSeconds:N1} mb/sec.";

      //await Testingggggggg(thm, file);
    }
    catch (Exception ex) { Report4.Text = ex.Message; Trace.WriteLine($"** {ex.Message}  "); }
  }

  async Task Testingggggggg(string thm, string file)
  {
    ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
    //var me = await graphServiceClient.Me.Request().GetAsync();
    Image1.Source = await GetBipmapFromStream(await _graphServiceClient.Me.Photo.Content.Request().GetAsync());

    var driveItem1 = await _graphServiceClient.Drive.Root.Request().Expand(thm).GetAsync();
    var driveItem2 = await _graphServiceClient.Drive.Root.ItemWithPath("/Pictures").Request().Expand(thm).GetAsync();
    var driveItem4 = await _graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(thm).GetAsync();

    Image4.Source = new Uri(driveItem4.WebUrl); // Image3.Source = new Uri("https://onedrive.live.com/?authkey=undefined&cid=869AFB15787C9269&id=869AFB15787C9269%211118450&parId=869AFB15787C9269%21930167&o=OneUp");

    var items = await _graphServiceClient.Me.Drive.Root.Children.Request().GetAsync(); //tu: onedrive root folder items == 16 dirs.
    var folderDetails = items.ToList()[12].Folder;
  }

  async Task StartPlayingMediaStream(Stream stream)
  {
    var media = new Media(_libVLC, new StreamMediaInput(stream)); // new Media(_libVLC, "https://streams.videolan.org/streams/360/eagle_360.mp4", FromType.FromLocation);

    _ = VideoView1.MediaPlayer?.Play(media); // non-blocking
    await Task.Delay(1000);
    VideoView1.MediaPlayer?.SeekTo(TimeSpan.FromMilliseconds(media.Duration / 2));
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