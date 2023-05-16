using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GraphTutorialConsole2;

public class Settings
{
  public string? ClientId { get; set; }
  public string? TenantId { get; set; }
  public string[]? GraphUserScopes { get; set; }

  public static Settings LoadSettings()
  {
    // Load settings
    IConfiguration config = new ConfigurationBuilder()
        //.AddJsonFile("appsettings.json", optional: false)        // appsettings.json is required
        //.AddJsonFile($"appsettings.Development.json", optional: true)        // appsettings.Development.json" is optional, values override appsettings.json
        .AddUserSecrets<Program>()        // User secrets are optional, values override both JSON files
        .Build();

    return config.GetRequiredSection("Settings").Get<Settings>() ??
        throw new Exception("Could not load app settings. See README for configuration instructions.");
  }
}

/*
 User token: 

EwCQA8l6BAAUAOyDv0l6PcCVu89kmzvqZmkWABkAAd5bzTWxiLfS9QJRwEp3ZflAeNMj77kva3eP/WupMr/L67IcUBEBFENttWi2JzenC0dghUqkeGV1CE7E3IhmBbFAYWjVy2GGKoE+S6L/FDKBZspF2ASYgvNJNEaofDO4g9rD797+34iIHO1/cgfjAirKgWpWHz2XpRGNouGWVjymSkaNlayUKBY5wUhLpq1XtJQ+EsQbCnaYdbbVpVuiFgaZ5mQpRLlXQft/JlxmZPArZTLCRGB01F8VQNdOGb6Upe7Yc92eHxW+JTDyPFpI4/8NPmq2mPDy/FB4v2+/BWu69R+h6iHkL88+doj2WfuECmpAdqAmmyhPUf3SEwFn5koDZgAACG11EAA8b61xYAJWlx+WxyQh5kG796BqiDfikve6rjw9BH4hXcNhrXTkHB1ek2zgQ7HNxm+wFKjxbii1vTIv+O6fkCTs3HA4OANpkecyDU8ae3Le5Sgadgcyb5R4mScMGucRixO4ed/2D89WXvSn1Mlm/Zjji5erslVZNvKcY0FBj8v+UMmOnERwNhznG3TYJGGkx1oBLcbxb2abOyorfohaJ2pVnRS3J3uMJeBwJfCTSExAeCs0plsc7f/ebAUTbkmdRlNdTMm3iHGlD84RnQaNjmBgtLJrT3XzoCqpmISc0LModKpYCnqb3OXk25KGm6ZzWFizTanjdCMGoiSOCUp7axXwQJcsER79TUOF7KDC926Pr0UnLG4XY8lJPoSmHKfKMX39dPi56O0P7u+Wa4Nc18/rHnjDQKx98ZOBiFG7nRDs0UoL1bGW0QDVWLEJRzPXemtaP9BZAgfsDAXSueGCL90erJ+/Vh1Az5AX7kQzJeRwhnd7f89NgdgX09cJnqF7E9Of/i/APOLpwzu70ZPRE+Zscv+yl4J6P2107uF6LVu1a5uslDNbT4nmB8295XfLdQu0xi5gRJCXGgQLe4IgEcUIRc9Mcd6+YHY34f5OsAwbZon5IaJVqXB7DRP3Cet4qc8GlSP0g01EiQzabCgv95OJPSYAxE46p47CcZ9NwaJLFkomMa9DKITjjE88xcBIeSxYr4qLpMob2X4MwWjcarYk103zKWbhSaEB8WA3g6vjlHb0mz9qwDJHbuXMTSDHZIT4aIeHmnd0KjFtci41aDVQ7/t6fOaY7OuWbm4AT7lu/4XCo4ESK48C

Please choose one of the following options:
0. Exit*/