﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using DemoLibrary;
using LibVLCSharp.Shared;
using Microsoft.Graph;
using Microsoft.Graph.CallRecords;
using StandardLib.Helpers;
using static System.Diagnostics.Trace;

namespace DemoApp;
public partial class MainWindow : Window
{
  const string _allFilesTxt = @"C:\g\Microsoft-Graph\Src\msgraph-training-uwp\DemoApp\Stuff\AllFiles.txt", thm = "thumbnails,children($expand=thumbnails)";
  readonly Random _random = new(DateTime.Now.Second);
  GraphServiceClient? _graphServiceClient;
  string[] _allFilesArray;
  readonly LibVLC _libVLC;
  const int _period = 5_000;

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
    WriteLine($"** {_allFilesArray.Length,8:N0}  files in {Stopwatch.GetElapsedTime(start1).TotalSeconds,5:N1} sec.");

    while (true)
    {
      await TryOneDriveMeThingy();
    }
    //await Testingggggggg(thm, file);
  }

  async Task TryOneDriveMeThingy()
  {
    try
    {
      ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));

      var next = _random.Next(_allFilesArray.Length);
      var file = _allFilesArray[next][(OneDrive.Root.Length - Environment.UserName.Length + 5)..];
      //file = @"C:\Users\alexp\OneDrive\Pictures\Main\_New\2013-07-14 Lumia520\Lumia520 014.mp4"[OneDrive.Root.Length..]; //100mb
      //file = @"C:\Users\alexp\OneDrive\Pictures\Camera imports\2018-07\VID_20180610_191622.mp4"[OneDrive.Root.Length..]; //700mb takes ~1min to download on WiFi and only then starts playing.

      Report3.Text = $"{file}";

      var driveItem = await _graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(thm).GetAsync();
      Report3.Text = $"{.000001 * driveItem.Size,8:N2} mb    {driveItem.Name}"; // Write($"** {.000001 * driveItem.Size,8:N2} mb   sec:{Stopwatch.GetElapsedTime(start).TotalSeconds,5:N2}");

      var taskStream = _graphServiceClient.Drive.Root.ItemWithPath(file).Content.Request().GetAsync();
      var taskDelay = Task.Delay(_period);

      var start = Stopwatch.GetTimestamp();
      await Task.WhenAll(taskStream, taskDelay);

      VideoView1.MediaPlayer?.Stop(); 
      System.Media.SystemSounds.Beep.Play();

      Write($"{Stopwatch.GetElapsedTime(start).TotalSeconds,8:N2}");
      if (driveItem.Video is not null)
      {
        var durationInMs = StartPlayingMediaStream(taskStream.Result);
        Report2.Text = $"{.000001 * driveItem.Size,8:N2} mb  {Stopwatch.GetElapsedTime(start).TotalSeconds:N1} sec.   {durationInMs*.001:N0} sec  {driveItem.Name}  ";
      }
      if (driveItem.Image is not null)
      {
        Image2.Source = await GetBipmapFromStream(taskStream.Result);
        Report2.Text = $"{.000001 * driveItem.Size,8:N2} mb  {Stopwatch.GetElapsedTime(start).TotalSeconds:N1} sec.   {driveItem.Image.Width:N0}x{driveItem.Image.Height:N0} {driveItem.Name}  ";
      }

      Write($"  {Stopwatch.GetElapsedTime(start).TotalSeconds,5:N1} = {.000001 * driveItem.Size / Stopwatch.GetElapsedTime(start).TotalSeconds,4:N1} mb/sec.    {driveItem.Name}  \n");
    }
    catch (Exception ex) { Report4.Text = ex.Message; Trace.WriteLine($"** {ex.Message}  "); }
  }
  long StartPlayingMediaStream(Stream stream)
  {
    var media = new LibVLCSharp.Shared.Media(_libVLC, new StreamMediaInput(stream));

    _ = VideoView1.MediaPlayer?.Play(media); // non-blocking
    VideoView1.MediaPlayer?.SeekTo(TimeSpan.FromMilliseconds(media.Duration / 2));

    return media.Duration; // in ms
  }
  static async Task<BitmapImage> GetBipmapFromStream(Stream? stream)
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

    await Task.Delay(100);

    return bmp;
  }

  async void OnNext(object sender, RoutedEventArgs e) => await TryOneDriveMeThingy();
  void OnClose(object sender, RoutedEventArgs e) => Close();

  async Task Testingggggggg(string thm, string file)
  {
    ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
    //var me = await graphServiceClient.Me.Request().GetAsync();
    Image1.Source = await GetBipmapFromStream(await _graphServiceClient.Me.Photo.Content.Request().GetAsync());
    _ = await _graphServiceClient.Drive.Root.Request().Expand(thm).GetAsync();
    _ = await _graphServiceClient.Drive.Root.ItemWithPath("/Pictures").Request().Expand(thm).GetAsync();
    var driveItem4 = await _graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(thm).GetAsync();

    Image4.Source = new Uri(driveItem4.WebUrl); // Image3.Source = new Uri("https://onedrive.live.com/?authkey=undefined&cid=869AFB15787C9269&id=869AFB15787C9269%211118450&parId=869AFB15787C9269%21930167&o=OneUp");

    var items = await _graphServiceClient.Me.Drive.Root.Children.Request().GetAsync(); //tu: onedrive root folder items == 16 dirs.
    _ = items.ToList()[12].Folder;
  }

  public static async Task Main11()
  {
    WriteLine("\n** Main11() started");
    var task1 = Task1();
    var task2 = Task2();
    var task3 = Task.Delay(3000);

    task1.Start();
    task2.Start();
    task3.Start();

    await Task.WhenAll(task1, task2, task3);
  }

  private static async Task Task1() { await Task.Delay(1000); WriteLine("Task 1 completed"); }
  private static async Task Task2() { await Task.Delay(2000); WriteLine("Task 2 completed"); }
}
