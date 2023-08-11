namespace Win_App_calling_MsGraph
{
  [System.Runtime.Versioning.SupportedOSPlatform("windows")]
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

    public static string ZoePiTry1 => "81b1c6c7-ea12-4466-841c-0c53b530330b"; // <== https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps
    public static string ZoePiTry2 => "751b8b39-cde8-44e5-91e4-020f42e86e95"; // Created by/fot the QuickStart WPF app   WORKS for the WPF app
    public static string ZoePiTryC => "37a70278-1484-49a3-902e-f10e06dd86f0"; // Try00C
    public static string NadinTry0 => "9ba0619e-3091-40b5-99cb-c2aca4abd04e"; // fake
    public static string JingmTry1 => "6dc84e4e-68d0-4f11-ba48-7e468aecb270"; // Try 1
    public static string JingmTry2 => "adcdfbad-d7c4-4df6-a1ab-eca20f7eb8a5"; // Try002 
    public static string JingmTry3 => "99789b15-531c-4c29-9386-406acbda8f58"; // Try003 
    public static string AlexpGood => "9ba0619e-3091-40b5-99cb-c2aca4abd04e"; // MsgSlideshowUsrCtrl
    public static string AlexpTest => "195390b6-cc9c-4294-a219-369d9e4cb9fa"; // AppRegPocFixTry
    public static string XbyProper => "702b6d6d-05a3-4403-a294-d3afc3a41b77"; // XbyProperAccount Aug 10 trying to use new app reg-n ..with no avaul.
    public static string WellKnown => "4a1aa1d5-c567-49d0-ad0b-cd957a47f842"; // ==> APPARENTLY:  this is a well known client id for the Microsoft Graph Explorer demos
  }
}