namespace DemoApp;
public partial class MainWindow
{
  public MainWindow()
  {
    InitializeComponent();

    DragMovable = false;

    if (Debugger.IsAttached || true)
    {
      canvas.Width = 1920;  //  WinFormsControlLib.WinFormHelper.PrimaryScreen.Bounds.Width;
      canvas.Height = 1080; //  WinFormsControlLib.WinFormHelper.PrimaryScreen.Bounds.Height;
    }
    else
    {
      canvas.Width = 5120;  //  WinFormsControlLib.WinFormHelper.GetSumOfAllBounds.Width;
      canvas.Height = 2520; //  WinFormsControlLib.WinFormHelper.GetSumOfAllBounds.Height;
    }
    WriteLine(tbkTitle.Text = $"{Environment.MachineName}:   canvas.Width: {canvas.Width}, canvas.Height: {canvas.Height}");

    UnitF1_A.ClientId = Win_App_calling_MsGraph.ClientId.AlexpGood;
    //MsgSlideshowUsrCtrl1.ClientId = Win_App_calling_MsGraph.ClientId.AlexpGood;
    //MsgSlideshowUsrCtrl1.ClientNm = nameof(Win_App_calling_MsGraph.ClientId.AlexpGood);
  }

  void OnClose(object sender, RoutedEventArgs e) => Close();
}