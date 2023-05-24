using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DemoApp;
public partial class App : Application
{
  protected override void OnStartup(StartupEventArgs e)
  {
    base.OnStartup(e);

    var listener = new TextWriterTraceListener(@"c:\temp\logs\MSGraphSlideshowApp.log"); // { Filter = new ErrorFilter() };
    Trace.Listeners.Add(listener);
    Trace.AutoFlush = true;
    Trace.WriteLine("\r\n"); // between-runs separator. 2023-04-22
  }
}
