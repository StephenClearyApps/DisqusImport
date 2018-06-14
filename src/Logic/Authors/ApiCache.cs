using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace DisqusImport.Logic.Authors
{
    public sealed class ApiCache
    {
        private List<ApiAuthorData> _data;

        public ApiCache()
        {
            _data = File.Exists("apicache.json") ? JsonConvert.DeserializeObject<List<ApiAuthorData>>(File.ReadAllText("apicache.json")) : new List<ApiAuthorData>();
        }

        public ApiAuthorData TryGetByHashedEmail(string hashedEmail)
        {
            if (hashedEmail == null)
                return null;
            return _data.FirstOrDefault(x => x.HashedEmail == hashedEmail);
        }

        public ApiAuthorData TryGetByUsername(string username)
        {
            if (username == null)
                return null;
            return _data.FirstOrDefault(x => x.Username == username);
        }

        public ApiAuthorData TryGetByPostId(string postId) => _data.FirstOrDefault(x => x.PostId == postId);

        public void Save(ApiAuthorData apiData)
        {
            _data.Add(apiData);
            File.WriteAllText("apicache.json", JsonConvert.SerializeObject(_data));
        }
    }
}
