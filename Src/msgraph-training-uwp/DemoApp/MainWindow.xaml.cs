﻿using System.Windows;
using MSGraphSlideshow;

namespace DemoApp;
public partial class MainWindow
{
  public MainWindow()
  {
    InitializeComponent();

    MsgSlideshowUsrCtrl1.ClientId = Win_App_calling_MsGraph.ClientId.ZoePiTryC;
    MsgSlideshowUsrCtrl1.ClientNm = nameof(Win_App_calling_MsGraph.ClientId.ZoePiTryC);
  }

  void OnClose(object sender, RoutedEventArgs e) => Close();
}