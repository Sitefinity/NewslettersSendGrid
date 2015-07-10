# Sitefinity Email campaigns SendGrid connector

###Summary
_This project implements a two way communication between the SendGrid Web API and the Sitefinity email campaigns (newsletters) module, enabling partial integration between the two systems._

The project consist of two major components:
* The first one 'SendGridSender.cs' is responsible for adapting the Sitefinity email campaigns data for SendGrid and transporting it to the SendGrid services. It is an implementation of base sender of the Sitefinity notifications system.

* The second component 'SendGridEventsInboundService.cs' is a web service that receives event notifications by SendGrid about failed deliveries. It is implemented using the Service Stack components and for now can handle only dropped and bounced events that notify of a failed delivery.

###How to install

* Build the main project (Telerik.Sitefinity.Newsletters.SendGrid) in this repository with the correct Sitefinity references and place the resulting dll in the \bin directory of your Sitefinity web site or install the corresponding NuGet package from the official Sitefinity nuget repository. 

* Navigate to http://__[mySitefinityWebSite]__/Sitefinity/Administration/Settings/Advanced/Notifications. Then in the tree view on the left expand Profiles and select SendGrid. Here you will have to specify a valid port and host address although their values are not going to be used since the SendGrid SDK uses a hardcoded address for service calls. You can use 'api.sendgrid.com' for host and '80' for port. Enter your SendGrid account username and password then change the SenderType to __Telerik.Sitefinity.Newsletters.SendGrid.Notifications.SendGridSender__.

* Navigate to http://__[mySitefinityWebSite]__/Sitefinity/Administration/Settings/Basic/Newsletters/?sf_global=true and change the active profile to SendGrid.
