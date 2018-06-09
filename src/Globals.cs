using System;
using System.Collections.Generic;
using System.Text;

public static class Globals
{
    public static Encoding Utf8 { get; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
}