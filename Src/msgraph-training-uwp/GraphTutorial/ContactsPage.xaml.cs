
//https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/extern-alias
//extern alias GridV1;
//extern alias GridV2;
//using Class1V1 = GridV1::Namespace.Class1;
//using Class1V2 = GridV2::Namespace.Class1;
//extern alias MSGraphBeta;
//using GraphBeta = MSGraphBeta.Microsoft.Graph;

using Microsoft.Graph;
//using Microsoft.Graph.Beta;

using Microsoft.Toolkit.Graph.Providers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GraphTutorial
{
  public sealed partial class ContactsPage : Page
  {
    public ContactsPage() => InitializeComponent();
    private void ShowNotification(string message)
    {
      var mainPage = (Window.Current.Content as Frame).Content as MainPage;           // Get the main page that contains the InAppNotification
      var notification = mainPage.FindName("Notification") as InAppNotification; // Get the notification control

      notification.Show(message);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
      var graphClient = ProviderManager.Instance.GlobalProvider.Graph;      // Get the Graph client from the provider
      var report = "";

      try
      {
        var defoltContacts = await graphClient.Me.Contacts       /**/ .Request().GetAsync();
        var contactFolders = await graphClient.Me.ContactFolders /**/ .Request().GetAsync();

        report += $" *** {defoltContacts.Count,4} defoltContacts (main)    {contactFolders.Count,4} contactFolders.Count    \n";

        var contactFolderContacts = new List<object/*Contact*/>();      // Use this to store the contact from all contact folder.

        for (var i = 0; i < contactFolders.Count; i++)
        {
          var folderContacts = await graphClient.Me.ContactFolders[contactFolders[i].Id].Contacts.Request().GetAsync();

          contactFolderContacts.AddRange(folderContacts.AsEnumerable());
          //report += $" *** {i,4}   {folderContacts.Count,4} folderContacts.Count    \n";
        }

        contactFolderContacts.AddRange(defoltContacts.AsEnumerable());

        // Constanlty asks for aut-n:
        //while (defoltContacts.NextPageRequest != null)
        //{
        //  defoltContacts = await defoltContacts.NextPageRequest.GetAsync();
        //  contactFolderContacts.AddRange(defoltContacts.AsEnumerable());
        //}

        EventList.ItemsSource = contactFolderContacts.ToList();

        // Use this to test the result. for (var i = 0; i < contactFolderContacts.Count; i++)        {          var item = (dynamic)contactFolderContacts[i];          report += $" *** {i,4} FileAs: {item.FileAs,-32}   {item.DisplayName,-32}   ";        }

        //var c = new Microsoft.Graph.Contact();
        // Error CS0433  The type 'Contact' exists in both 'Microsoft.Graph.Beta, Version=0.8.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' and 'Microsoft.Graph, Version=3.21.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' GraphTutorial C:\g\Microsoft - Graph\Src\msgraph - training - uwp\GraphTutorial\ContactsPage.xaml.cs  62  Active
        // Error CS0430  The extern alias 'MSGraphBeta' was not specified in a / reference option GraphTutorial C:\g\Microsoft - Graph\Src\msgraph - training - uwp\GraphTutorial\ContactsPage.xaml.cs  2 Active

        //        var defoltContacts = await graphClient.Me.Contacts.Request()  // ?? IEnumerable<Contact>
        //                                                                   //.Select("FileAs")
        //                                                                   //.OrderBy("createdDateTime DESC")
        //.Top(7)
        //.GetAsync();

        //        EventList.ItemsSource = defoltContacts.CurrentPage.ToList();
      }
      catch (ServiceException ex) { ShowNotification($"Exception getting defoltContacts: {ex.Message}"); }

      tbkReport.Text = report;
      Debug.WriteLine(report);

      base.OnNavigatedTo(e);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      //while (defoltContacts.NextPageRequest != null)
      //{
      //  defoltContacts = await defoltContacts.NextPageRequest.GetAsync();
      //  contactFolderContacts.AddRange(defoltContacts.AsEnumerable());
      //}
    }

    //async Task AddContact(Contact myContact)
    //{
    //  try
    //  {
    //    var graphServiceClient = ProviderManager.Instance.GlobalProvider.Graph;      // Get the Graph client from the provider
    //    var requestContact = new Contact
    //    {
    //      GivenName = myContact.GivenName,
    //      Surname = myContact.Surname,
    //      CompanyName = myContact.CompanyName,
    //    };
    //    var emailList = new List<EmailAddress>();
    //    emailList.Add(new EmailAddress { Address = myContact.EmailAddress, Name = myContact.EmailAddress });
    //    requestContact.EmailAddresses = emailList;
    //    var businessPhonesList = new List<string>();
    //    businessPhonesList.Add(myContact.BusinessPhone);
    //    requestContact.BusinessPhones = businessPhonesList;
    //    var homePhonesList = new List<string>();
    //    homePhonesList.Add(myContact.HomePhone);
    //    requestContact.HomePhones = homePhonesList;
    //    await graphServiceClient.Me.Contacts.Request().AddAsync(requestContact);
    //  }
    //  catch (Exception el)
    //  {
    //  }
    //  return;
    //}
  }
}
