using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.Sitefinity.Newsletters.SendGrid.Notifications;
using Telerik.Sitefinity.Services.Notifications;
using System.Collections.Generic;
using Telerik.JustMock;
using System.Net.Mail;
using SendGrid.SmtpApi;
using ServiceStack.Text;

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

            CollectionAssert.AreEquivalent(subscribers.Select(s => new MailAddress(s.Email)).ToList(), message.To.ToList());
            Assert.AreEqual(messageJob.MessageTemplate.BodyHtml, message.Html);
            Assert.AreEqual(messageJob.MessageTemplate.PlainTextVersion, message.Text);
            this.AssertHeader(message.Header);
        }

        private void AssertHeader(IHeader header)
        {
            var headerObj = JsonObject.Parse(header.JsonString());
            var subs = headerObj.Object("sub");

            var firstNameSubstitutions = subs.Get<string[]>(FirstNamePlaceholder).ToList();
            CollectionAssert.AreEqual(new List<string> { Subscriber1FirstName, Subscriber2FirstName }, firstNameSubstitutions);

            var lastNameSubstitutions = subs.Get<string[]>(LastNamePlaceholder).ToList();
            CollectionAssert.AreEqual(new List<string> { "", Subscriber2LastName }, lastNameSubstitutions);

            var emailSubstitutions = subs.Get<string[]>(EmailPlaceholder).ToList();
            CollectionAssert.AreEqual(new List<string> { Subscriber1Email, Subscriber2Email }, emailSubstitutions);

            var resolveKeySubstitutions = subs.Get<string[]>(ResolveKeyPlaceholder).ToList();
            CollectionAssert.AreEqual(new List<string> { Subscriber1ResolveKey, Subscriber2ResolveKey }, resolveKeySubstitutions);

            var uniqueArgs = headerObj.Object("unique_args");

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