using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Telerik.Sitefinity.Newsletters.SendGrid
{
    /// <summary>
    /// Contains Sitefinity newsletter templates placeholders related constants.
    /// </summary>
    public class NewsletterTemplatesConstants
    {
        /// <summary>
        /// The placeholder start tag. 
        /// </summary>
        /// <remarks>
        /// Example placeholder: {|Subscriber.ResolveKey|}
        /// </remarks>
        public static readonly string PlaceholdersStartTag = "{|";
        
        /// <summary>
        /// The placeholder end tag. 
        /// </summary>
        /// <remarks>
        /// Example placeholder: {|Subscriber.ResolveKey|}
        /// </remarks>
        public static readonly string PlaceholderEndTag = "|}";

        /// <summary>
        /// Placeholders matching regular expression.
        /// </summary>
        public static readonly Regex PlaceholdersRegex = new Regex(@"\{\|([a-zA-Z0-9\s_-]+?)\.([a-zA-Z0-9\s_-]+?)\|\}", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }
}
