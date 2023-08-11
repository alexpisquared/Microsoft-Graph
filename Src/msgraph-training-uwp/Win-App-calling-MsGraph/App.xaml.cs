using System.Windows;
using Microsoft.Identity.Client;
namespace Win_App_calling_MsGraph;
public partial class App : Application
{
  static App()
  {
    NewMethod(ClientId.XbyProper);
  }

  public static void NewMethod(string clientId)
  {
    PublicClientApp = PublicClientApplicationBuilder.Create(clientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, tenant: "common")
            .WithDefaultRedirectUri()
            .Build();
  }

  public static IPublicClientApplication PublicClientApp { get; set; }
}
