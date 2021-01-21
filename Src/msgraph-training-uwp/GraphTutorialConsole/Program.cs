using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace GraphTutorial
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine(".NET Core Graph Tutorial\n");

      var appConfig = LoadAppSettings();

      if (appConfig == null)
      {
        Console.WriteLine("Missing or invalid appsettings.json...exiting");
        return;
      }

      var appId = appConfig["appId"];
      var scopesString = appConfig["scopes"];
      var scopes = scopesString.Split(';');

      // Initialize the auth provider with values from appsettings.json
      var authProvider = new DeviceCodeAuthProvider(appId, scopes);

      // Request a token to sign in the user
      var accessToken = authProvider.GetAccessToken().Result;


      // Initialize Graph client
      GraphHelper.Initialize(authProvider);

      // Get signed in user
      var user = GraphHelper.GetMeAsync().Result;
      Console.WriteLine($"Welcome {user.DisplayName}!\n");


      int choice = -1;

      while (choice != 0)
      {
        Console.WriteLine("Please choose one of the following options:");
        Console.WriteLine("0. Exit");
        Console.WriteLine("1. Display access token");
        Console.WriteLine("2. View this week's calendar");
        Console.WriteLine("3. Add an event");

        try
        {
          choice = int.Parse(Console.ReadLine());
        }
        catch (System.FormatException)
        {
          // Set to invalid value
          choice = -1;
        }

        switch (choice)
        {
          case 0:
            // Exit the program
            Console.WriteLine("Goodbye...");
            break;
          case 1:
            // Display access token
            Console.WriteLine($"Access token: {accessToken}\n");
            break;
          case 2:
            // List the calendar
            ListCalendarEvents(
    user.MailboxSettings.TimeZone,
    $"{user.MailboxSettings.DateFormat} {user.MailboxSettings.TimeFormat}"
);
            break;
          case 3:
            // Create a new event
            CreateEvent(user.MailboxSettings.TimeZone);
            break;
          default:
            Console.WriteLine("Invalid choice! Please try again.");
            break;
        }
      }
    }

    static IConfigurationRoot LoadAppSettings()
    {
      var appConfig = new ConfigurationBuilder()
          .AddUserSecrets<Program>()
          .Build();

      // Check for required settings
      if (string.IsNullOrEmpty(appConfig["appId"]) ||
          string.IsNullOrEmpty(appConfig["scopes"]))
      {
        return null;
      }

      return appConfig;
    }

    static string FormatDateTimeTimeZone(
      Microsoft.Graph.DateTimeTimeZone value,
      string dateTimeFormat)
    {
      // Parse the date/time string from Graph into a DateTime
      var dateTime = DateTime.Parse(value.DateTime);

      return dateTime.ToString(dateTimeFormat);
    }


    static void ListCalendarEvents(string userTimeZone, string dateTimeFormat)
    {
      var events = GraphHelper
          .GetCurrentWeekCalendarViewAsync(DateTime.Today, userTimeZone)
          .Result;

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
      {
        confirm = Console.ReadKey(true);
      }
      while (confirm.Key != ConsoleKey.Y && confirm.Key != ConsoleKey.N);

      Console.WriteLine();
      return (confirm.Key == ConsoleKey.Y);
    }

    static string GetUserInput(
        string fieldName,
        bool isRequired,
        Func<string, bool> validate)
    {
      string returnValue = null;
      do
      {
        Console.Write($"Enter a {fieldName}: ");
        if (!isRequired)
        {
          Console.Write("(ENTER to skip) ");
        }
        var input = Console.ReadLine();

        if (!string.IsNullOrEmpty(input))
        {
          if (validate.Invoke(input))
          {
            returnValue = input;
          }
        }
      }
      while (string.IsNullOrEmpty(returnValue) && isRequired);

      return returnValue;
    }

    static void CreateEvent(string userTimeZone)
    {
      // Prompt user for info

      // Require a subject
      var subject = GetUserInput("subject", true,
          (input) => {
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
              (input) => {
                return GetUserYesNo($"{input} - add attendee?");
              });

          if (!string.IsNullOrEmpty(attendee))
          {
            attendeeList.Add(attendee);
          }
        }
        while (!string.IsNullOrEmpty(attendee));
      }

      var startString = GetUserInput("event start", true,
          (input) => {
          // Validate that input is a date
          return (DateTime.TryParse(input, out var result));
          });

      var start = DateTime.Parse(startString);

      var endString = GetUserInput("event end", true,
          (input) => {
          // Validate that input is a date
          // and is later than start
          return (DateTime.TryParse(input, out var result) &&
              result.CompareTo(start) > 0);
          });

      var end = DateTime.Parse(endString);

      var body = GetUserInput("body", false,
          (input => { return true; }));

      Console.WriteLine($"Subject: {subject}");
      Console.WriteLine($"Attendees: {string.Join(";", attendeeList)}");
      Console.WriteLine($"Start: {start.ToString()}");
      Console.WriteLine($"End: {end.ToString()}");
      Console.WriteLine($"Body: {body}");
      if (GetUserYesNo("Create event?"))
      {
        GraphHelper.CreateEvent(
            userTimeZone,
            subject,
            start,
            end,
            attendeeList,
            body).Wait();
      }
      else
      {
        Console.WriteLine("Canceled.");
      }
    }
  }
}

