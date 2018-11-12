using System;
using System.Collections.Generic;
using Telerik.Sitefinity.Services.Notifications;
using Telerik.Sitefinity.Services.Notifications.Model;

namespace Telerik.Sitefinity.Newsletters.SendGrid.Test.Mocks
{
    internal class SubscriberMock : ISubscriberResponse, INotifiable
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ResolveKey { get; set; }
        public bool Disabled { get; set; }
        public IDictionary<string, string> CustomProperties { get; set; }
        public bool IsNotified { get; set; }
        public SendResultType Result { get; set; }
        public int RetryCount { get; set; }
    }
}
