# Microsoft-Graph ... Dabbling into... let's use for onedrive file access for the pic viewer.

## UWP Graph Tutorial (:registered as on Azure)  -  LOST ONE  -  

UWP selected
Registration Successful!

  Thanks for registering your app. We've configured the app ID (also called client ID) and redirect URI in the code sample.

  App Name              My UWP App

  App ID (or Client ID) See-App-IDs-4e0d-8ce5-fa222b3dd60b

    WARNING!!! No Go! Use the manually in Azure created App Id !!!       _vvvvv_

Fix for the "ref not found in universe" error:   PM>Install-Package Microsoft.Graph -Version 3.21.0

### Fix for Contacts - access denied 
  nogo: https://aad.portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps
    added Contacts.Read ..Shared permissions to the app in https://aad.portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/CallAnAPI/appId/5b02c71c-515a-4a7a-9081-0fb56f25958c/isMSAApp/

  WORKS: add here <data name="Scopes" xml:space="preserve"> + more from https://stackoverflow.com/questions/51760194/get-contacts-from-all-outlook-contact-folders-microsoft-graph


## Console  GraphTutorialConsole:   reviwed in May 2023:  all works, too tedious: must find the way to not login evey time.
.NET Core Graph Tutorial  from  https://docs.microsoft.com/en-us/graph/tutorials/dotnet-core
For the original see C:\gh\s\msgraph-training-dotnet-core\demo\GraphTutorial 
...all messed up ... ==> 
## Console  GraphTutorialConsole2:  redoing steps from  https://docs.microsoft.com/en-us/graph/tutorials/dotnet-core  May 15, 2023

### also see 
    https://developer.microsoft.com/en-us/graph/graph-explorer
    OAuth2: https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-device-code

# Core  Win-App-calling-MsGraph  
https://learn.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-windows-desktop
### .Net4 -> .Net7
#### Option 1: Express  mode <== NOGO: AD only?!?!?
#### Option 2: Advanced mode <== SUCCESS!!!  

more samples: https://learn.microsoft.com/en-us/azure/active-directory/develop/sample-v2-code#desktop

??? 
Sample code of how to Use GraphServiceClient with acquire a token using AuthenticationResult

Also See:
  working UWP sample C:\gh\s\onedrive-sample-photobrowser-uwp\OneDrivePhotoBrowser\
                     C:\gh\s\onedrive-sample-photobrowser-uwp\OneDrivePhotoBrowser\Controllers\ItemsController.cs   <==  1dr navigation samples
  !WORKG WinForms:   https://github.com/OneDrive/onedrive-sample-apibrowser-dotnet
                     C:\gh\s\onedrive-sample-apibrowser-dotnet\OneDriveApiBrowser\FormBrowser.cs
                     ^ 1dr navigation samples

## Next:
  ++ cache token to FS
  -- Nuget pkg for Microsoft.Graph is too old im DEmoApp ... 
    using https://developer.microsoft.com/en-us/graph/quick-start:
      Your registration was successful! Thanks for registering your app. We configured the client ID in the code sample.
      App name Graph .NET quick start
      ClientID 4a3fe902-51ea-40ed-8ce2-8666f68ad88f
      ==> still not the latest ver: 4... not 5... which is a breaking change for 4...


