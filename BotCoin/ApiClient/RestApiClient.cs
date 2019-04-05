using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace BotCoin.ApiClient
{
    public class RestApiClient : IDisposable
    {
        private readonly xNet.HttpRequest _request;
        private readonly object _obj;
        protected readonly int OrderBookLimit;
        public    readonly string BaseUri;
        public    readonly string PublicKey;
        public    readonly string SecretKey;
        protected          byte[] ApiSecret;

        private RestApiClient()
        {
            _request = new xNet.HttpRequest();
            _obj = new object();
        }

        public RestApiClient(string url, string certificate = null) : this()
        {
            BaseUri = url;
        }

        public RestApiClient(ExchangeSettingsData setting) : this(setting.RestUrl)
        {
            OrderBookLimit = 20;
            SecretKey      = setting.SecretKey;
            PublicKey      = setting.PublicKey;
            ApiSecret      = Encoding.ASCII.GetBytes(SecretKey);
        }

        public void Dispose()
        {
            _request.Dispose();
        }

        public static string UrlEncode(IDictionary<string, string> dict, bool escape = true)
        {
            return String.Join("&", dict.Select(kvp => String.Format("{0}={1}", kvp.Key, escape ? HttpUtility.UrlEncode(kvp.Value) : kvp.Value)));
        }

        public ApiResult UserQuery(string path, HttpMethod httpMethod, Dictionary<string, string> headers = null, string content = "", bool jsonContent = false)
        {
            xNet.HttpResponse response = null;
            string contentType = null;
            path = BaseUri + path;

            if (!(httpMethod == HttpMethod.Get || httpMethod == HttpMethod.Post ||
                  httpMethod == HttpMethod.Delete || httpMethod == HttpMethod.Put))
            {
                throw new ArgumentException("Unsupported http method " + httpMethod);
            }
            if (httpMethod != HttpMethod.Get && content.Length == 0)
                throw new ArgumentException("Empty content.");

            if (headers != null && headers.Count == 0)
                throw new ArgumentException("Empty headers.");

            if (headers != null)
            {
                foreach (var h in headers)
                    _request.AddHeader(h.Key, h.Value);
            }
            if (httpMethod != HttpMethod.Get)
            {
                contentType = jsonContent ? "application/json" : "application/x-www-form-urlencoded";
            }
            lock (_obj)
            {
                try
                {
                    if (httpMethod == HttpMethod.Get)
                        response = _request.Get(path);

                    else if (httpMethod == HttpMethod.Post)
                        response = _request.Post(path, content, contentType);

                    else if (httpMethod == HttpMethod.Delete)
                        response = _request.Delete(path, content, contentType);

                    else if (httpMethod == HttpMethod.Put)
                        response = _request.Put(path, content, contentType);
                }
                catch (xNet.HttpException ex)
                {
                    return new ApiResult((int)ex.HttpStatusCode, ex.InnerMessage);
                }
            }
            content = response.ToString();
            ParseResponseHeaders(response);

            return new ApiResult((int)response.StatusCode, content);
        }
        
        protected virtual void ParseResponseHeaders(xNet.HttpResponse response)
        {
        }

        public string GetQuery(string path, Dictionary<string, string> headers = null, string content = "")
        {
            if (content.Length > 0)
                path = String.Format("{0}?{1}", path, content);

            var result = UserQuery(path, HttpMethod.Get, headers);
            return result.Content;
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
                    sb.Append(HEX_DIGITS[(bytes[i] & 0xf0) >> 4] + "" + HEX_DIGITS[bytes[i] & 0xf]);
            }
            return sb.ToString();
        }
    }
}
