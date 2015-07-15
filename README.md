# Sitefinity Email campaigns SendGrid connector

###Summary
_This project implements a two way communication between the SendGrid Web API and the Sitefinity email campaigns (newsletters) module._

The project consist of two major components:
* The first one 'SendGridSender.cs' is responsible for adapting the Sitefinity email campaigns data for SendGrid and transporting it to the SendGrid services. It is an implementation of the Sitefinity notifications system base sender.

* The second component 'SendGridEventsInboundService.cs' is a web service that receives event notifications by SendGrid about failed deliveries. It is implemented using the Service Stack components and for now can handle only dropped and bounced events.

###Requirements

* The two way communication depends on the ability of the SendGrid services to notify your Sitefinity web application for unsuccessful email deliveries. This means that your Sitefinity web site must be publicly accessible.

###Setup considerations

By default the web service in Sitefinity that receives the event notifications from SendGrid requires an authenticated backend user to prevent unauthorized access to the service functionality.
It is advisable that a specific user is created for this purpose. Since SendGrid uses basic authentication and therefore supplies the username and password combination in each request an HTTPS connection is highly recommended. 

As for now the service functionality of the service is limited to writing failed delivery entries in the database only if the request contains a valid campaign and subscriber ids which makes the service a bit more resilient to attacks. This is why the authentication requirement can be switched off with a configuration change [see how to switch off required authentication](#How-to-switch-off-required-authentication).

###How to install

* Build the main project (Telerik.Sitefinity.Newsletters.SendGrid) in this repository with the correct Sitefinity package versions and place the resulting dll in the \bin directory of your Sitefinity web site or install the Sitefinity built package from the official Sitefinity NuGet repository. We will supply a signed package in our NuGet repository for each official Sitefinity version.

* Install from Sitefinity NuGet repository:
```powershell
Install-Package Telerik.Sitefinity.Newsletters.SendGrid -Version 8.1.5800.0 -Source http://nuget.sitefinity.com/nuget/
```

* Navigate to http://__[mySitefinityWebSite]__/Sitefinity/Administration/Settings/Advanced/Notifications. Then in the tree view on the left expand Profiles and select SendGrid. Here you will have to specify a valid port and host address although their values are not going to be used since the SendGrid SDK uses a hardcoded address for service calls. You can use 'api.sendgrid.com' for host and '80' for port. Enter your SendGrid account username and password then change the SenderType to __Telerik.Sitefinity.Newsletters.SendGrid.Notifications.SendGridSender__.

* Navigate to 'http://__[mySitefinityWebSite]__/Sitefinity/Administration/Settings/Basic/Newsletters/?sf_global=true' and change the active profile to SendGrid.

* In your SendGrid account navigate to Dashboard, then Settings -> Mail Settings and activate the event notification app. You have to specify the HTTP post URL to 'http(s)://__[username]:[password]@[mySitefinityWebSite]__/restapi/sendgrid/events' or 'http(s)://__[mySitefinityWebSite]__/restapi/sendgrid/events' if you have disabled the authentication requirement ([see setup considerations](#setup considerations)).  Then check the Bounced and Dropped events to be reported and then save the configuration.

* Create a Sitefinity __backend__ user with the specified [username] and [password] of the step above.

###How to switch off required authentication

You can switch off the authentication requirement of the SendGrid event receiving Sitefinity service by adding an application setting in the web.config.

```xml
<?xml version="1.0" encoding="utf-8"?> 
  <configuration>  
    <appSettings>
      <!--other app settings-->
      <add key="SendGridConnector:RequireAuth" value="false" />
    </appSettings> 
  </configuration>
```
