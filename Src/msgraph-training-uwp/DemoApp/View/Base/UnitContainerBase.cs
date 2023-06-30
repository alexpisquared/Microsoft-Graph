﻿namespace MSGraphSlideshowApp.View.Base;
public partial class UnitContainerBase : UserControl
{
  bool isDragging, _isResizing, _isLoaded;
  System.Windows.Point _lastMousePosition, clickPosition;
  public static readonly DependencyProperty WindowStateProperty = DependencyProperty.Register("WindowState", typeof(bool), typeof(UnitContainerBase), new PropertyMetadata(true, propChanged)); public bool WindowState { get => (bool)GetValue(WindowStateProperty); set => SetValue(WindowStateProperty, value); }
  static async void propChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is UnitContainerBase uc && e.NewValue is bool b)
      if (b)
      {
        await uc.RestorePlacementFromFile();
        uc.WindowState = true;
        await uc.Store();
      }
      else
      {
        uc.WindowState = false;
        await uc.Store();
        uc.Width = 210;
        uc.Height = 80;
      }
  }

  async Task Store()
  {
    var jsonFile = @$"\temp\_{Name}_.json";
    await File.WriteAllTextAsync(jsonFile, JsonSerializer.Serialize(new LayoutVM2
    {
      Top = Canvas.GetTop(this),
      Left = Canvas.GetLeft(this),
      Width = Width,
      Height = Height,
      WindowState = WindowState
    }));
  }
  async Task RestorePlacementFromFile()
  {
    try
    {
      var jsonFile = @$"\temp\_{Name}_.json";
      var layout = !File.Exists(jsonFile) ? new LayoutVM2() : JsonSerializer.Deserialize<LayoutVM2>(await File.ReadAllTextAsync(jsonFile)) ?? new LayoutVM2();
      Canvas.SetTop(this, layout.Top);
      Canvas.SetLeft(this, layout.Left);
      Width = layout.Width < 1 ? 512 : layout.Width;
      Height = layout.Height < 1 ? 256 : layout.Height;
      if (!_isLoaded)
        WindowState = layout.WindowState;
    }
    catch (Exception ex)
    {
      WriteLine($"{ex.Message}");
      DataContext = new LayoutVM2();
      if (Debugger.IsAttached) Debugger.Break();
    }
  }

  public async void OnLoaded(object sender, RoutedEventArgs e) { await RestorePlacementFromFile(); _isLoaded = true; }
  public void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
  {
    isDragging = true;
    clickPosition = e.GetPosition(this);
    _ = ((sender as Border)?.CaptureMouse());
    Panel.SetZIndex(this, 111);
  }
  public async void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
  {
    try
    {
      isDragging = false;
      (sender as Border)?.ReleaseMouseCapture();
      Panel.SetZIndex(this, 11);
      await Store();
    }
    catch (Exception ex)
    {
      WriteLine($"{ex.Message}");
      if (Debugger.IsAttached) Debugger.Break();
    }
  }
  public void Border_MouseMove(object sender, MouseEventArgs e)
  {
    if (isDragging && sender is Border draggableControl)
    {
      var currentPosition = e.GetPosition(Parent as UIElement);

      var canvas = ((FrameworkElement)((FrameworkElement)draggableControl.Parent).Parent).Parent as Canvas;

      ArgumentNullException.ThrowIfNull(canvas, "▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄ Canvas ▀▄▀▄▀▄▀▄▀▄▀▄▀▄");

      var canvasWidth = canvas.ActualWidth;
      var canvasHeight = canvas.ActualHeight;

      // Calculate the new position of the usercontrol
      var newX = currentPosition.X - clickPosition.X;
      var newY = currentPosition.Y - clickPosition.Y;

      // Check if the new position is within the canvas bounds
      if (newX < 0)
        newX = 0;
      if (newX + ActualWidth > canvasWidth)
        newX = canvasWidth - ActualWidth;
      if (newY < 0)
        newY = 0;
      if (newY + ActualHeight > canvasHeight)
        newY = canvasHeight - ActualHeight;

      // Set the new position of the usercontrol
      SetValue(Canvas.LeftProperty, newX);
      SetValue(Canvas.TopProperty, newY);
    }
  }
  public void Rectng_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
  {
    _isResizing = true;
    _lastMousePosition = e.GetPosition(this);
    _ = Mouse.Capture((IInputElement)sender);
  }
  public async void Rectng_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
  {
    try
    {
      _isResizing = false;
      _ = Mouse.Capture(null);
      await Store();
    }
    catch (Exception ex)
    {
      WriteLine($"{ex.Message}");
      if (Debugger.IsAttached) Debugger.Break();
    }
  }
  public void Rectng_MouseMove(object sender, MouseEventArgs e)
  {
    if (_isResizing)
    {
      var currentPosition = e.GetPosition(this);
      var delta = currentPosition - _lastMousePosition;
      if (Width + delta.X > 0)
      Width += delta.X;
      //else
      //  Width = 500;

      if (Height + delta.Y > 0)
      Height += delta.Y;
      //else
      //  Height = 300;

      _lastMousePosition = currentPosition;
    }
  }
}
