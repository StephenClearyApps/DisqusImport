using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DisqusImport
{
    class Program
    {
        static void Main(string[] args)
        {
            // Note: "post" in the Disqus world is a single comment.
            //   "post" in the Staticman world is a page; this is what Disqus calls a "thread".

            var ser = new XmlSerializer(typeof(disqus));
            using (var reader = XmlReader.Create("disqus.xml"))
            {
                var data = (disqus)ser.Deserialize(reader);
                var threadLookup = data.thread.ToDictionary(x => x.id1, x => x.link);
                foreach (var threadedPosts in data.post.GroupBy(x => x.thread.id))
                {
                    // Use `thread/link` from XML as our Staticman `post_id`
                    var uri = new Uri(threadLookup[threadedPosts.Key]);
                    var staticmanPostId = Path.ChangeExtension(uri.AbsolutePath, "").Replace('/', '_').Trim('_');

                    foreach (var post in threadedPosts.Where(x => !x.isDeleted))
                    {
                        // Write out to files that match staticman.yml 'path' and 'filename' settings.
                        if (!Directory.Exists(Path.Combine("raw", staticmanPostId)))
                            Directory.CreateDirectory(Path.Combine("raw", staticmanPostId));

                        var result = Convert(staticmanPostId, post);
                        var filename = result.Date.ToString("yyyy-MM-dd") + "-" + result.Id.ToString("D") + ".json";
                        File.WriteAllText(Path.Combine("raw", staticmanPostId, filename), JsonConvert.SerializeObject(result, SerializerSettings));
                    }
                }
            }
            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        static JsonModel Convert(string staticmanPostId, post post)
        {
            return new JsonModel
            {
                DisqusId = post.id1,
                DiqusParentId = post.parent?.id,
                PostId = staticmanPostId,
                UserId = post.author.username == null ? "" : "disqus:" + post.author.username,
                Name = post.author.name,
                Message = post.message,
                Date = post.createdAt,
            };
        }

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };
    }
}
