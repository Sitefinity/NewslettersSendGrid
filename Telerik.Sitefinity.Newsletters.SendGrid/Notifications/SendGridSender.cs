using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Telerik.Microsoft.Practices.Unity.Utility;
using Telerik.Sitefinity.Services.Notifications;
using Telerik.Sitefinity.Services.Notifications.Composition;
using Telerik.Sitefinity.Services.Notifications.Configuration;
using Telerik.Sitefinity.Services.Notifications.Model;
using SG = SendGrid;

namespace Telerik.Sitefinity.Newsletters.SendGrid.Notifications
{
    /// <summary>
    /// SendGrid email sender.
    /// </summary>
    public class SendGridSender : Sender, IBatchSender
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridSender" /> class.
        /// </summary>
        /// <remarks>This constructor method is used by the unit tests project.</remarks>
        public SendGridSender()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridSender" /> class.
        /// </summary>
        /// <param name="senderProfile">The sender profile.</param>
        /// <remarks>This is the constructor that is used by the Sitefinity notifications send scheduled task
        /// to construct an initialized instance of this sender.</remarks>
        public SendGridSender(SenderProfileElement senderProfile)
        {
            this.InitSettings(senderProfile);
        }

        /// <summary>
        /// Gets the size of the batch.
        /// </summary>
        /// <value>The size of the batch.</value>
        public int BatchSize
        {
            get
            {
                return 100;
            }
        }

        /// <summary>
        /// Gets the interval in seconds between batches.
        /// </summary>
        /// <value>The pause interval.</value>
        public int PauseInterval
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the user name of the SendGrid account used to send the emails.
        /// </summary>
        /// <value>The SendGrid user name.</value>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password of the SendGrid account.
        /// </summary>
        /// <value>The SendGrid account password.</value>
        public string Password { get; set; }

