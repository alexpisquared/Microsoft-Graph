using System.Windows;

namespace DemoApp;
public partial class MainWindow
{
  public MainWindow() => InitializeComponent();

  void OnClose(object sender, RoutedEventArgs e) => Close();
}