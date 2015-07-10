using System;
using System.ComponentModel;
using System.Linq;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Newsletters.SendGrid.Services;
using Telerik.Sitefinity.Services;

namespace Telerik.Sitefinity.Newsletters.SendGrid
{
    /// <summary>
    /// Contains the application startup event handlers registering the required components for the translations module of Sitefinity.
    /// </summary>
    public static class Startup
    {
        /// <summary>
        /// Called before the Asp.Net application is started. Subscribes for the logging and exception handling configuration related events.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void OnPreApplicationStart()
        {
            Bootstrapper.Bootstrapping += Startup.BootstrapperBootstrapping;
        }

        private static void BootstrapperBootstrapping(object sender, EventArgs e)
        {
            SystemManager.RegisterServiceStackPlugin(new SendGridWebHookPlugin());
        }
    }
}
