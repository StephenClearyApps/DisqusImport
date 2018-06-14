using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace DisqusImport.Logic.Authors
{
    /// <summary>
    /// Author details retrieved from the Disqus API (and saved in the cache).
    /// </summary>
    public sealed class ApiAuthorData
    {
        /// <summary>
        /// The MD5-hashed email, suitable for use with Gravatar.
        /// </summary>
        public string HashedEmail { get; set; }

        /// <summary>
        /// Disqus username. May be <c>null</c>.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Whether the author has uploaded a Disqus avatar.
        /// </summary>
        public bool HasAvatar { get; set; }

        /// <summary>
        /// A link for the author. May be <c>null</c>.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The first post id in the xml associated with this author.
        /// </summary>
        public string PostId { get; set; }

        public static ApiAuthorData CreateFromPostDetails(JObject response)
        {
            var hasCustomAvatar = response?["response"]?["author"]?["avatar"]?["isCustom"];
            return new ApiAuthorData
            {
                PostId = ((string)response?["response"]?["id"]).NullIfEmpty(),
                Username = ((string)response?["response"]?["author"]?["username"]).NullIfEmpty(),
                HashedEmail = ((string)response?["response"]?["author"]?["emailHash"]).NullIfEmpty(),
                Url = ((string)response?["response"]?["author"]?["url"]).NullIfEmpty(),
                HasAvatar = hasCustomAvatar != null && (bool)hasCustomAvatar,
            };
        }
    }
}