/*
.NET Core Graph Tutorial

To sign in, use a web browser to open the page https://microsoft.com/devicelogin and enter the code EHYZCBKE4 to authenticate.
Welcome Alex Pigida!

Please choose one of the following options:
0. Exit
1. Display access token
2. View this week's calendar
3. Add an event
3
Enter a subject: Test from Console
Subject: Test from Console - is that right? (y/n)
Do you want to invite attendees? (y/n)
Enter a event start: 14:00
Enter a event end: 15:00
Enter a body: (ENTER to skip) event body goes here...
Subject: Test from Console
Attendees:
Start: 2021-01-21 14:00:00
End: 2021-01-21 15:00:00
Body: event body goes here...
Create event? (y/n)
Event added to calendar.
Please choose one of the following options:
0. Exit
1. Display access token
2. View this week's calendar
3. Add an event

Invalid choice! Please try again.
Please choose one of the following options:
0. Exit
1. Display access token
2. View this week's calendar
3. Add an event
2
Events:
Subject: MS Graph - testing
  Organizer: alex.pigida@outlook.com
  Start: 2021-01-21 12:30
  End: 2021-01-21 13:00
Subject: Test from Console
  Organizer: Alex Pigida
  Start: 2021-01-21 14:00
  End: 2021-01-21 15:00
Please choose one of the following options:
0. Exit
1. Display access token
2. View this week's calendar
3. Add an event
1
Access token: EwCIA8l6BAAU6k7+XVQzkGyMv7VHB/h4cHbJYRAAAaGXGmbfVmGP4ayUUdW5Hm182VrEXaq2R74mxzKtXmNX9GDS4OmFbcq/X+XLxCa+LKjlOipJjAGUT7HTpBzqTjlBvTHNnibxTzBc/LwpoWhoJB93s8BmVz5gmQ7kiXEi/xXHn810kjA05QF+UFxeEuMD0T/Rd646jZaw2ezFkcgMA9K5V6a4DbIteQTgZSAnqYleS00GvoCInQxxFUeOkilEZnyGGu7ZqyUPn6v2U33Kp2uUC2gK/pQfIettGknOIdOi8DGrUkdsC6liW1lcST5cUaj5rCxQMFoWJjmcIngE+jaKwCdhkRAjxnDFnoY3oIwpv8aAcg+XknvnAW6eiRoDZgAACDhzYrMNUV8mWAKjKwe3IiAmzfQXsV8QVCNWMHreZv6oUblOqLXXXJRgDEtuHeR2yFxNEbA899g0LMwVa+dbY7pZGBS4QijJxq3zVV318MIrtqrOHR6V1aOKBjXK/MCb+oAcRxTkcZegx/RzH97frMXR7Mi16FSgwoudVOf1qyoCkAItrdcaOH/GsmGxEICMKu7DywhcKuinLgJdhO/y1/KzaKEHmprKVJDYEK8Q+ZodtlsoiW/wI8S77guD2EJdsIIYGJRCAQwO7jH+AAMFuGqA012FcpNmDKI033XUtNxrdt9RC3WyNG59QOKC2Bd8aWEhMs/d082w8n3wp22BAchlG79/+mAchQP5erPggLn+Pt6syl+adz+WLMIAIdQS7I/PjGHaaWRjOWgLUrmCuA3Ja+cT1qKa0/9awye1JIqoTN16D9vMLmWSkpuZPND/JHKjxrAmiKa1807bnqFRX+1lb48HrWbY8Jj4hJNI/0AdiEUvZXExnzbAosm/aDCrDWOsa9Nve4dfP17y0nutXWSUoLYC6lYI/sgDNEJemTB0DDlElSHWcVrC17aFC3NmnOAMIwAKxzZGR12ecf210bJLbFmNYJHrj1qlHzmwKeRkIgegN9FwbTFxphob7d/a6TlKfOP89i+jXwQjM9ATw1SjGTB/MVstT96KPyCEXlWb2Y5T9rvFzKzlYplS4AhACUKgaCgbHWlpxR+Qc/ljjifqGEv76QKjZjaCIJgHK+KqKbz1a34r/CzFembkAjbhBexEedVXq8KZHmt34DD8HuljmJdJKDqQlPsu45fKQLw5immNAg==

Please choose one of the following options:
0. Exit
1. Display access token
2. View this week's calendar
3. Add an event
*/