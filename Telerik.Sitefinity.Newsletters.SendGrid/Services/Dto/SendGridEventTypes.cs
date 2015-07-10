using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telerik.Sitefinity.Newsletters.SendGrid.Services.Dto
{
    /// <summary>
    /// A static class containing the SendGrid event type names.
    /// </summary>
    public static class SendGridEventTypes
    {
        /// <summary>
        /// Bounce event type name.
        /// </summary>
        public static readonly string Bounce = "Bounce";

        /// <summary>
        /// Dropped event type name.
        /// </summary>
        public static readonly string Dropped = "Dropped";

        //// TODO: fill in the other event types according to 
        //// https://sendgrid.com/docs/API_Reference/Webhooks/event.html
    }
}
