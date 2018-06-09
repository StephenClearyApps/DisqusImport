using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using DisqusImport.Jwk;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nito.Guids;
using static Globals;

namespace DisqusImport
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists("key.public.json"))
            {
                Console.WriteLine("Creating key files key.private.json and key.public.json. Be sure to keep your private key private!");
                var key = (Rsa: RSA.Create(4096), Padding: RSAEncryptionPadding.OaepSHA1);
                File.WriteAllText("key.private.json", JsonConvert.SerializeObject(key.Rsa.ToJwk(key.Padding, includePrivateKey: true)));
                File.WriteAllText("key.public.json", JsonConvert.SerializeObject(key.Rsa.ToJwk(key.Padding, includePrivateKey: false)));
            }

            var (rsa, padding) = JsonConvert.DeserializeObject<RsaJwk>(File.ReadAllText("key.public.json")).ToRSA();

            // Note: "post" in the Disqus world is a single comment.
            //   "post" in the Staticman world is a page; this is what Disqus calls a "thread".

            var ser = new XmlSerializer(typeof(disqus));
            using (var reader = XmlReader.Create("disqus.xml"))
            {
                var data = (disqus)ser.Deserialize(reader);
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

                        var result = Convert(staticmanPostId, post, rsa, padding);
                        var filename = result.Date.ToString("yyyy-MM-dd") + "-" + result.Id.ToString("D") + ".json";
                        File.WriteAllText(Path.Combine("raw", staticmanPostId, filename), JsonConvert.SerializeObject(result, SerializerSettings));
                    }
                }
            }
            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        static JsonModel Convert(string staticmanPostId, post post, RSA rsa, RSAEncryptionPadding padding)
        {
            return new JsonModel
            {
                DisqusId = post.id1,
                DiqusParentId = post.parent?.id,
                PostId = staticmanPostId,
                AuthorUserId = post.author.username == null ? "" : "disqus:" + post.author.username,
                AuthorName = post.author.name,
                AuthorEmailMD5 = post.author.email == null ? "" : EmailMd5(post.author.email),
                AuthorEmailEncrypted = post.author.email == null ? "" : EmailEncrypt(post.author.email, rsa, padding),
                Message = post.message,
                Date = post.createdAt,
            };
        }

        private static string EmailEncrypt(string email, RSA rsa, RSAEncryptionPadding padding)
        {
            // 1) Convert to UTF8.
            var bytes = Utf8.GetBytes(email);

            // 2) Encrypt.
            var encrypted = rsa.Encrypt(bytes, padding);

            // 3) Base64-encode.
            return System.Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Converts an email string to a Gravatar-compatible MD5 hash. See https://en.gravatar.com/site/implement/hash/
        /// </summary>
        private static string EmailMd5(string email)
        {
            // 1) Trim leading and trailing space characters.
            email = email.Trim(' ');
            if (email == "")
                return "";

            // 2) Force all characters to lowercase.
            email = email.ToLowerInvariant();

            // 3) (assumed) UTF8-encode.
            var bytes = Utf8.GetBytes(email);

            // 4) MD5 hash
            var hash = Md5.ComputeHash(bytes);

            // 5) (assumed from example) Convert to lowercase hex string.
            return hash.ToLowercaseHexString();
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

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };
    }
}
