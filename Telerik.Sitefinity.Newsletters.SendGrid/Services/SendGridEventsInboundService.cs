using System;
using System.Linq;
using System.Net;
using ServiceStack;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Modules.Newsletters;
using Telerik.Sitefinity.Modules.Newsletters.Communication;
using Telerik.Sitefinity.Newsletters.Model;
using Telerik.Sitefinity.Newsletters.SendGrid.Services.Dto;

namespace Telerik.Sitefinity.Newsletters.SendGrid.Services
{
    /// <summary>
    /// This contains the implementation of a SendGrid events receiving web hook.
    /// </summary>
    public class SendGridEventsInboundService : Service
    {
        /// <summary>
        /// Handles the notifications events sent by SendGrid.
        /// </summary>
        /// <param name="events">An array of the events that have occurred.</param>
        public void Post(SendGridEvent[] events)
        {
            // TODO: provider name should be configurable
            // TODO: get the manager only if an event that we are interested in is present in the events variable.
            // Could be done with a property but I am not aware of the this object lifecycle at the moment.
            // TODO: Ideally the whole data related operations like getting subscribers, campaigns and writing statistics 
            // should be in a separate component and not in the service itself.
            var newslettersManager = NewslettersManager.GetManager();
            var isToSaveChanges = false;

            // TODO: Use dynamic objects and inspect them for the required properties instead of 
            // this strongly typed approach since the different events have very different properties
            // and this hook should be able to handle all and not throw exceptions if an event that is
            // not supported is received.
            foreach (var sendGridEvent in events)
            { 
                // TODO: rewrite this with chain of responsibility so that additional event handlers can be easily added.
                if (this.IsToHanlde(sendGridEvent))
                {
                    BounceStatus bounceStatus = this.ResolveStatus(sendGridEvent);

                    Campaign campaign;
                    Subscriber subscriber;
                    try
                    {
                        campaign = newslettersManager.GetCampaign(sendGridEvent.SitefinityCampaignId);
                        subscriber = newslettersManager.GetSubscriber(sendGridEvent.SitefinitySubscriberId);
                    }
                    catch (Telerik.Sitefinity.SitefinityExceptions.ItemNotFoundException)
                    {
                        // If the campaign or the subscriber is not found we skip to the next event.
                        break;
                    }

                    this.WriteStatistics(newslettersManager, campaign, subscriber, bounceStatus);
                    this.PerformBounceAction(newslettersManager, subscriber, bounceStatus);
                    isToSaveChanges = true;
                }
            }

            if (isToSaveChanges)
            {
                newslettersManager.SaveChanges(); 
            }

            this.Response.StatusCode = (int)HttpStatusCode.OK;
        }
 
        private BounceStatus ResolveStatus(SendGridEvent sendGridEvent)
        {
            BounceStatus bounceStatus;

            // If the event is for dropped email we mark it as hard bounced.
            if (sendGridEvent.EventType.EqualsIgnoreCase(SendGridEventTypes.Dropped))
            {
                bounceStatus = BounceStatus.Hard;
            }
            else
            {
                bounceStatus = MessageParser.GetBounceMessageStatus(sendGridEvent.Status);
            }

            return bounceStatus;
        }
 
        private bool IsToHanlde(SendGridEvent sendGridEvent)
        {
            // TODO: trace the messages that should not be handled for debugging purposes.
            // For now this service can handle only bounced and dropped events.
            return
                  (sendGridEvent.EventType.EqualsIgnoreCase(SendGridEventTypes.Bounce) || sendGridEvent.EventType.EqualsIgnoreCase(SendGridEventTypes.Dropped)) &&
                  sendGridEvent.SitefinityCampaignId != Guid.Empty &&
                  sendGridEvent.SitefinitySubscriberId != Guid.Empty;
        }
 
        private void WriteStatistics(NewslettersManager newslettersManager, Campaign campaign, Subscriber subscriber, BounceStatus bounceStatus)
        {
            // TODO: check for indexes on the bounce status database table by campaign id and subscriber id.
            var bounceStat = newslettersManager.GetBounceStats().Where(b => b.Campaign.Id == campaign.Id && b.Subscriber.Id == subscriber.Id).FirstOrDefault();
            if (bounceStat == null)
            {
                bounceStat = newslettersManager.CreateBounceStat();
            }
            else
            {
                // If the bounce stat is already created, then the current bounce is caused by retry operation.
                bounceStat.RetryCount++;
            }

            bounceStat.Campaign = campaign;
            bounceStat.Subscriber = subscriber;
            bounceStat.SmtpStatus = Enum.GetName(typeof(Telerik.Sitefinity.Newsletters.Model.MessageStatus), bounceStatus);
            bounceStat.BounceStatus = bounceStatus;
            bounceStat.IsProcessing = false;
        }

        private void PerformBounceAction(NewslettersManager newslettersManager, Subscriber subscriber, BounceStatus bounceStatus)
        {
            var bounceActionResolver = ObjectFactory.Resolve<BounceActionResolver>();

            BounceAction bounceAction = bounceActionResolver.ResolveAction(bounceStatus);
            bounceActionResolver.PerformAction(newslettersManager, subscriber.Id, bounceAction, false);
        }
    }
}