using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Polly;

namespace DisqusImport.Logic
{
    public sealed class DisqusApi
    {
        private readonly string _accessToken;
        private readonly string _apiKey;
        private readonly HttpClient _client = new HttpClient();
        private readonly Policy _policy = Policy
            .Handle<DisqusApiException>(ex => (int) ex.Details["code"] == 13 || (int) ex.Details["code"] == 14)
            .WaitAndRetryForeverAsync(_ => TimeSpan.FromMinutes(1));

        public DisqusApi(string accessToken, string apiKey)
        {
            _accessToken = accessToken;
            _apiKey = apiKey;
        }

        public async Task<JObject> PostDetailsAsync(string post)
        {
            var url = $"https://disqus.com/api/3.0/posts/details.json?post={post}&access_token={_accessToken}&api_key={_apiKey}";
            return await _policy.ExecuteAsync(async () =>
            {
                var response = await _client.GetAsync(url);
                return await ParseResponse(response);
            });
        }

        private static async Task<JObject> ParseResponse(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(json);
            try
            {
                response.EnsureSuccessStatusCode();
                return result;
            }
            catch (Exception ex)
            {
                throw new DisqusApiException(result, ex);
            }
        }

        private sealed class DisqusApiException : Exception
        {
            public DisqusApiException(JObject details, Exception innerException)
            {
                Details = details;
            }

            public JObject Details { get; }
        }
    }
}
