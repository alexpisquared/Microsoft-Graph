﻿using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
    var graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
    {
      requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
      await Task.CompletedTask;
    }));

    var me = await graphServiceClient.Me.Request().GetAsync();
    try
    {
      Image1.Source = GetBipmapFromStream(await graphServiceClient.Me.Photo.Content.Request().GetAsync());
      Image2.Source = GetBipmapFromStream(await graphServiceClient.Drive.Root.ItemWithPath("/Pictures/2017-02/wp_ss_20170223_0002.png").Content.Request().GetAsync());

      var driveItem1 = await graphServiceClient.Drive.Root.Request().Expand("thumbnails,children($expand=thumbnails)").GetAsync();
      var driveItem2 = await graphServiceClient.Drive.Root.ItemWithPath("/Pictures").Request().Expand("thumbnails,children($expand=thumbnails)").GetAsync();
      var driveItem3 = await graphServiceClient.Drive.Root.ItemWithPath("/Pictures/2017-02/wp_ss_20170223_0002.png").Request().Expand("thumbnails,children($expand=thumbnails)").GetAsync();

      var items = await graphServiceClient.Me.Drive.Root.Children.Request().GetAsync(); //tu: onedrive root folder items == 16 dirs.
      var folderDetails = items.ToList()[12].Folder;
    }
    catch (Exception ex) { Report1.Text = ex.Message; }
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