using MSGraphSlideshow;

namespace MSGraphSlideshowApp.View;

public partial class UnitF1View
{
  public UnitF1View()
  {
    InitializeComponent();

    MsgSlideshowUsrCtrl1.ClientId = Win_App_calling_MsGraph.ClientId.AlexpGood;
    MsgSlideshowUsrCtrl1.ClientNm = nameof(Win_App_calling_MsGraph.ClientId.AlexpGood);
  }
  public static readonly DependencyProperty ClientIdProperty = DependencyProperty.Register("ClientId", typeof(string), typeof(UnitF1View)); public string ClientId { get { return (string)GetValue(ClientIdProperty); } set { SetValue(ClientIdProperty, value); } }

  new void OnLoaded(object sender, RoutedEventArgs e)
  {
    base.OnLoaded(sender, e);

    MsgSlideshowUsrCtrl1.ClientId = ClientId; // new WpfUserControlLib.Helpers.ConfigRandomizer().GetRandomFromUserSection("ClientId");
  }
}
