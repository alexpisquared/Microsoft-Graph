﻿using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace TokenCaching;

/// <summary>
/// This advanced console app uses MSAL with the token cache based on Config.cs to show how various MSAL flows work.
/// If you are new to this sample, please look at Config and ExampleUsage 
/// </summary>
class Program
{
  static async Task Main(string[] args)
  {
    var pca1 = CreatePublicClient("https://login.microsoftonline.com/common"); // It's recommended to create a separate PublicClient Application for each tenant but only one CacheHelper object
    var cacheHelper = await CreateCacheHelperAsync().ConfigureAwait(false);
    cacheHelper.RegisterCache(pca1.UserTokenCache);

    // Advanced scenario for when 2 or more apps share the same cache             
    cacheHelper.CacheChanged += (s, e) => // this event is very expensive perf wise
    {
      Console.BackgroundColor = ConsoleColor.DarkRed;
      Console.WriteLine($"  Cache Changed,   Added: {e.AccountsAdded.Count()} Removed: {e.AccountsRemoved.Count()}  ");
      Console.ResetColor();
    };

    AuthenticationResult result;

    while (true)
    {
      Console.ResetColor();
      Console.WriteLine($@"
              1. Acquire Token using Username and Password - for TEST only, do not use in production!
              2. Acquire Token using Device Code Flow
              3. Acquire Token Interactive
              4. Acquire Token Silent
              5. Display Accounts (reads the cache)
              6. Acquire Token U/P and Silent in a loop                REMOVES FROM CACHE.        
              7. Use persistence layer to read / write any data
              8. Use persistence layer to read / write any data with process-level lock
              c. Clear cache
              e. Expire Access Tokens (TEST only!)
              x. Exit app ");
      Console.ForegroundColor = ConsoleColor.Blue;

      try
      {
        switch (Console.ReadKey().KeyChar)
        {
          case '1': //  Acquire Token using Username and Password (requires config)             // IMPORTANT: you should ALWAYS try to get a token silently first
            result = await AcquireTokenROPCAsync(pca1).ConfigureAwait(false);
            DisplayResult(result);
            break;

          case '2': // Device Code Flow                                                         // IMPORTANT: you should ALWAYS try to get a token silently first
            result = await pca1.AcquireTokenWithDeviceCode(Config.Scopes, (dcr) =>
            {
              Console.WriteLine(dcr.Message);
              return Task.FromResult(1);
            }).ExecuteAsync().ConfigureAwait(false);
            DisplayResult(result);

            break;
          case '3': // Interactive                                  // IMPORTANT: you should ALWAYS try to get a token silently first
            {
              var accounts = await pca1.GetAccountsAsync().ConfigureAwait(false);

              result = await pca1.AcquireTokenInteractive(Config.Scopes)
                  .WithAccount(accounts.FirstOrDefault())
                  .WithPrompt(Prompt.SelectAccount)
                  .ExecuteAsync()
                  .ConfigureAwait(false);
              DisplayResult(result);
            }
            break;
          case '4': // Silent
            {
              Console.WriteLine("Getting all the accounts. This reads the cache");
              var accounts = await pca1.GetAccountsAsync().ConfigureAwait(false);
              var firstAccount = accounts.FirstOrDefault();

              // this is expected to fail when account is null
              result = await pca1.AcquireTokenSilent(Config.Scopes, firstAccount)
                  .ExecuteAsync()
                  .ConfigureAwait(false);
              DisplayResult(result);
            }
            break;
          case '5': // Display Accounts            
            var accounts2 = await pca1.GetAccountsAsync().ConfigureAwait(false);
            if (!accounts2.Any())
              Console.WriteLine("No accounts were found in the cache.");

            foreach (var acc in accounts2)
              Console.WriteLine($"Account for {acc.Username}");
            break;

          case '6': // U/P and Silent in a loop
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("CTRL-C to stop...");

#pragma warning disable CS0618 // Type or member is obsolete
            cacheHelper.Clear();
#pragma warning restore CS0618 // Type or member is obsolete

            var pca2 = CreatePublicClient("https://login.microsoftonline.com/common");
            var pca3 = CreatePublicClient("https://login.microsoftonline.com/common");
            cacheHelper.RegisterCache(pca2.UserTokenCache);
            cacheHelper.RegisterCache(pca3.UserTokenCache);

            while (true)
            {
              _ = await Task.WhenAll(
                  RunRopcAndSilentAsync("PCA_1", pca1),
                  RunRopcAndSilentAsync("PCA_2", pca2),
                  RunRopcAndSilentAsync("PCA_3", pca3)
              ).ConfigureAwait(false);

              Trace.Flush();
              await Task.Delay(2000).ConfigureAwait(false);
            }
            break;

          case '7':
            var storageProperties = new StorageCreationPropertiesBuilder(
               Config.CacheFileName + ".other_secrets",
               Config.CacheDir)
            .WithMacKeyChain(
               Config.KeyChainServiceName + ".other_secrets",
               Config.KeyChainAccountName);

            var storage = Storage.Create(storageProperties.Build());
            //string lockFilePath = Path.Combine(Config.CacheDir, Config.CacheFileName + ".other_secrets.lockfile");

            var secretBytes = Encoding.UTF8.GetBytes("secret");
            Console.WriteLine("Writing...");
            storage.WriteData(secretBytes);

            Console.WriteLine("Writing again...");
            storage.WriteData(secretBytes);

            Console.WriteLine("Reading...");
            var data = storage.ReadData();
            Console.WriteLine("Read: " + Encoding.UTF8.GetString(data));

            Console.WriteLine("Deleting...");
            storage.Clear();
            break;

          case '8':
            storageProperties = new StorageCreationPropertiesBuilder(Config.CacheFileName + ".other_secrets2", Config.CacheDir).WithMacKeyChain(Config.KeyChainServiceName + ".other_secrets2", Config.KeyChainAccountName);

            storage = Storage.Create(storageProperties.Build());

            var lockFilePath = Path.Combine(Config.CacheDir, Config.CacheFileName + ".lockfile");

            using (new CrossPlatLock(lockFilePath)) // cross-process only
            {
              secretBytes = Encoding.UTF8.GetBytes("secret");
              Console.WriteLine("Writing...");
              storage.WriteData(secretBytes);

              Console.WriteLine("Writing again...");
              storage.WriteData(secretBytes);

              // if another process (not thread!) attempts to read / write this secret and uses the CrossPlatLock mechanism, it will wait for the lock to be released first
              await Task.Delay(1000).ConfigureAwait(false);

              Console.WriteLine("Reading...");
              data = storage.ReadData();
              Console.WriteLine("Read: " + Encoding.UTF8.GetString(data));

              Console.WriteLine("Deleting...");
              storage.Clear();
            } // lock released
            break;

          case 'c':
            var accounts4 = await pca1.GetAccountsAsync().ConfigureAwait(false);
            foreach (var acc in accounts4)
            {
              Console.WriteLine($"Removing account for {acc.Username}");
              await pca1.RemoveAsync(acc).ConfigureAwait(false);
            }            
            break;

          case 'e': // This is only meant for testing purposes

            _ = await pca1.GetAccountsAsync().ConfigureAwait(false); // do smth that loads the cache first

            var expiredValue = DateTimeOffset.UtcNow.AddMonths(-1);

            var accessor = pca1.UserTokenCache.GetType()
                .GetRuntimeProperties()
                .Single(p => p.Name == "Microsoft.Identity.Client.ITokenCacheInternal.Accessor")
                .GetValue(pca1.UserTokenCache);

            var internalAccessTokens = accessor.GetType().GetMethod("GetAllAccessTokens")?.Invoke(accessor, new object[] { null }) as IEnumerable<object>;

            foreach (var internalAt in internalAccessTokens)
            {
              _ = internalAt.GetType().GetRuntimeMethods().Single(m => m.Name == "set_ExpiresOn").Invoke(internalAt, new object[] { expiredValue });
              _ = accessor.GetType().GetMethod("SaveAccessToken")?.Invoke(accessor, new[] { internalAt });
            }

            var ctor = typeof(TokenCacheNotificationArgs).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single();

            var notificationArgs = ctor.Invoke(new object[] { pca1.UserTokenCache, Config.ClientId, null, true, false, true, null, null, null });
            var task = pca1.UserTokenCache.GetType().GetRuntimeMethods()
                .Single(m => m.Name == "Microsoft.Identity.Client.ITokenCacheInternal.OnAfterAccessAsync")
                .Invoke(pca1.UserTokenCache, new[] { notificationArgs });

            await (task as Task).ConfigureAwait(false);
            break;

          case 'x':
            return;
        }
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Exception : " + ex);
      }
    }
  }

  static async Task<AuthenticationResult> RunRopcAndSilentAsync(      string logPrefix,      IPublicClientApplication pca)
  {
    Console.WriteLine($"{logPrefix} Acquiring token by ROPC...");
    _ = await AcquireTokenROPCAsync(pca).ConfigureAwait(false);

    Console.WriteLine($"{logPrefix} OK. Now getting the accounts");
    var accounts = await pca.GetAccountsAsync().ConfigureAwait(false);

    Console.WriteLine($"{logPrefix} Acquiring token silent");

    var result = await pca.AcquireTokenSilent(Config.Scopes, accounts.First())
        .ExecuteAsync()
        .ConfigureAwait(false);

    Console.WriteLine($"{logPrefix} Deleting the account");
    foreach (var acc in accounts)
      await pca.RemoveAsync(acc).ConfigureAwait(false);

    return result;
  }

  static async Task<AuthenticationResult> AcquireTokenROPCAsync(      IPublicClientApplication pca)
  {
    return string.IsNullOrEmpty(Config.Username) ||
        string.IsNullOrEmpty(Config.Password)
      ? throw new InvalidOperationException("Please configure a username and password!")
      : await pca.AcquireTokenByUsernamePassword(
        Config.Scopes,
        Config.Username,
        Config.Password)
        .ExecuteAsync()
        .ConfigureAwait(false);
  }

  static void DisplayResult(AuthenticationResult result)
  {
    Console.ForegroundColor = ConsoleColor.DarkGreen;
    Console.WriteLine($"Token Acquisition:  Success!");
    Console.WriteLine($"  Got a token for:  {result.Account.Username}");
    Console.WriteLine($"     Token source:  {result.AuthenticationResultMetadata.TokenSource}");
    Console.WriteLine($"       Expires on:  {result.ExpiresOn.ToLocalTime()}     {result.AccessToken[..12]}...{result.AccessToken[^12..]}");
    Console.ResetColor();
  }

  static async Task<MsalCacheHelper> CreateCacheHelperAsync()
  {
    StorageCreationProperties storageProperties;

    try
    {
      storageProperties = new StorageCreationPropertiesBuilder(
          Config.CacheFileName,
          Config.CacheDir)
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
          Config.Authority)
      .Build();

      var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties).ConfigureAwait(false);

      cacheHelper.VerifyPersistence();

      return cacheHelper;
    }
    catch (MsalCachePersistenceException e)
    {
      Console.WriteLine($"WARNING! Unable to encrypt tokens at rest." +
          $" Saving tokens in plaintext at {Path.Combine(Config.CacheDir, Config.CacheFileName)} ! Please protect this directory or delete the file after use");
      Console.WriteLine($"Encryption exception: " + e);

      storageProperties =
          new StorageCreationPropertiesBuilder(
              Config.CacheFileName + ".plaintext", // do not use the same file name so as not to overwrite the encypted version
              Config.CacheDir)
          .WithUnprotectedFile()
          .Build();

      var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties).ConfigureAwait(false);
      cacheHelper.VerifyPersistence();

      return cacheHelper;
    }
  }

  static IPublicClientApplication CreatePublicClient(string authority)
  {
    var appBuilder = PublicClientApplicationBuilder.Create(Config.ClientId)                                                                             
        .WithAuthority(AzureCloudInstance.AzurePublic, tenant: "common")
        .WithRedirectUri("http://localhost"); // make sure to register this redirect URI for the interactive login to work

    var app = appBuilder.Build();
    Console.WriteLine($"Built public client");

    return app;
  }
}
