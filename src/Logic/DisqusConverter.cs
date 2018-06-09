using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Nito.Guids;
using static Globals;

namespace DisqusImport.Logic
{
    public sealed class DisqusConverter
    {
        private readonly DisqusCommentConverter _commentConverter;

        public DisqusConverter((RSA Rsa, RSAEncryptionPadding Padding) key)
        {
            _commentConverter = new DisqusCommentConverter(key);
        }

        public void Convert(disqus data)
        {
            // Note: "post" in the Disqus world is a single comment.
            //   "post" in the Staticman world is a page; this is what Disqus calls a "thread".

            var threadLookup = data.thread.ToDictionary(x => x.id1, x => x.link);
            foreach (var threadedPosts in data.post.GroupBy(x => x.thread.id))
            {
                // Use `thread/link` from XML as the basis of our Staticman `post_id`
                var threadLink = threadLookup[threadedPosts.Key];
                var staticmanPostId = StaticmanPostId(threadLink);

                foreach (var post in threadedPosts.Where(x => !x.isDeleted))
                {
                    // Write out to files that match staticman.yml 'path' and 'filename' settings.
                    if (!Directory.Exists(Path.Combine("raw", staticmanPostId)))
                        Directory.CreateDirectory(Path.Combine("raw", staticmanPostId));

                    var result = _commentConverter.Convert(staticmanPostId, post);
                    var filename = result.Date.ToString("yyyy-MM-dd") + "-" + result.Id.ToString("D") + ".json";
                    File.WriteAllText(Path.Combine("raw", staticmanPostId, filename), JsonConvert.SerializeObject(result, SerializerSettings));
                }
            }
        }

        private static readonly Regex Extension = new Regex(@"\.[A-Za-z]{3}$");

        private static string StaticmanPostId(string url)
        {
            // 1) If the URL starts with http://, replace with https://
            if (url.StartsWith("http://"))
                url = "https://" + url.Substring(7);

            // 2) Take the URL path (not including scheme and domain).
            var uri = new Uri(url);
            var path = uri.AbsolutePath;

            // 3) Path: If the path ends with .html or any three-character [A-Za-z] extension, strip it.
            if (path.EndsWith(".html"))
                path = path.Substring(0, path.Length - 5);
            else if (Extension.IsMatch(path))
                path = path.Substring(0, path.Length - 4);

            // 4) Path: For each UTF-16 code unit, if it's not in the whitelist [A-Za-z0-9-_;.~()], then replace it with _.
            path = GitFilename(path);

            // 5) Path: Trim leading and trailing _ characters.
            path = path.Trim('_');

            // 6) Calculate the V3 URL GUID for the URL.
            var guid = GuidFactory.CreateMd5(GuidNamespaces.Url, Utf8.GetBytes(url));

            // 7) Combine the path and lowercase-hyphenated GUID, separated by '-'.
            return path + "-" + guid.ToString("D");

        }

        private static string GitFilename(string input)
        {
            // Git has a lot of problems with non-ASCII characters.
            var sb = new StringBuilder(input.Length);
            foreach (var ch in input)
            {
                if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '-' ||
                    ch == '_' || ch == ';' || ch == '.' || ch == '~' || ch == '(' || ch == ')')
                    sb.Append(ch);
                else
                    sb.Append('_');
            }

            return sb.ToString();
        }
    }
}
