using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SendGrid.Helpers.Mail;
using ServiceStack.Text;
using System.Collections.Generic;
using Telerik.JustMock;
using Telerik.Sitefinity.Newsletters.SendGrid.Notifications;
using Telerik.Sitefinity.Services.Notifications;

namespace Telerik.Sitefinity.Newsletters.SendGrid.Test
{
    /// <summary>
    /// Contains SendGrid sender related tests.
    /// </summary>
    [TestClass]
    public class SendGridSenderTests
    {
        /// <summary>
        /// Tests the message construction.
        /// </summary>
        [TestMethod]
        public void TestMessageConstruction()
        {
            var sender = new SendGridSender();

            var messageJob = this.MockMessageJob();
            var subscribers = this.MockSubscribers();

            var message = sender.ConstructMessage(messageJob, subscribers);

            var expected = subscribers.Select(s => s.Email).ToList();
            var actual = message.Personalizations.SelectMany(x => x.Tos).Select(x => x.Email).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
            Assert.AreEqual(messageJob.MessageTemplate.BodyHtml, message.HtmlContent);
            Assert.AreEqual(messageJob.MessageTemplate.PlainTextVersion, message.PlainTextContent);
            this.AssertHeader(message);
        }

        private void AssertHeader(SendGridMessage message)
        {
            var headerObj = JsonObject.Parse(message.Serialize());
            var personalizations = headerObj.ArrayObjects("personalizations");

            var personalization = personalizations[0];
            var substitutions = personalization.ArrayObjects("substitutions");

            var firstNameSubstitution = substitutions[0][FirstNamePlaceholder];
            Assert.AreEqual(Subscriber1FirstName, firstNameSubstitution);

            var lastNameSubstitution = substitutions[0][LastNamePlaceholder];
            Assert.AreEqual(Subscriber1LastName, lastNameSubstitution);

            var emailSubstitution = substitutions[0][EmailPlaceholder];
            Assert.AreEqual(Subscriber1Email, emailSubstitution);

            personalization = personalizations[1];
            substitutions = personalization.ArrayObjects("substitutions");

            firstNameSubstitution = substitutions[0][FirstNamePlaceholder];
            Assert.AreEqual(Subscriber2FirstName, firstNameSubstitution);

            lastNameSubstitution = substitutions[0][LastNamePlaceholder];
            Assert.AreEqual(Subscriber2LastName, lastNameSubstitution);

            emailSubstitution = substitutions[0][EmailPlaceholder];
            Assert.AreEqual(Subscriber2Email, emailSubstitution);

            var uniqueArgs = headerObj.Object("custom_args");

            var campaingId = uniqueArgs.Get(CampaignCustomHeaderKey);
            Assert.AreEqual(CampaignCustomHeaderValue, campaingId);

            var subscriberId = uniqueArgs.Get(SubscriberCustomHeaderKey);
            Assert.AreEqual(SubscriberCustomHeaderValue, subscriberId);
        }
        
        private ICollection<ISubscriberResponse> MockSubscribers()
        {
            return new List<ISubscriberResponse>()
            {
                // Creating the first subscriber with null for last name to check that null values are not skipped in the 
                // resulting SendGrid message which will mess the whole substitution.
                this.CreateSubscriber(Subscriber1Email, Subscriber1FirstName, null, Subscriber1ResolveKey),
                this.CreateSubscriber(Subscriber2Email, Subscriber2FirstName, Subscriber2LastName, Subscriber2ResolveKey)
            };
        }
 
        private ISubscriberResponse CreateSubscriber(string email, string firstName, string lastName, string resolveKey)
        {
            ISubscriberResponse subscriberOne = Mock.Create<ISubscriberResponse>();
            Mock.Arrange(() => subscriberOne.Email).Returns(email);
            Mock.Arrange(() => subscriberOne.FirstName).Returns(firstName);
            Mock.Arrange(() => subscriberOne.LastName).Returns(lastName);
            Mock.Arrange(() => subscriberOne.ResolveKey).Returns(resolveKey);
            return subscriberOne;
        }

        private IMessageJobRequest MockMessageJob()
        {
            var messageJob = Mock.Create<IMessageJobRequest>();
            Mock.Arrange(() => messageJob.MessageTemplate.BodyHtml).Returns(SendGridSenderTests.HtmlTemplate);
            Mock.Arrange(() => messageJob.MessageTemplate.PlainTextVersion).Returns(SendGridSenderTests.HtmlTemplate);
            Mock.Arrange(() => messageJob.SenderEmailAddress).Returns(SenderEmailAddress);
            Mock.Arrange(() => messageJob.SenderName).Returns(SenderName);
            Mock.Arrange(() => messageJob.CustomMessageHeaders).Returns(new Dictionary<string, string> { { CampaignCustomHeaderKey, CampaignCustomHeaderValue }, { SubscriberCustomHeaderKey, SubscriberCustomHeaderValue } });

            return messageJob;
        }

        private readonly static string SenderEmailAddress = "noreply@telerik.com";
        private readonly static string SenderName = "telerik";
        private readonly static string HtmlTemplate =
            @"Hi {|Subscriber.FirstName|},A test email. LastName: {|Subscriber.LastName|}Email: {|Subscriber.Email|}iss 6 subjectiss 6teleriknoreply@telerik.comall";

        private readonly static string FirstNamePlaceholder = "{|Subscriber.FirstName|}";
        private readonly static string LastNamePlaceholder = "{|Subscriber.LastName|}";
        private readonly static string EmailPlaceholder = "{|Subscriber.Email|}";
        private readonly static string ResolveKeyPlaceholder = "{|Subscriber.ResolveKey|}";

        private readonly static string Subscriber1FirstName = "John";
        private readonly static string Subscriber1LastName = string.Empty;
        private readonly static string Subscriber1Email = "John.Smith@telerik.com";
        private readonly static string Subscriber1ResolveKey = "4bb0d672-f33b-69db-9dcf-ff0000f26bea";

        private readonly static string Subscriber2FirstName = "Carl";
        private readonly static string Subscriber2Email = "Carl.Lee@telerik.com";
        private readonly static string Subscriber2LastName = "Lee";
        private readonly static string Subscriber2ResolveKey = "52b0d672-f33b-69db-9dcf-ff0000f26bea";

        private readonly static string CampaignCustomHeaderKey = "X-Sitefinity-Campaign";
        private readonly static string CampaignCustomHeaderValue = "f1bad672-f33b-69db-9dcf-ff0000f26bea";
        private readonly static string SubscriberCustomHeaderKey = "X-Sitefinity-Subscriber";
        private readonly static string SubscriberCustomHeaderValue = ResolveKeyPlaceholder;
    }
}