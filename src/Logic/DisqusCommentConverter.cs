using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static Globals;

namespace DisqusImport.Logic
{
    /// <summary>
    /// Converts a single Disqus comment.
    /// </summary>
    public sealed class DisqusCommentConverter
    {
        private static readonly Regex AnchorFixer = new Regex(@"<a href=""([^""]+)"" rel=""[^""]+"" title=""[^""]+"">([^<]+)</a>");
        private readonly (RSA Rsa, RSAEncryptionPadding Padding) _key;

        public DisqusCommentConverter((RSA Rsa, RSAEncryptionPadding Padding) key)
        {
            _key = key;
        }

        public JsonModel Convert(string staticmanPostId, post post)
        {
            return new JsonModel
            {
                DisqusId = post.id1,
                DiqusParentId = post.parent?.id,
                PostId = staticmanPostId,
                AuthorUserId = post.author.username == null ? "" : "disqus:" + post.author.username,
                AuthorName = post.author.name,
                AuthorEmailMD5 = post.author.email == null ? "" : EmailMd5(post.author.email),
                AuthorEmailEncrypted = post.author.email == null ? "" : EmailEncrypt(post.author.email),
                Message = ConvertMessage(post.message),
                Date = post.createdAt,
            };
        }

        private static string ConvertMessage(string message)
        {
            // Strip a@title attributes (which don't work with showdown), and undo Disqus link shortening.
            var html = AnchorFixer.Replace(message, match =>
            {
                if (match.Groups[2].Value.EndsWith("..."))
                {
                    var shortened = match.Groups[2].Value.Substring(0, match.Groups[2].Value.Length - 3);
                    if (match.Groups[1].Value.StartsWith(shortened))
                        return $"<a href=\"{match.Groups[1].Value}\">{match.Groups[1].Value}</a>";
                }

                return $"<a href=\"{match.Groups[1].Value}\">{match.Groups[2].Value}</a>";
            });
            return MarkdownConverter.Convert(html);
        }

        /// <summary>
        /// Encrypts an email string.
        /// </summary>
        private string EmailEncrypt(string email)
        {
            // 1) Convert to UTF8.
            var bytes = Utf8.GetBytes(email);

            // 2) Encrypt.
            var encrypted = _key.Rsa.Encrypt(bytes, _key.Padding);

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
    }
}
