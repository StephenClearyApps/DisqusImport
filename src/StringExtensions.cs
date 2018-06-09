using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DisqusImport
{
    public static class StringExtensions
    {
        public static string ToLowercaseHexString(this IEnumerable<byte> bytes) => string.Join("", bytes.Select(x => x.ToString("x2")));
    }
}
