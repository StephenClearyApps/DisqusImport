using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DisqusImport.Logic
{
    public sealed class GravatarApi
    {
        private readonly HttpClient _client = new HttpClient();

        public async Task<bool> AvatarExists(string emailHash)
        {
            var url = $"https://www.gravatar.com/avatar/{emailHash}?d=404";
            var response = await _client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
                return true;
            if (response.StatusCode == HttpStatusCode.NotFound)
                return false;
            throw new InvalidOperationException("Unexpected response from gravatar.");
        }
    }
}
