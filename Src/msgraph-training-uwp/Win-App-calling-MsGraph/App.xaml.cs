using System.Windows;
using Microsoft.Identity.Client;
namespace Win_App_calling_MsGraph;
public partial class App : Application
{
  static App() => PublicClientApp = PublicClientApplicationBuilder.Create(ClientId.JingmTry2)
        .WithAuthority(AzureCloudInstance.AzurePublic, tenant: "common")
        .WithDefaultRedirectUri()
        .Build();

  public static IPublicClientApplication PublicClientApp { get; set; }
}
