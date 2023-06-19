using Microsoft.Identity.Client.Extensions.Msal;

namespace DemoLibrary;

internal static class AppSettings // 
{
  public static readonly string[] Scopes = new[] { "user.read" }; //, "Files.Read" };     //todo: worked on Nuc2 with this on

  public const string Authority = "https://login.microsoftonline.com/common";

  // Cache settings
  public static readonly string CacheDir = MsalCacheHelper.UserRootDirectory;
  public const string CacheFileName = "myapp_msal_cache.txt";
  public const string CacheFileNam2 = "myapp_msal_cache.plaintext.txt"; // do not use the same file name so as not to overwrite the encypted version

  public const string KeyChainServiceName = "myapp_msal_service";
  public const string KeyChainAccountName = "myapp_msal_account";

  public const string LinuxKeyRingSchema = "com.contoso.devtools.tokencache";
  public const string LinuxKeyRingCollection = MsalCacheHelper.LinuxKeyRingDefaultCollection;
  public const string LinuxKeyRingLabel = "MSAL token cache for all AAVpro dev tool apps.";
  public static readonly KeyValuePair<string, string> LinuxKeyRingAttr1 = new("Version", "1");
  public static readonly KeyValuePair<string, string> LinuxKeyRingAttr2 = new("ProductGroup", "MyApps");
}