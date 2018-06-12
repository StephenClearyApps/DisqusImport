﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using DisqusImport.Jwk;
using DisqusImport.Logic;
using Newtonsoft.Json;

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

            var publicKey = JsonConvert.DeserializeObject<RsaJwk>(File.ReadAllText("key.public.json")).ToRSA();
            var privateKey = JsonConvert.DeserializeObject<RsaJwk>(File.ReadAllText("key.private.json")).ToRSA();
            var converter = new DisqusConverter(publicKey, privateKey);

            var ser = new XmlSerializer(typeof(disqus));
            using (var reader = XmlReader.Create("disqus.xml"))
            {
                var data = (disqus)ser.Deserialize(reader);
                Preprocess(data);
                converter.Import(data);
            }
            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        private static void Preprocess(disqus data)
        {
            foreach (var post in data.post)
            {
                if (post.author.email != null && (post.author.email.EndsWith(".disqus.net", StringComparison.InvariantCultureIgnoreCase) ||
                                                  (post.author.email.StartsWith("anonymized", StringComparison.InvariantCultureIgnoreCase) && post.author.email.EndsWith("disqus.com", StringComparison.InvariantCultureIgnoreCase))))
                {
                    if (post.author.email.Contains("cleary", StringComparison.InvariantCultureIgnoreCase))
                        post.author.email = "honeypot.ourteddybear@xoxy.net";
                    else
                        post.author.email = null;
                }
                else if (post.author.email == "disqus.ourteddybear@xoxy.net")
                {
                    post.author.email = "honeypot.ourteddybear@xoxy.net";
                }
                //else if (post.author.email != null && (post.author.email.Contains("disqus", StringComparison.InvariantCultureIgnoreCase) || post.author.email.Contains("cleary", StringComparison.InvariantCultureIgnoreCase)))
                //{
                //    Console.WriteLine(post.author.email);
                //}

                if (post.author.name == "Stephen Cleary, Nito Programs")
                    post.author.name = "Stephen Cleary";

                if (post.author.name.Contains("cleary", StringComparison.InvariantCultureIgnoreCase) && post.author.name != "Stephen Cleary")
                    Console.WriteLine(post.author.name);
            }
        }
    }
}
