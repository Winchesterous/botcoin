using System;
using System.Collections.Generic;

namespace BotCoin.DataType
{
    public class ApiResult
    {
        public readonly int StatusCode;        
        public readonly int? RetryAfterSeconds;
        public readonly int? RateLimit;
        public readonly int? RemainingLimit;
        public readonly DateTime? ResetLimitTime;

        public string Content { set; get; }

        public ApiResult(int httpCode, string content, Dictionary<string, long> headers = null)
        {
            StatusCode = httpCode;
            Content = content;

            if (headers != null)
            {
                if (headers.ContainsKey("Retry-After"))
                    RetryAfterSeconds = Convert.ToInt32(headers["Retry-After"]);

                if (headers.ContainsKey("X-RateLimit-Limit"))
                    RateLimit = Convert.ToInt32(headers["X-RateLimit-Limit"]);

                if (headers.ContainsKey("X-RateLimit-Remaining"))
                    RemainingLimit = Convert.ToInt32(headers["X-RateLimit-Remaining"]);

                if (headers.ContainsKey("X-RateLimit-Reset"))
                {
                    var value = Convert.ToInt32(headers["X-RateLimit-Reset"]);
                    ResetLimitTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(value);
                }
            }
        }
    }
}
