﻿using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using static System.Diagnostics.Trace;

namespace DemoLibrary;
public class AuthUsagePOC
{
  IPublicClientApplication? _msalClient;

  public async Task<(bool success, string report, AuthenticationResult? result)> LogInAsync(string clientId)
  {
    if (string.IsNullOrWhiteSpace(clientId))
      return (false, "op: check cfg flow for ClientId!!!", null);

    try
    {
      // It's recommended to create a separate PublicClient Application for each tenant but only one CacheHelper object
      var appBuilder = PublicClientApplicationBuilder.Create(clientId)
            //.WithAuthority("https://login.microsoftonline.com/common")
            .WithAuthority(AzureCloudInstance.AzurePublic, tenant: "common")
            //.WithDefaultRedirectUri();
            //.WithRedirectUri($"ms-appx-web://microsoft.aad.brokerplugin/{clientId}"); // 2024-08: expired and failed to launch the interactive login, but the http://localhost worked for alx:
            .WithRedirectUri("http://localhost"); // make sure to register this redirect URI for the interactive login to work (at https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/~/Authentication/appId/9ba0619e-3091-40b5-99cb-c2aca4abd04e/isMSAApp~/false)

      //appBuilder.WithWindowsBrokerOptions(new WindowsBrokerOptions() { HeaderText= "▄▀▄▀▄▀▄▀", ListWindowsWorkAndSchoolAccounts=false });
      //appBuilder.WithBroker(true);

      _msalClient = appBuilder.Build();

      var cacheHelper = await CreateCacheHelperAsync(clientId).ConfigureAwait(false);
      cacheHelper.RegisterCache(_msalClient.UserTokenCache);

      //tmi: WriteLine("Token Acquisition: getting all the accounts; this reads the cache...");
      var accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);

      AuthenticationResult authResult;
      try
      {
        authResult = await _msalClient.AcquireTokenSilent(AppSettings.Scopes, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(false); // this is expected to fail when account is null
      }
      catch (MsalUiRequiredException ex)
      {
        WriteLine($"■ Error Acquiring Token Silently ==> going interactive...    MsalUiRequiredException:\n  {ex.Message}"); // A MsalUiRequiredException happened on AcquireTokenSilent. This indicates you need to call AcquireTokenInteractive to acquire a token

        try
        {
          authResult = await _msalClient.AcquireTokenInteractive(AppSettings.Scopes)
              .WithAccount(accounts.FirstOrDefault())
              .WithPrompt(Prompt.SelectAccount)
              .ExecuteAsync();
        }
        catch (MsalException msalEx)
        {
          WriteLine($"■ Error Acquiring Token INTERACTIVELY:   \n  {msalEx.Message}");
          return (false, $"■ Error Acquiring Token INTERACTIVELY:   {msalEx}", null);
        }
      }
      catch (Exception ex) { return (false, $"Error Acquiring Token Silently:   {ex}", null); }

      return (true, ReportResult(authResult), authResult);
    }
    catch (Exception ex) { return (false, $"Error Acquiring Token Silently:   {ex}", null); }
  }

  public async Task<string> SignOut()
  {
    if (_msalClient is null) throw new InvalidOperationException("PublicClientApp is null");

    var accounts = await _msalClient.GetAccountsAsync();
    if (accounts?.Any() == true)
    {
      try
      {
        await _msalClient.RemoveAsync(accounts.FirstOrDefault());
        return "User has signed-out";
      }
      catch (MsalException ex)
      {
        return $"Error signing-out user: {ex.Message}";
      }
    }
    else
    {
      return "No accounts to sign-out";
    }
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