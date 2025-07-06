using DemoLibrary;

try
{
  Console.ForegroundColor = ConsoleColor.DarkYellow;
  Console.WriteLine(
    """
  2025-07-06  ?!?!?!?

  Pops up a dialog box like this:

  ---------------------------
  Microsoft Visual Studio
  ---------------------------
  Error launching

  WSL is not installed.
  Select OK to Learn More, or Cancel to abort.
  ---------------------------
  OK   Cancel   
  ---------------------------



  ---------------------------
  Microsoft Visual Studio
  ---------------------------
  Try running your application again once the script in the command window has completed. You may need to interact with the script.
  ---------------------------
  OK   
  ---------------------------
  

  Then it opens a browser to the WSL install page, which is not what I expected:  https://learn.microsoft.com/en-us/windows/wsl/install
  
  wsl --install
  """);


  Console.ForegroundColor = ConsoleColor.Yellow;
  Console.WriteLine("WAIT! It hangs the VS for a while ... but then comes out file");

  await Task.Delay(5000);
  return; // just to see the message above, then exit

  string clientId = "9ba0619e-3091-40b5-99cb-c2aca4abd04e"; 
  var rv = await new AuthUsagePOC().LogInAsync(clientId);

  Console.ForegroundColor = rv.success ? ConsoleColor.DarkGreen : ConsoleColor.Red;
  Console.WriteLine($"{rv.report}");
  Console.ResetColor();
  Console.WriteLine($"\n\n\tThe End.");

}
catch (Exception ex)
{
  Console.ForegroundColor = ConsoleColor.Red;
  Console.WriteLine($"Exception: {ex.Message}");
}
finally {   
  Console.ResetColor();
}