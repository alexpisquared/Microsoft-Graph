namespace DemoApp;
public partial class MainWindow
{
  public MainWindow()
  {
    InitializeComponent();

    MsgSlideshowUsrCtrl1.ClientId = Win_App_calling_MsGraph.ClientId.AlexpGood; 
    //MsgSlideshowUsrCtrl1.ClientNm = nameof(Win_App_calling_MsGraph.ClientId.AlexpGood);
  }

  void OnClose(object sender, RoutedEventArgs e) => Close();
}