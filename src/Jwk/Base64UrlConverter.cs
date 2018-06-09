using System;
using System.IO;
using Newtonsoft.Json;

namespace DisqusImport.Jwk
{
    public sealed class Base64UrlConverter : JsonConverter<byte[]>
    {
        public override void WriteJson(JsonWriter writer, byte[] value, JsonSerializer serializer)
        {
            var encodedValue = Base64UrlEncode(value);
            serializer.Serialize(writer, encodedValue);
        }

        public override byte[] ReadJson(JsonReader reader, Type objectType, byte[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var encodedValue = (string)reader.Value;
            return Base64UrlDecode(encodedValue);
        }

        // From RFC7515
        private static string Base64UrlEncode(byte[] arg)
        {
            string s = Convert.ToBase64String(arg); // Regular base64 encoder
            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding
            return s;
        }

        // From RFC7515
        private static byte[] Base64UrlDecode(string arg)
        {
            string s = arg;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding
            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: s += "=="; break; // Two pad chars
                case 3: s += "="; break; // One pad char
                default:
                    throw new InvalidDataException("Illegal base64url string.");
            }
            return Convert.FromBase64String(s); // Standard base64 decoder
        }
    }
}