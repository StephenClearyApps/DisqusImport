using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nito.Guids;
using static Globals;

namespace DisqusImport
{
    public sealed class JsonModel
    {
        [JsonProperty("_id")]
        public Guid Id { get; private set; }

        [JsonIgnore]
        public string DisqusId
        {
            set => Id = ConvertDisqusId(value);
        }

        public string Name { get; set; }

        public string Message { get; set; }

        public string PostId { get; set; }

        public string ReplyTo { get; private set; }

        [JsonIgnore]
        public string DiqusParentId
        {
            set => ReplyTo = value == null ? "" : ConvertDisqusId(value).ToString("D");
        }

        public DateTime Date { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Timestamp => Date;

        public string UserId { get; set; }

        public static Guid ConvertDisqusId(string disqusId) => GuidFactory.CreateMd5(DisqusNamespace, Utf8.GetBytes(disqusId));

        private static readonly Guid DisqusNamespace = Guid.Parse("23F48135-168C-4769-8D5C-4693E3F80E03");
    }
}
