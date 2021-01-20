﻿using Microsoft.Graph;
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

      try
      {
        var defoltContacts = await graphClient.Me.Contacts       /**/ .Request().GetAsync();
        var contactFolders = await graphClient.Me.ContactFolders /**/ .Request().GetAsync();

        var contactFolderContacts = new List<object/* Contact*/>();      // Use this to store the contact from all contact folder.

        if (contactFolders.Count > 0)
        {
          for (var i = 0; i < contactFolders.Count; i++)
          {
            var folderContacts = await graphClient
                .Me
                .ContactFolders[contactFolders[i].Id]
                .Contacts
                .Request()
                .GetAsync();

            contactFolderContacts.AddRange(folderContacts.AsEnumerable());
          }

          contactFolderContacts.AddRange(defoltContacts.AsEnumerable());        // This will combine the contact from main folder and the additional folders.
        }
        else
        {
          contactFolderContacts.AddRange(defoltContacts.AsEnumerable());        // This user only has the default contacts folder
        }

        // Use this to test the result.
        foreach (var item in contactFolderContacts)
        {
          Debug.WriteLine("first:" + item/*.EmailAddresses*/);
        }

        //var c = new Microsoft.Graph.Contact();

        var contactList = await graphClient.Me.Contacts.Request()  // ?? IEnumerable<Contact>
                                                                   //.Select("FileAs")
                                                                   //.OrderBy("createdDateTime DESC")
            .Top(7)
            .GetAsync();

        while (contactList.NextPageRequest != null)
        {
          contactList = await contactList.NextPageRequest.GetAsync();
        }

        EventList.ItemsSource = contactList.CurrentPage.ToList();
      }
      catch (Microsoft.Graph.ServiceException ex)
      {
        ShowNotification($"Exception getting contactList: {ex.Message}");
      }

      base.OnNavigatedTo(e);
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
