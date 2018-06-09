using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public static class Globals
{
    public static Encoding Utf8 { get; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public static MD5 Md5 { get; } = MD5.Create();
}