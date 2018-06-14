using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DisqusImport.Logic.Authors
{
    /// <summary>
    /// Author information gleaned from the Disqus xml import file.
    /// </summary>
    public sealed class XmlAuthorData
    {
        private readonly (RSA Rsa, RSAEncryptionPadding Padding) _publicKey;
        private readonly post _post;

        private XmlAuthorData((RSA Rsa, RSAEncryptionPadding Padding) publicKey, post post)
        {
            _publicKey = publicKey;
            _post = post;
        }

        /// <summary>
        /// The author's name. Never <c>null</c>.
        /// </summary>
        public string Name => _post.author.name.NullIfEmpty();

        /// <summary>
        /// The Disqus username. May be <c>null</c>.
        /// </summary>
        public string Username => _post.author.username.NullIfEmpty();

        /// <summary>
        /// The hashed email. May be <c>null</c>.
        /// </summary>
        public string HashedEmail => _post.author.email.NullIfEmpty() == null ? null : ConvertUtil.HashEmail(_post.author.email);

        /// <summary>
        /// The encrypted email. May be <c>null</c>.
        /// </summary>
        public string EncryptedEmail => _post.author.email.NullIfEmpty() == null ? null : ConvertUtil.EncryptEmail(_post.author.email, _publicKey.Rsa, _publicKey.Padding);

        public static XmlAuthorData Create((RSA Rsa, RSAEncryptionPadding Padding) publicKey, post post) => new XmlAuthorData(publicKey, post);
    }
}
