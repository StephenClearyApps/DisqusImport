using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DisqusImport
{
    public static class GuidExtensions
    {
        public static byte[] ToBigEndianByteArray(this Guid guid) => guid.ToByteArray().SwapGuidByteArray();

        internal static byte[] SwapGuidByteArray(this byte[] bytes)
        {
            var a = bytes[3] << 24 | bytes[2] << 16 | bytes[1] << 8 | bytes[0];
            var b = (short)(bytes[5] << 8 | bytes[4]);
            var c = (short)(bytes[7] << 8 | bytes[6]);
            var result = new byte[16];
            result[0] = (byte)(a >> 24);
            result[1] = (byte)(a >> 16);
            result[2] = (byte)(a >> 8);
            result[3] = (byte)(a);
            result[4] = (byte)(b >> 8);
            result[5] = (byte)(b);
            result[6] = (byte)(c >> 8);
            result[7] = (byte)(c);
            Array.Copy(bytes, 8, result, 8, 8);
            return result;
        }
    }

    public static class GuidFactory
    {
        public static Guid CreateFromLittleEndianByteArray(byte[] bytes) => new Guid(bytes);

        public static Guid CreateFromBigEndianByteArray(byte[] bytes) => new Guid(bytes.SwapGuidByteArray());

        /// <summary>
        /// Creates a SHA-1 name-based GUID.
        /// </summary>
        /// <param name="namespace">The namespace.</param>
        /// <param name="bytes">The bytes to hash.</param>
        public static Guid CreateSha1(Guid @namespace, byte[] bytes)
        {
            var namespaceBytes = @namespace.ToBigEndianByteArray();

            byte[] hash;
            using (var algorithm = SHA1.Create())
            {
                algorithm.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
                algorithm.TransformFinalBlock(bytes, 0, bytes.Length);
                hash = algorithm.Hash;
            }

            var guidBytes = new byte[16];
            Array.Copy(hash, 0, guidBytes, 0, 16);

            // Version 5
            guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | (5 << 4));

            // Variant RFC4122
            guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

            return CreateFromBigEndianByteArray(guidBytes);
        }
    }
}
