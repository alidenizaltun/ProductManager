using System.Security.Cryptography;
using System.Text;

namespace ProductManager.Infrastructure.Extensions
{
    public static class RsaExtension
    {
        public static string ExportPrivateKeyPem(this RSA rsa)
        {
            var privateKey = rsa.ExportPkcs8PrivateKey();
            var base64 = Convert.ToBase64String(privateKey);
            return PemFormat("PRIVATE KEY", base64);
        }
        
        public static string ExportPublicKeyPem(this RSA rsa)
        {
            var publicKey = rsa.ExportSubjectPublicKeyInfo();
            var base64 = Convert.ToBase64String(publicKey);
            return PemFormat("PUBLIC KEY", base64);
        }
        
        // Yardımcı: PEM formatı üretici
        private static string PemFormat(string label, string base64)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"-----BEGIN {label}-----");
            for (int i = 0; i < base64.Length; i += 64)
            {
                int len = Math.Min(64, base64.Length - i);
                sb.AppendLine(base64.Substring(i, len));
            }
            sb.AppendLine($"-----END {label}-----");
            return sb.ToString();
        }
    }
}


