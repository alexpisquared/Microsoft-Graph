namespace Win_App_calling_MsGraph;

class ClientId
{
  // Below are the clientId (Application Id) of your app registration and the tenant information.
  // You have to replace:
  // - the content of ClientID with the Application Id for your app registration
  // - the content of Tenant by the information about the accounts allowed to sign-in in your application:
  //   - For Work or School account in your org, use your tenant ID, or domain
  //   - for any Work or School accounts, use `organizations`
  //   - for any Work or School accounts, or Microsoft personal account, use `common`
  //   - for Microsoft Personal account, use consumers

  public const string ZoePiTry1 = "81b1c6c7-ea12-4466-841c-0c53b530330b"; // ZoeP_  // <== https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps
  public const string ZoePiTry2 = "751b8b39-cde8-44e5-91e4-020f42e86e95"; // ZoePi  // Created by/fot the QuickStart WPF app   WORKS for the WPF app
  public const string NadinTry0 = "9ba0619e-3091-40b5-99cb-c2aca4abd04e"; // nadin 
  public const string JingmTry1 = "6dc84e4e-68d0-4f11-ba48-7e468aecb270"; // jingm  // Try 1
  public const string AlexpGood = "9ba0619e-3091-40b5-99cb-c2aca4abd04e"; // alex_  // MsgSlideshowUsrCtrl
  public const string AlexpTest = "195390b6-cc9c-4294-a219-369d9e4cb9fa"; // alexp  // AppRegPocFixTry
}