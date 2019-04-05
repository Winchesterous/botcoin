using BotCoin.ApiClient;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace BotCoin.DataType.WebApi
{
    public class LogRequest
    {
        [JsonIgnore]
        readonly RestApiClient2 _api;

        [JsonProperty("id")]
        public string SessionId { set; get; }
        [JsonProperty("type")]
        public ServiceName ServName { set; get; }
        [JsonProperty("msg")]
        public string Message { set; get; }
        [JsonProperty("event_time")]
        public DateTime? Timestamp { set; get; }
        [JsonProperty("event_type")]
        public string EventType { set; get; }
        [JsonProperty("ex_id")]
        public int? ExchangeId { set; get; }

        public LogRequest(RestApiClient2 api, ServiceName servName = ServiceName.Undefined)
        {
            ServName = servName;
            _api = api;
        }

        public ApiResult WriteLog(string sessionId, string message, ServiceEventType eventType = ServiceEventType.Info, int? exchangeId = null)
        {
            Message    = message;
            SessionId  = sessionId;
            ExchangeId = exchangeId;

            var json = JsonConvert.SerializeObject(this);
            var url = "/v1/log";

            switch (eventType)
            {
            case ServiceEventType.Info: url += "/info"; break;
            case ServiceEventType.Warn: url += "/warn"; break;
            case ServiceEventType.Fail: url += "/fail"; break;
            default: throw new InvalidOperationException();
            }
            return _api.UserQuery(url, HttpMethod.Post, json, true);
        }

        public ApiResult WriteScalperLog(string sessionId, DateTime time, string eventType, string message)
        {
            Message = message;
            SessionId = sessionId;
            EventType = eventType;
            Timestamp = time;

            return _api.UserQuery("/v1/log/scalper", HttpMethod.Post, JsonConvert.SerializeObject(this), true);
        }
    }
}
