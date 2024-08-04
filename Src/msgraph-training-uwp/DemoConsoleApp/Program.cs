using DemoLibrary;

string clientId = "??";
var rv = await new AuthUsagePOC().LogInAsync(clientId);

Console.ForegroundColor = rv.success ? ConsoleColor.DarkGreen : ConsoleColor.Red;
Console.WriteLine($"{rv.report}");
Console.ResetColor();
Console.WriteLine($"\n\n\tThe End.");
