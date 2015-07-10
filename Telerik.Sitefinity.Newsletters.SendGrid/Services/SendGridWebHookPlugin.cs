using System;
using ServiceStack;
using Telerik.Sitefinity.Newsletters.SendGrid.Services.Dto;

namespace Telerik.Sitefinity.Newsletters.SendGrid.Services
{
    /// <summary>
    /// A ServiceStack plug-in that configures the web hook endpoints used by SendGrid for event notifications.
    /// </summary>
    public class SendGridWebHookPlugin : IPlugin
    {
        /// <summary>
        /// Adding the SendGrid web hook service routes
        /// </summary>
        /// <param name="appHost">The service stack appHost</param>
        public void Register(IAppHost appHost)
        {
            if (appHost == null)
                throw new ArgumentNullException("appHost");

            appHost.RegisterService(typeof(SendGridEventsInboundService));

            // TODO: A unique string should be added to the URL for security reasons.
            appHost.Routes.Add<SendGridEvent[]>(string.Concat(SendGridWebHookPlugin.SendGridServiceRoute, "/events"), "POST");
        }

        private const string SendGridServiceRoute = "/sendgrid";
    }
}