using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using static Globals;

namespace DisqusImport.Logic
{
    public static class ConvertUtil
    {
        /// <summary>
        /// Encrypts an email string.
        /// </summary>
        public static string EncryptEmail(string email, RSA rsa, RSAEncryptionPadding padding)
        {
            // 1) Convert to UTF8.
            var bytes = Utf8.GetBytes(email);

            // 2) Encrypt.
            var encrypted = rsa.Encrypt(bytes, padding);

            // 3) Base64-encode.
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Converts an email string to a Gravatar-compatible MD5 hash. See https://en.gravatar.com/site/implement/hash/
        /// </summary>
        public static string HashEmail(string email)
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
