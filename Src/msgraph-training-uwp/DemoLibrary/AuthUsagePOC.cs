using System.Diagnostics;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace DemoLibrary;
public class AuthUsagePOC
{
  public async Task<(bool success, string report, AuthenticationResult? result)> LogInAsync()
  {
    try
    {
      // It's recommended to create a separate PublicClient Application for each tenant but only one CacheHelper object
      var appBuilder = PublicClientApplicationBuilder.Create(Config.ClientId)
          .WithAuthority(AzureCloudInstance.AzurePublic, tenant: "common")
          .WithRedirectUri("http://localhost"); // make sure to register this redirect URI for the interactive login to work
      var pca1 = appBuilder.Build();

      var cacheHelper = await CreateCacheHelperAsync().ConfigureAwait(false);
      cacheHelper.RegisterCache(pca1.UserTokenCache);

      Debug.WriteLine("    Getting all the accounts. This reads the cache.");
      var accounts = await pca1.GetAccountsAsync().ConfigureAwait(false);

      AuthenticationResult result;
      try
      {
        result = await pca1.AcquireTokenSilent(Config.Scopes, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(false); // this is expected to fail when account is null
      }
      catch (MsalUiRequiredException ex)
      {
        Debug.WriteLine($"MsalUiRequiredException: {ex.Message}"); // A MsalUiRequiredException happened on AcquireTokenSilent. This indicates you need to call AcquireTokenInteractive to acquire a token

        try
        {
          result = await pca1.AcquireTokenInteractive(Config.Scopes)
              .WithAccount(accounts.FirstOrDefault())
              .WithPrompt(Prompt.SelectAccount)
              .ExecuteAsync();
        }
        catch (MsalException msalex) { return (false, $"Error Acquiring Token:   {msalex}", null); }
      }
      catch (Exception ex) { return (false, $"Error Acquiring Token Silently:   {ex}", null); }

      return (true, DisplayResult(result), result);
    }
    catch (Exception ex) { return (false, $"Error Acquiring Token Silently:   {ex}", null); }
  }

  string DisplayResult(AuthenticationResult result)
  {
    return
    $"    Token Acquisition:  Success! \n" +
    $"      Got a token for:  {result.Account.Username} \n" +
    $"         Token source:  {result.AuthenticationResultMetadata.TokenSource} \n" +
    $"           Expires on:  {result.ExpiresOn.ToLocalTime()}     {result.AccessToken[..12]}...{result.AccessToken[^12..]} \n";
  }

  async Task<MsalCacheHelper> CreateCacheHelperAsync()
  {
    try
    {
      var storageProperties = new StorageCreationPropertiesBuilder(Config.CacheFileName, Config.CacheDir)
        .WithLinuxKeyring(
          Config.LinuxKeyRingSchema,
          Config.LinuxKeyRingCollection,
          Config.LinuxKeyRingLabel,
          Config.LinuxKeyRingAttr1,
          Config.LinuxKeyRingAttr2)
        .WithMacKeyChain(
          Config.KeyChainServiceName,
          Config.KeyChainAccountName)
        .WithCacheChangedEvent( // do NOT use unless really necessary, high perf penalty!
          Config.ClientId,
          Config.Authority).Build();

      var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties).ConfigureAwait(false);
      cacheHelper.VerifyPersistence();
      return cacheHelper;
    }
    catch (MsalCachePersistenceException e)
    {
      Debug.WriteLine($"WARNING! Unable to encrypt tokens at rest. Saving tokens in plaintext at {Path.Combine(Config.CacheDir, Config.CacheFileName)} ! Please protect this directory or delete the file after use.\n  Encryption exception: " + e);

      var storageProperties = new StorageCreationPropertiesBuilder(Config.CacheFileNam2, Config.CacheDir).WithUnprotectedFile().Build();

      var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties).ConfigureAwait(false);
      cacheHelper.VerifyPersistence();
      return cacheHelper;
    }
  }
}

internal static class Config // App settings
{
  public static readonly string[] Scopes = new[] { "user.read" };

  public const string Authority = "https://login.microsoftonline.com/common";

  public const string ClientId = "9ba0619e-3091-40b5-99cb-c2aca4abd04e";

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