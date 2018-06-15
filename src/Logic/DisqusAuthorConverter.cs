using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DisqusImport.Logic.Authors;
using Newtonsoft.Json;

namespace DisqusImport.Logic
{
    public sealed class DisqusAuthorConverter
    {
        private readonly DisqusApi _disqusApi;
        private readonly ApiCache _cache;
        private readonly (RSA Rsa, RSAEncryptionPadding Padding) _publicKey;

        public DisqusAuthorConverter(DisqusApi disqusApi, ApiCache cache, (RSA Rsa, RSAEncryptionPadding Padding) publicKey)
        {
            _disqusApi = disqusApi;
            _cache = cache;
            _publicKey = publicKey;
        }

        public async Task<AuthorDetails> ConvertAsync(string filename, post post)
        {
            if (post.author.email == "disqus.ourteddybear@xoxy.net" || post.author.username == "stephen_cleary")
                return Owner;

            var postId = post.id1;
            var postAuthorData = XmlAuthorData.Create(_publicKey, post);
            var apiAuthorData = _cache.TryGetByHashedEmail(postAuthorData.HashedEmail) ?? _cache.TryGetByUsername(postAuthorData.Username) ?? _cache.TryGetByPostId(postId);
            if (apiAuthorData == null)
            {
                apiAuthorData = ApiAuthorData.CreateFromPostDetails(await _disqusApi.PostDetailsAsync(postId));
                _cache.Save(apiAuthorData);
            }

            var existingAuthorData = LoadExisting(filename);

            var result = new AuthorDetails();
            result.EncryptedEmail = existingAuthorData.EncryptedEmail ?? postAuthorData.EncryptedEmail;
            result.HashedEmail = existingAuthorData.HashedEmail ?? apiAuthorData.HashedEmail;
            result.Name = postAuthorData.Name;
            result.Url = apiAuthorData.Url;
            result.Username = apiAuthorData.Username;
            if (apiAuthorData.HasAvatar)
                result.FallbackAvatar = $"https://disqus.com/api/users/avatars/{result.Username}.jpg";
            return result;
        }

        private static AuthorDetails LoadExisting(string filename)
        {
            if (!File.Exists(filename))
                return new AuthorDetails();
            var json = JsonConvert.DeserializeObject<JsonModel>(File.ReadAllText(filename));
            return new AuthorDetails
            {
                HashedEmail = json.AuthorEmailMD5.NullIfEmpty(),
                EncryptedEmail = json.AuthorEmailEncrypted.NullIfEmpty(),
                Url = json.AuthorUri.NullIfEmpty(),
                Name = json.AuthorName.NullIfEmpty(),
                FallbackAvatar = json.AuthorFallbackAvatar.NullIfEmpty(),
                Username = json.AuthorUserId.NullIfEmpty() != null && json.AuthorUserId.StartsWith("disqus:") ? json.AuthorUserId.Substring(7) : null,
            };
        }

        private static readonly AuthorDetails Owner = new AuthorDetails
        {
            Name = "Stephen Cleary",
            HashedEmail = "3db7b6e14d9da42751e4bab03bc4d034",
            Url = "https://stephencleary.com/"
        };
    }
}
