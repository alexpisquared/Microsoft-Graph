# Microsoft-Graph
## Dabbling into ...just

### UWP selected
#### UWP selected
##### UWP selected
###### UWP selected

https://developer.microsoft.com/en-us/graph/quick-start?appID=c0e92fd4-e675-4e0d-8ce5-fa222b3dd60b&appName=My%20UWP%20App&redirectUrl=http://localhost:8000&platform=option-windowsuniversal

UWP selected
Registration Successful!
  Thanks for registering your app. We've configured the app ID (also called client ID) and redirect URI in the code sample.
  App Name              My UWP App
  App ID (or Client ID) See-App-IDs-4e0d-8ce5-fa222b3dd60b
    WARNING!!! No Go! Use the manually in Azure created App Id !!!       _vvvvv_
Refer to C:\g\Microsoft-Graph\Src\msgraph-training-uwp\ReadMe.md for the WORKING Azure steps.
Fix for the "ref not found in universe" error:   PM>Install-Package Microsoft.Graph -Version 3.21.0

Fix for Contacts - access denied 
  nogo: https://aad.portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps
    added Contacts.Read ..Shared permissions to the app in https://aad.portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/CallAnAPI/appId/5b02c71c-515a-4a7a-9081-0fb56f25958c/isMSAApp/

  WORKS: add here <data name="Scopes" xml:space="preserve"> + more from https://stackoverflow.com/questions/51760194/get-contacts-from-all-outlook-contact-folders-microsoft-graph