        /// <summary>
        /// Sends an instant message to the specified subscriber.
        /// </summary>
        /// <param name="messageInfo">The message info.</param>
        /// <param name="subscriber">The subscriber.</param>
        /// <returns>The result of the sending operation.</returns>
        public override SendResult SendMessage(IMessageInfo messageInfo, ISubscriberRequest subscriber)
        {
            throw new NotSupportedException("The SendGrid sender does not support sending a single email.");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", "CA1063:ImplementIDisposableCorrectly",
            Justification = "The IDisposable interface is imposed by the base class but there is nothing to dispose of.")]
        public sealed override void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sends the message to the given subscribers using the SendGrid service.
        /// </summary>
        /// <param name="messageJob">The message info.</param>
        /// <param name="subscribers">The subscribers.</param>
        /// <returns>Information about the batch send operation.</returns>
        public SendResult SendMessage(IMessageJobRequest messageJob, IEnumerable<ISubscriberResponse> subscribers)
        {
            Guard.ArgumentNotNull(messageJob, "messageJob");
            Guard.ArgumentNotNull(subscribers, "subscribers");

            var message = this.ConstructMessage(messageJob, subscribers);
            var task = this.SendAsync(message);
            var subscriberResulType = task.Result.Type == SendResultType.Failed ? SendResultType.FailedRecipient : task.Result.Type;
            foreach (var subscriber in subscribers)
            {
                var notifiableSubscriber = subscriber as INotifiable;
                if (notifiableSubscriber != null)
                {
                    notifiableSubscriber.Result = subscriberResulType;
                    notifiableSubscriber.IsNotified = true;
                }
            }

            return task.Result;
        }
 
        /// <summary>
        /// Constructs a SendGrid message based on the specified <paramref name="messageJob"/> and <paramref name="subscribers"/>.
        /// </summary>
        /// <param name="messageJob">The Sitefinity message job from which to construct a SendGrid message.</param>
        /// <param name="subscribers">The subscribers for the message.</param>
        /// <returns>A SendGrid message with configured subscribers, substitutions, subject template etc.</returns>
        public SG.SendGridMessage ConstructMessage(IMessageJobRequest messageJob, IEnumerable<ISubscriberResponse> subscribers)
        {
            //// TODO: raise some events here. There is a high chance someone would like 
            //// to extend the message before or after it has been constructed in the following code.
            var message = new SG.SendGridMessage();

            this.AddGlobalProperties(message, messageJob);

            // Adding per subscriber information that is needed to build the message template and the subscriber id custom message header.
            this.AddSubscribersInfo(message, messageJob, subscribers);

            // Adding the custom headers of the message job
            this.AddCustomHeaders(message, messageJob.CustomMessageHeaders);
            return message;
        }

        /// <summary>
        /// Sends the specified SendGrid <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The SendGrid message to send.</param>
        /// <returns>
        /// The asynchronous task that can be awaited. The Task.Result is the Sitefinity SendResult
        /// that indicates the weather the sending was successful or not.</returns>
        public async Task<SendResult> SendAsync(SG.SendGridMessage message)
        {
            // Create credentials, specifying your user name and password.
            var credentials = new NetworkCredential(this.Username, this.Password);

            // Create a Web transport for sending email.
            var transportWeb = new SG.Web(credentials);

            try
            {
                // No need for asynchronous execution since the sender is invoked in a separate thread
                // dedicated to sending the messages.
                await transportWeb.DeliverAsync(message);

                return SendResult.ReturnSuccess();
            }
            catch (HttpException ex)
            {
                // HttpExceptions are expected so we just return that the sending failed.
                return SendResult.ReturnFailed(ex);
            }
            catch (Exception ex)
            {
                // TODO: run those exceptions through a Notifications exception handling policy before returning the result.
                return SendResult.ReturnFailed(ex);
            }
        }

        private void AddSubscribersInfo(SG.SendGridMessage message, IMessageJobRequest messageJob, IEnumerable<ISubscriberResponse> subscribers)
        {
            IEnumerable<string> replacementTags = this.GetReplacementTags(messageJob);

            // Creating Dictionary<string, List<string>> that will contain the replacement tag like {|Subscriber.Name|} and custom headers
            // like {|Subscriber.ResolveKey|} as keys. The value will be the list of per subscriber values in the same order as the list of 
            // emails to send to.
            var substitutions = this.InitializeSubstitutions(replacementTags, messageJob.CustomMessageHeaders);

            // Filling in the substitutions data structure with per subscriber values.
            foreach (var subscriber in subscribers)
            {
                // TODO: validate subscribers email addresses
                // TODO: add email + recipient name as TO address.
                message.AddTo(subscriber.Email);

                var subscriberPropertiesAsDict = subscriber.ToDictionary();
                this.CalculateSubstitutions(substitutions, subscriberPropertiesAsDict);
            }

            // Adding the constructed substitutions in the SendGrid message.
            foreach (var substitutionPair in substitutions)
            {
                message.AddSubstitution(substitutionPair.Key, substitutionPair.Value);
            }
        }
 
        private void CalculateSubstitutions(Dictionary<string, List<string>> substitutions, Dictionary<string, string> subscriberProperties)
        {
            foreach (var substitution in substitutions)
            {
                string replacementTagsValues = substitution
                                                           .Key
                                                           .Replace(NewsletterTemplatesConstants.PlaceholdersStartTag, string.Empty)
                                                           .Replace(NewsletterTemplatesConstants.PlaceholderEndTag, string.Empty)
                                                           .Trim();

                string value;

                // the order in the list of substitutions is important so we insert an empty 
                // string even if there is no value in the subscriber properties.
                if (!subscriberProperties.TryGetValue(replacementTagsValues, out value))
                    value = string.Empty;

                substitution.Value.Add(value);
            }
        }
 
        private Dictionary<string, List<string>> InitializeSubstitutions(IEnumerable<string> replacementTags, IDictionary<string, string> customHeaders)
        {
            var substitutions = new Dictionary<string, List<string>>(replacementTags.Count());

            // Initializing the substitutions dictionary so that it can later be accessed by replacementTag.
            foreach (var replacementTag in replacementTags)
            {
                substitutions.Add(replacementTag, new List<string>(this.BatchSize));
            }

            foreach (var customHeader in customHeaders)
            {
                // add substitution dictionaries only if the custom header value is a placeholder.
                if (NewsletterTemplatesConstants.PlaceholdersRegex.IsMatch(customHeader.Value))
                {
                    substitutions.Add(customHeader.Value, new List<string>(this.BatchSize));
                }
            }

            return substitutions;
        }
 
        private IEnumerable<string> GetReplacementTags(IMessageJobRequest messageJob)
        {
            // TODO: add the replacement tags from the HTML and plain text messages in a hash set then return the set.AsEnumerable();
            // this way all replacement tags will be considered not just those in the HTML message.
            var matches = NewsletterTemplatesConstants.PlaceholdersRegex.Matches(messageJob.MessageTemplate.BodyHtml);
            var replacementTags = new List<string>(matches.Count);
            foreach (Match match in matches)
            {
                replacementTags.Add(match.Value);
            }

            return replacementTags;
        }

        private void AddGlobalProperties(SG.SendGridMessage message, IMessageJobRequest messageJob)
        {
            message.From = new MailAddress(messageJob.SenderEmailAddress, messageJob.SenderName);
            message.Subject = messageJob.MessageTemplate.Subject;
            message.Text = messageJob.MessageTemplate.PlainTextVersion;
            message.Html = messageJob.MessageTemplate.BodyHtml;
        }

        private void AddCustomHeaders(SG.SendGridMessage message, IDictionary<string, string> customMessageHeaders)
        {
            message.AddUniqueArgs(customMessageHeaders);
        }

        private void InitSettings(SenderProfileElement senderProfile)
        {
            var smtpSenderProfile = (SmtpSenderProfileElement)senderProfile;
            this.Username = smtpSenderProfile.Username;
            this.Password = smtpSenderProfile.Password;
        }
    }
}