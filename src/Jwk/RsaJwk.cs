using Newtonsoft.Json;

namespace DisqusImport.Jwk
{
    public sealed class RsaJwk
    {
        // See RFC7517
        [JsonProperty("kty")]
        public string KeyType { get; set; }

        // See RFC7517
        [JsonProperty("alg")]
        public string Algorithm { get; set; }

        // See RFC7518, 6.3
        [JsonProperty("n"), JsonConverter(typeof(Base64UrlConverter))]
        public byte[] Modulus { get; set; }

        // See RFC7518, 6.3
        [JsonProperty("e"), JsonConverter(typeof(Base64UrlConverter))]
        public byte[] Exponent { get; set; }

        // See RFC7518, 6.3
        [JsonProperty("d", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(Base64UrlConverter))]
        public byte[] PrivateExponent { get; set; }

        // See RFC7518, 6.3
        [JsonProperty("p", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(Base64UrlConverter))]
        public byte[] FirstPrimeFactor { get; set; }

        // See RFC7518, 6.3
        [JsonProperty("q", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(Base64UrlConverter))]
        public byte[] SecondPrimeFactor { get; set; }

        // See RFC7518, 6.3
        [JsonProperty("dp", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(Base64UrlConverter))]
        public byte[] FirstFactorCrtExponent { get; set; }

        // See RFC7518, 6.3
        [JsonProperty("dq", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(Base64UrlConverter))]
        public byte[] SecondFactorCrtExponent { get; set; }

        // See RFC7518, 6.3
        [JsonProperty("qi", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(Base64UrlConverter))]
        public byte[] FirstCrtCoefficient { get; set; }
    }
}
