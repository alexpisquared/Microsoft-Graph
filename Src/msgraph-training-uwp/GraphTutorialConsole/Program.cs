using System;
using System.Collections.Generic;
using GraphTutorial;
using Microsoft.Extensions.Configuration;

namespace GraphTutorialConsole;

class Program
{
  static void Main(string[] args)
  {
    Console.WriteLine(".NET Core Graph Tutorial\n");

    var appConfig = new ConfigurationBuilder().AddUserSecrets<Program>().Build(); //tu: secrets by the book. LoadAppSettings();
    var appId = appConfig["appId"];
    var scopes = appConfig["scopes"].Split(';');

    var authProvider = new DeviceCodeAuthProvider(appId, scopes); // Initialize the auth provider with values from appsettings.json
    var accessToken0 = appConfig["AccessToken"];
    var accessToken = accessToken0 ?? authProvider.GetAccessToken().Result;       // Request a token to sign in the user

    GraphHelper.Initialize(authProvider);

    var user = GraphHelper.GetMeAsync().Result;
    Console.WriteLine($"Welcome {user?.DisplayName ?? "NUL"}!\n");

    var choice = -1;

    while (choice != 0)
    {
      Console.WriteLine("Please choose one of the following options:");
      Console.WriteLine("0. Exit");
      Console.WriteLine("1. Display access token");
      Console.WriteLine("2. View this week's calendar");
      Console.WriteLine("3. Add an event");
      Console.WriteLine("4. List PHOTO!!!!!!!!!!!!!");

      try
      {
        choice = int.Parse(Console.ReadLine());
      }
      catch (FormatException)
      {
        // Set to invalid value
        choice = -1;
      }

      switch (choice)
      {
        case 0: Console.WriteLine("Goodbye..."); break;
        case 1: Console.WriteLine($"Access token: {accessToken}\n"); break;
        case 2: ListCalendarEvents(user.MailboxSettings.TimeZone, $"{user.MailboxSettings.DateFormat} {user.MailboxSettings.TimeFormat}"); break;
        case 4: ListPhotos(user.MailboxSettings.TimeZone, $"{user.MailboxSettings.DateFormat} {user.MailboxSettings.TimeFormat}"); break;
        case 3: CreateEvent(user.MailboxSettings.TimeZone); break;
        default: Console.WriteLine("Invalid choice! Please try again."); break;
      }
    }
  }

  static IConfigurationRoot LoadAppSettings()
  {
    var appConfig = new ConfigurationBuilder().AddUserSecrets<Program>().Build(); //tu: secrets by the book.

    return string.IsNullOrEmpty(appConfig["appId"]) || string.IsNullOrEmpty(appConfig["scopes"]) ? null : appConfig;
  }

  static string FormatDateTimeTimeZone(Microsoft.Graph.DateTimeTimeZone value, string dateTimeFormat)
  {
    // Parse the date/time string from Graph into a DateTime
    var dateTime = DateTime.Parse(value.DateTime);

    return dateTime.ToString(dateTimeFormat);
  }

  static void ListCalendarEvents(string userTimeZone, string dateTimeFormat)
  {
    var events = GraphHelper.GetCurrentWeekCalendarViewAsync(DateTime.Today, userTimeZone).Result;

    Console.WriteLine("Events:");

    foreach (var calendarEvent in events)
    {
      Console.WriteLine($"Subject: {calendarEvent.Subject}");
      Console.WriteLine($"  Organizer: {calendarEvent.Organizer.EmailAddress.Name}");
      Console.WriteLine($"  Start: {FormatDateTimeTimeZone(calendarEvent.Start, dateTimeFormat)}");
      Console.WriteLine($"  End: {FormatDateTimeTimeZone(calendarEvent.End, dateTimeFormat)}");
    }
  }
  static void ListPhotos(string userTimeZone, string dateTimeFormat)
  {
    var events = GraphHelper.GetPhotoAsync(DateTime.Today, userTimeZone).Result;

    Console.WriteLine("Events:");

    foreach (var calendarEvent in events)
    {
      Console.WriteLine($"Subject: {calendarEvent.Subject}");
      Console.WriteLine($"  Organizer: {calendarEvent.Organizer.EmailAddress.Name}");
      Console.WriteLine($"  Start: {FormatDateTimeTimeZone(calendarEvent.Start, dateTimeFormat)}");
      Console.WriteLine($"  End: {FormatDateTimeTimeZone(calendarEvent.End, dateTimeFormat)}");
    }
  }

  static bool GetUserYesNo(string prompt)
  {
    Console.Write($"{prompt} (y/n)");
    ConsoleKeyInfo confirm;
    do
      confirm = Console.ReadKey(true);
    while (confirm.Key is not ConsoleKey.Y and not ConsoleKey.N);

    Console.WriteLine();
    return confirm.Key == ConsoleKey.Y;
  }

  static string GetUserInput(string fieldName, bool isRequired, Func<string, bool> validate)
  {
    string returnValue = null;
    do
    {
      Console.Write($"Enter a {fieldName}: ");
      if (!isRequired)
        Console.Write("(ENTER to skip) ");
      var input = Console.ReadLine();

      if (!string.IsNullOrEmpty(input))
        if (validate.Invoke(input))
          returnValue = input;
    }
    while (string.IsNullOrEmpty(returnValue) && isRequired);

    return returnValue;
  }

  static void CreateEvent(string userTimeZone)
  {
    // Prompt user for info

    // Require a subject
    var subject = GetUserInput("subject", true,
        (input) =>
        {
          return GetUserYesNo($"Subject: {input} - is that right?");
        });

    // Attendees are optional
    var attendeeList = new List<string>();
    if (GetUserYesNo("Do you want to invite attendees?"))
    {
      string attendee = null;

      do
      {
        attendee = GetUserInput("attendee", false,
            (input) =>
            {
              return GetUserYesNo($"{input} - add attendee?");
            });

        if (!string.IsNullOrEmpty(attendee))
          attendeeList.Add(attendee);
      }
      while (!string.IsNullOrEmpty(attendee));
    }

    var startString = GetUserInput("event start", true,
        (input) =>
        {
          // Validate that input is a date
          return DateTime.TryParse(input, out var result);
        });

    var start = DateTime.Parse(startString);

    var endString = GetUserInput("event end", true,
        (input) =>
        {
          // Validate that input is a date
          // and is later than start
          return DateTime.TryParse(input, out var result) &&
              result.CompareTo(start) > 0;
        });

    var end = DateTime.Parse(endString);

    var body = GetUserInput("body", false,
        input => { return true; });

    Console.WriteLine($"Subject: {subject}");
    Console.WriteLine($"Attendees: {string.Join(";", attendeeList)}");
    Console.WriteLine($"Start: {start}");
    Console.WriteLine($"End: {end}");
    Console.WriteLine($"Body: {body}");
    if (GetUserYesNo("Create event?"))
      GraphHelper.CreateEvent(
          userTimeZone,
          subject,
          start,
          end,
          attendeeList,
          body).Wait();
    else
      Console.WriteLine("Canceled.");
  }
}