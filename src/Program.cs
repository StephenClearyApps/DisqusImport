using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using DisqusImport.Jwk;
using DisqusImport.Logic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nito.Guids;
using static Globals;

namespace DisqusImport
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists("key.public.json"))
            {
                Console.WriteLine("Creating key files key.private.json and key.public.json. Be sure to keep your private key private!");
                var (rsa, padding) = (RSA.Create(4096), RSAEncryptionPadding.OaepSHA1);
                File.WriteAllText("key.private.json", JsonConvert.SerializeObject(rsa.ToJwk(padding, includePrivateKey: true)));
                File.WriteAllText("key.public.json", JsonConvert.SerializeObject(rsa.ToJwk(padding, includePrivateKey: false)));
            }

            var key = JsonConvert.DeserializeObject<RsaJwk>(File.ReadAllText("key.public.json")).ToRSA();
            var converter = new DisqusConverter(key);

            var ser = new XmlSerializer(typeof(disqus));
            using (var reader = XmlReader.Create("disqus.xml"))
            {
                var data = (disqus)ser.Deserialize(reader);
                converter.Convert(data);
            }
            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
