using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using Newtonsoft.Json;

namespace PhotosService.Services
{
    public static class SignedUrlHelpers
    {
        private static readonly Lazy<string> privateKeyXmlString = new Lazy<string>(LoadPrivateKeyXmlString);

        public static string CreateSignedUrl(string resourceUrl, DateTime startUtc, DateTime endUtc)
        {
            var urlBuilder = new UriBuilder(resourceUrl);
            
            var policyString = CreatePolicyString(urlBuilder.ToString(), startUtc, endUtc);
            var policyBytes = Encoding.ASCII.GetBytes(policyString);
            var policyUrlSafeString = ToUrlSafeBase64String(policyBytes);

            var signatureBytes = CreateSignedHash(policyBytes);
            var signatureUrlSafeString = ToUrlSafeBase64String(signatureBytes);

            var queryStringCollection = HttpUtility.ParseQueryString(urlBuilder.Query);
            queryStringCollection.Add("policy", policyUrlSafeString);
            queryStringCollection.Add("signature", signatureUrlSafeString);
            urlBuilder.Query = queryStringCollection.ToString();

            return urlBuilder.ToString();
        }

        public static bool CheckSignedUrl(string signedUrl)
        {
            var urlBuilder = new UriBuilder(signedUrl);
            var queryStringCollection = HttpUtility.ParseQueryString(urlBuilder.Query);

            var expectedSignatureBytes = FromUrlSafeBase64String(queryStringCollection["signature"]);
            
            var policyBytes = FromUrlSafeBase64String(queryStringCollection["policy"]);
            var actualSignatureBytes = CreateSignedHash(policyBytes);
            var policyString = Encoding.ASCII.GetString(policyBytes);
            var policy = JsonConvert.DeserializeObject<SignedUrlPolicy>(policyString);

            queryStringCollection.Remove("policy");
            queryStringCollection.Remove("signature");
            urlBuilder.Query = queryStringCollection.ToString();
            var resourceUrl = urlBuilder.ToString();

            var nowTimestamp = ToUnixTimestamp(DateTime.UtcNow);

            if (!resourceUrl.Equals(policy.ResourceUrl))
                return false;

            if (nowTimestamp < policy.StartTimestamp || policy.EndTimestamp < nowTimestamp)
                return false;

            if (!AreByteArraysEqual(expectedSignatureBytes, actualSignatureBytes))
                return false;

            return true;
        }

        private static string CreatePolicyString(string resourceUrl, DateTime startUtc, DateTime endUtc)
        {
            var startTimestamp = ToUnixTimestamp(startUtc);
            var endTimestamp = ToUnixTimestamp(endUtc);

            if (startTimestamp > endTimestamp)
                throw new ArgumentException();

            var policy = new SignedUrlPolicy
            {
                ResourceUrl = resourceUrl,
                StartTimestamp = startTimestamp,
                EndTimestamp = endTimestamp
            };

            return JsonConvert.SerializeObject(policy);
        }

        private static byte[] CreateSignedHash(byte[] content)
        {
            byte[] hash = null;
            using (var cryptoSHA1 = new SHA1CryptoServiceProvider())
                hash = cryptoSHA1.ComputeHash(content);

            var rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.FromXmlString(privateKeyXmlString.Value);

            var rsaFormatter = new RSAPKCS1SignatureFormatter(rsaProvider);
            rsaFormatter.SetHashAlgorithm("SHA1");

            var signedHash = rsaFormatter.CreateSignature(hash);
            return signedHash;
        }

        private static string LoadPrivateKeyXmlString()
        {
            var privateKeyXml = new XmlDocument();
            var keyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Certificates",
                "urlkey.xml");
            privateKeyXml.Load(keyPath);
            return privateKeyXml.InnerXml;
        }

        private static string ToUrlSafeBase64String(byte[] bytes)
        {
            var base64String = Convert.ToBase64String(bytes);
            return base64String
                .Replace('+', '-')
                .Replace('=', '_')
                .Replace('/', '~');
        }

        private static byte[] FromUrlSafeBase64String(string urlSafeBase64String)
        {
            var base64String = urlSafeBase64String
                .Replace('-', '+')
                .Replace('_', '=')
                .Replace('~', '/');
            return Convert.FromBase64String(base64String);
        }

        private static readonly DateTime UnixTimeStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static int ToUnixTimestamp(DateTime dateTime) => (int)((dateTime - UnixTimeStart).TotalSeconds);

        private static bool AreByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null || a.Length != b.Length)
                return false;

            var areSame = true;
            for (var i = 0; i < a.Length; i++)
                areSame &= (a[i] == b[i]);
            return areSame;
        }

        private class SignedUrlPolicy
        {
            public string ResourceUrl { get; set; }
            public int StartTimestamp { get; set; }
            public int EndTimestamp { get; set; }
        }
    }
}