using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using WinHttp;

namespace BotCoin.ApiClient
{
    public class RestApiClient2 : IDisposable
    {
        protected readonly int OrderBookLimit;
        private readonly object _obj;
        public readonly string BaseUri;
        public readonly string PublicKey;
        public readonly string SecretKey;
        protected byte[] ApiSecret;

        public RestApiClient2(string url, string certificate = null)
        {
            _obj = new object();
            BaseUri = url;
        }

        public RestApiClient2(ExchangeSettingsData setting) : this(setting.RestUrl)
        {
            OrderBookLimit = 20;
            SecretKey = setting.SecretKey;
            PublicKey = setting.PublicKey;
            ApiSecret = Encoding.ASCII.GetBytes(SecretKey);
        }

        public ApiResult UserQuery(string path, HttpMethod http, string content = "", bool jsonContent = false)
        {
            lock (_obj)
                return UserQuery(path, http, null, content, jsonContent);
        }

        public string GetQuery(string path, Dictionary<string, string> headers = null, string content = "")
        {
            if (content.Length > 0)
                path = String.Format("{0}?{1}", path, content);

            lock (_obj)
            {
                var result = UserQuery(path, HttpMethod.Get, headers);
                return result.Content;
            }
        }

        private ApiResult UserQuery(string path, HttpMethod http, Dictionary<string, string> headers = null, string content = "", bool jsonContent = false)
        {
            var methodName = http.Method.ToString();
            var winHttp = new WinHttpRequest();

            path = BaseUri + path;

            if (methodName == "POST" && content.Length == 0)
                throw new ArgumentException("Empty content.");

            if (headers != null && headers.Count == 0)
                throw new ArgumentException("Empty headers.");

            winHttp.Open(methodName, path, true);

            if (http == HttpMethod.Post)
                winHttp.SetRequestHeader("Content-Type", jsonContent ? "application/json" : "application/x-www-form-urlencoded");

            if (headers != null)
            {
                foreach (var header in headers)
                    winHttp.SetRequestHeader(header.Key, header.Value);
            }

            winHttp.Send(content);
            winHttp.WaitForResponse();

            if (winHttp.Status != 200)
                throw new InvalidOperationException(String.Format("{0} {1} {2} {3}", winHttp.StatusText, methodName, path, winHttp.ResponseText));

            ParseResponseHeaders();
            return new ApiResult(winHttp.Status, winHttp.ResponseText);
        }

        protected virtual void ParseResponseHeaders()
        {
        }
                
        internal string CreateSignature256(string message)
        {
            using (var hmac = new HMACSHA256(ApiSecret))
            {
                var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(message));
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            }
        }

        internal byte[] CreateHash256(string message)
        {
            using (SHA256 hash = SHA256Managed.Create())
                return hash.ComputeHash(Encoding.UTF8.GetBytes(message));
        }

        internal string CreateSignature512(string message)
        {
            using (var hmac = new HMACSHA512(ApiSecret))
            {
                var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(message));
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            }
        }

        internal byte[] CreateSignature512(byte[] message)
        {
            using (var hmac = new HMACSHA512(Convert.FromBase64String(SecretKey)))
                return hmac.ComputeHash(message);
        }

        protected string CreateSignatureMD5(string message)
        {
            var HEX_DIGITS = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            var bytes = Encoding.Default.GetBytes(message);
            var sb = new StringBuilder();

            using (var md = new MD5CryptoServiceProvider())
            {
                bytes = md.ComputeHash(bytes);
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(HEX_DIGITS[(bytes[i] & 0xf0) >> 4] + "" + HEX_DIGITS[bytes[i] & 0xf]);
                }
            }
            return sb.ToString();
        }

        public void Dispose()
        {
        }
    }
}
