using System.IO;
using System.Security.Cryptography;

namespace DisqusImport.Jwk
{
    public static class RsaJwkExtensions
    {
        public static RsaJwk ToJwk(this RSA rsa, RSAEncryptionPadding padding, bool includePrivateKey) => rsa.ExportParameters(includePrivateKey).ToJwk(padding);

        public static RsaJwk ToJwk(this RSAParameters parameters, RSAEncryptionPadding padding)
        {
            return new RsaJwk
            {
                KeyType = "RSA",
                Algorithm = padding.ToAlgorithm(),
                Modulus = parameters.Modulus,
                Exponent = parameters.Exponent,
                PrivateExponent = parameters.D,
                FirstPrimeFactor = parameters.P,
                SecondPrimeFactor = parameters.Q,
                FirstFactorCrtExponent = parameters.DP,
                SecondFactorCrtExponent = parameters.DQ,
                FirstCrtCoefficient = parameters.InverseQ,
            };
        }

        public static (RSA Rsa, RSAEncryptionPadding Padding) ToRSA(this RsaJwk jwk)
        {
            var rsa = RSA.Create(jwk.GetRSAParameters());
            return (rsa, jwk.GetPadding());
        }

        public static RSAParameters GetRSAParameters(this RsaJwk jwk)
        {
            return new RSAParameters
            {
                D = jwk.PrivateExponent,
                DP = jwk.FirstFactorCrtExponent,
                DQ = jwk.SecondFactorCrtExponent,
                Exponent = jwk.Exponent,
                InverseQ = jwk.FirstCrtCoefficient,
                Modulus = jwk.Modulus,
                P = jwk.FirstPrimeFactor,
                Q = jwk.SecondPrimeFactor,
            };
        }

        public static RSAEncryptionPadding GetPadding(this RsaJwk jwk)
        {
            switch (jwk.Algorithm)
            {
                case "RSA-OAEP": return RSAEncryptionPadding.OaepSHA1;
                case "RSA-OAEP-256": return RSAEncryptionPadding.OaepSHA256;
                case "RSA-OAEP-384": return RSAEncryptionPadding.OaepSHA384;
                case "RSA-OAEP-512": return RSAEncryptionPadding.OaepSHA512;
                default: throw new InvalidDataException($"Unsupported JWK algorithm {jwk.Algorithm}");
            }
        }

        private static string ToAlgorithm(this RSAEncryptionPadding padding)
        {
            switch (padding.OaepHashAlgorithm.Name)
            {
                case "SHA1": return "RSA-OAEP";
                case "SHA256": return "RSA-OAEP-256";
                case "SHA384": return "RSA-OAEP-384";
                case "SHA512": return "RSA-OAEP-512";
                default: throw new InvalidDataException($"Unknown hash algorithm {padding.OaepHashAlgorithm.Name}");
            }
        }
    }
}
