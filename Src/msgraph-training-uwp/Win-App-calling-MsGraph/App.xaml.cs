using System.Windows;
using Microsoft.Identity.Client;
namespace Win_App_calling_MsGraph;
public partial class App : Application
{

  static App() => PublicClientApp = PublicClientApplicationBuilder.Create(ClientId)
        .WithAuthority(AzureCloudInstance.AzurePublic, Tenant)
        .WithDefaultRedirectUri()
        .Build();

  // Below are the clientId (Application Id) of your app registration and the tenant information.
  // You have to replace:
  // - the content of ClientID with the Application Id for your app registration
  // - the content of Tenant by the information about the accounts allowed to sign-in in your application:
  //   - For Work or School account in your org, use your tenant ID, or domain
  //   - for any Work or School accounts, use `organizations`
  //   - for any Work or School accounts, or Microsoft personal account, use `common`
  //   - for Microsoft Personal account, use consumers
  private static readonly string ClientId = "9ba0619e-3091-40b5-99cb-c2aca4abd04e";

  private static readonly string Tenant = "common";

  public static IPublicClientApplication PublicClientApp { get; private set; }
}
