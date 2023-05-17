using Microsoft.Identity.Client.Extensions.Msal;

namespace TokenCaching;

internal static class Config
{
  // App settings
  public static readonly string[] Scopes = new[] { "user.read" };

  public const string Authority = "https://login.microsoftonline.com/common";

  public const string ClientId = "9ba0619e-3091-40b5-99cb-c2aca4abd04e";

  // Cache settings
  public const string CacheFileName = "myapp_msal_cache.txt";
  public static readonly string CacheDir = MsalCacheHelper.UserRootDirectory;

  public const string KeyChainServiceName = "myapp_msal_service";
  public const string KeyChainAccountName = "myapp_msal_account";

  public const string LinuxKeyRingSchema = "com.contoso.devtools.tokencache";
  public const string LinuxKeyRingCollection = MsalCacheHelper.LinuxKeyRingDefaultCollection;
  public const string LinuxKeyRingLabel = "MSAL token cache for all Contoso dev tool apps.";
  public static readonly KeyValuePair<string, string> LinuxKeyRingAttr1 = new("Version", "1");
  public static readonly KeyValuePair<string, string> LinuxKeyRingAttr2 = new("ProductGroup", "MyApps");

  // For Username / Password flow - to be used only for testing!
  public const string Username = "liu.kang@bogavrilltd.onmicrosoft.com";
  public const string Password = "";
}
