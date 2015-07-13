using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Telerik.Sitefinity.Services.Notifications;

namespace Telerik.Sitefinity.Newsletters.SendGrid.Notifications
{
    /// <summary>
    /// Provides methods for manipulating ISubscriberResponse objects
    /// </summary>
    internal static class SubscriberExtensions
    {
        /// <summary>
        /// Creates a dictionary with merge tags from the subscriber properties.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <returns>The subscriber properties as dictionary.</returns>
        /// <example>
        /// Here is an example content in the dictionary:
        /// <code>
        /// { Key = "Subscriber.Id", Value = "0c43c999-d8fa-6a02-bd4d-ff0000ee95d1" },
        /// { Key = "Subscriber.Email, Value = "example@example.com" },
        /// { Key = "Subscriber.CustomPropertyName", Value = "customPropertyValueAsString" }
        /// </code>
        /// </example>
        public static Dictionary<string, string> ToDictionary(this ISubscriberResponse subscriber)
        {
            var dict = new Dictionary<string, string>();
            dict.Add(GetMergeTag("Id"), subscriber.Id.ToString());
            dict.Add(GetMergeTag("Email"), GetPropertyValue(subscriber.Email));
            dict.Add(GetMergeTag("FirstName"), GetPropertyValue(subscriber.FirstName));
            dict.Add(GetMergeTag("LastName"), GetPropertyValue(subscriber.LastName));
            dict.Add(GetMergeTag("ResolveKey"), GetPropertyValue(subscriber.ResolveKey));

            foreach (var item in subscriber.CustomProperties)
                dict.Add(GetMergeTag(item.Key), item.Value);

            return dict;
        }

        private static string GetMergeTag(string propertyName)
        {
            return string.Format(CultureInfo.InvariantCulture, MergeTagTemplate, propertyName);
        }

        private static string GetPropertyValue(string value)
        {
            return !value.IsNullOrEmpty() ? value : string.Empty;
        }

        private const string MergeTagTemplate = SubscriberCategory + ".{0}";

        /// <summary>
        /// Contains the category used as first part in the subscriber merge tags.
        /// </summary>
        public const string SubscriberCategory = "Subscriber";
    }
}
