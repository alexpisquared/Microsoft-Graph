using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using static System.Diagnostics.Trace;

namespace DemoLibrary;
public class AuthUsagePOC
{
  public async Task<(bool success, string report, AuthenticationResult? result)> LogInAsync(string clientId)
  {
    try
    {
      // It's recommended to create a separate PublicClient Application for each tenant but only one CacheHelper object
      var appBuilder = PublicClientApplicationBuilder.Create(clientId)
          .WithAuthority(AzureCloudInstance.AzurePublic, tenant: "common")
          .WithRedirectUri("http://localhost"); // make sure to register this redirect URI for the interactive login to work
      var pca1 = appBuilder.Build();

      var cacheHelper = await CreateCacheHelperAsync(clientId).ConfigureAwait(false);
      cacheHelper.RegisterCache(pca1.UserTokenCache);

      //tmi: WriteLine("Token Acquisition: getting all the accounts; this reads the cache...");
      var accounts = await pca1.GetAccountsAsync().ConfigureAwait(false);

      AuthenticationResult authResult;
      try
      {
        authResult = await pca1.AcquireTokenSilent(AppSettings.Scopes, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(false); // this is expected to fail when account is null
      }
      catch (MsalUiRequiredException ex)
      {
        WriteLine($"Error Acquiring Token Silently ==> going interactive...    MsalUiRequiredException: {ex.Message}"); // A MsalUiRequiredException happened on AcquireTokenSilent. This indicates you need to call AcquireTokenInteractive to acquire a token

        try
        {
          authResult = await pca1.AcquireTokenInteractive(AppSettings.Scopes)
              .WithAccount(accounts.FirstOrDefault())
              .WithPrompt(Prompt.SelectAccount)
              .ExecuteAsync();
        }
        catch (MsalException msalex) { return (false, $"Error Acquiring Token INTERACTIVELY:   {msalex}", null); }
      }
      catch (Exception ex) { return (false, $"Error Acquiring Token Silently:   {ex}", null); }

      return (true, ReportResult(authResult), authResult);
    }
    catch (Exception ex) { return (false, $"Error Acquiring Token Silently:   {ex}", null); }
  }

  static string ReportResult(AuthenticationResult result)
  {
    return $""" 
  Got a token for:  {result.Account.Username} 
     Token source:  {result.AuthenticationResultMetadata.TokenSource}     
       Expires on:  {result.ExpiresOn.ToLocalTime()}     
       {result.AccessToken[..12]}...{result.AccessToken[^12..]}
 """;
  }

  static async Task<MsalCacheHelper> CreateCacheHelperAsync(string clientId)
  {
    try
    {
      var storageProperties = new StorageCreationPropertiesBuilder(AppSettings.CacheFileName, AppSettings.CacheDir)
        .WithLinuxKeyring(
          AppSettings.LinuxKeyRingSchema,
          AppSettings.LinuxKeyRingCollection,
          AppSettings.LinuxKeyRingLabel,
          AppSettings.LinuxKeyRingAttr1,
          AppSettings.LinuxKeyRingAttr2)
        .WithMacKeyChain(
          AppSettings.KeyChainServiceName,
          AppSettings.KeyChainAccountName)
        .WithCacheChangedEvent( // do NOT use unless really necessary, high perf penalty!
          clientId,
          AppSettings.Authority).Build();

      var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties).ConfigureAwait(false);
      cacheHelper.VerifyPersistence();
      return cacheHelper;
    }
    catch (MsalCachePersistenceException e)
    {
      WriteLine($"WARNING! Unable to encrypt tokens at rest. Saving tokens in plaintext at {Path.Combine(AppSettings.CacheDir, AppSettings.CacheFileName)} ! Please protect this directory or delete the file after use.\n  Encryption exception: " + e);

      var storageProperties = new StorageCreationPropertiesBuilder(AppSettings.CacheFileNam2, AppSettings.CacheDir).WithUnprotectedFile().Build();

      var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties).ConfigureAwait(false);
      cacheHelper.VerifyPersistence();
      return cacheHelper;
    }
  }
}