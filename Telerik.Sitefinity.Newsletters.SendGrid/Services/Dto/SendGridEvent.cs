using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

namespace Telerik.Sitefinity.Newsletters.SendGrid.Services.Dto
{
    /// <summary>
    /// This class represents the common properties contained in a SendGrid event message
    /// </summary>
    /// <remarks>
    /// Docs: <c>https://sendgrid.com/docs/API_Reference/Webhooks/event.html</c>
    /// </remarks>
    [DataContract]
    public class SendGridEvent
    {
        /// <summary>
        /// Gets or sets the email that the event corresponds to.
        /// </summary>
        /// <value>The email.</value>
        [DataMember(Name = "email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the event.
        /// </summary>
        /// <value>The timestamp.</value>
        [DataMember(Name = "timestamp")]
        public int Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the id of the SMTP server.
        /// </summary>
        /// <value>The SMTP server id.</value>
        [DataMember(Name = "smtp-id")]
        public string SmtpId { get; set; }

        /// <summary>
        /// Gets or sets the message id.
        /// </summary>
        /// <value>The message id.</value>
        [DataMember(Name = "sg_message_id")]
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the SendGrid event type.
        /// </summary>
        /// <value>The SendGrid event type.</value>
        /// <example>
        /// Processed - Message has been received and is ready to be delivered.
        /// Dropped - You may see the following drop reasons: Invalid SMTPAPI header, Spam Content (if spam checker app enabled), Unsubscribed Address, Bounced Address, Spam Reporting Address, Invalid, Recipient List over Package Quota
        /// Delivered - Message has been successfully delivered to the receiving server.
        /// Deferred - Recipient’s email server temporarily rejected message.
        /// Bounce - Receiving server could not or would not accept message.
        /// Open - Recipient has opened the HTML message.
        /// Click - Recipient clicked on a link within the message.
        /// Spam Report - Recipient marked message as spam.
        /// Unsubscribe - Recipient clicked on message’s subscription management link.
        /// Group Unsubscribe - Recipient unsubscribed from specific group, by either direct link or updating preferences.
        /// Group Resubscribe - Recipient re-subscribes to specific group by updating preferences.
        /// </example>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Resubscribe is the event name used by SendGrid.")]
        [DataMember(Name = "event")]
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets the bounce type.
        /// </summary>
        /// <value>The bounce type.</value>
        [DataMember(Name = "type")]
        public string BounceType { get; set; }

        /// <summary>
        /// Gets or sets the reason for the bounce.
        /// </summary>
        /// <value>The reason for the bounced email.</value>
        [DataMember(Name = "reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets the status code string.
        /// </summary>
        /// <value>The status code as string.</value>
        /// <remaark>
        /// An example value is "5.0.0"
        /// </remaark>
        [DataMember(Name = "status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the id of the Sitefinity campaign that the bounced email refers to.
        /// </summary>
        /// <value>The Sitefinity campaign.</value>
        [DataMember(Name = "X-Sitefinity-Campaign")]
        public Guid SitefinityCampaignId { get; set; }

        /// <summary>
        /// Gets or sets the Sitefinity subscriber id.
        /// </summary>
        /// <value>The Sitefinity subscriber id.</value>
        [DataMember(Name = "X-Sitefinity-Subscriber")]
        public Guid SitefinitySubscriberId { get; set; }
    }
}
