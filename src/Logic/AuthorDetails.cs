using System;
using System.Collections.Generic;
using System.Text;

namespace DisqusImport.Logic
{
    public sealed class AuthorDetails
    {
        public string EncryptedEmail { get; set; }

        public string HashedEmail { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string FallbackAvatar { get; set; }

        public string Username { get; set; }
    }
}
