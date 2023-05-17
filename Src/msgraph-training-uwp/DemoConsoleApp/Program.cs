using DemoLibrary;

var rv = await new AuthUsagePOC().LogInAsync();

Console.ForegroundColor = rv.success ? ConsoleColor.DarkGreen : ConsoleColor.Red;
Console.WriteLine($"{rv.report}");
Console.ResetColor();
Console.WriteLine($"\n\n\tThe End.");
