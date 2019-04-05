using BotCoin.ApiClient;
using BotCoin.DataType.Database;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace BotCoin.DataType.WebApi
{
    public class TradeRequest
    {
        [JsonIgnore]
        readonly RestApiClient2 _api;
        [JsonProperty("user")]
        public BitmexUser User { set; get; }
        [JsonProperty("pos")]
        public DbPosition Position { set; get; }
        [JsonProperty("stop")]
        public double StopValue { set; get; }
        [JsonProperty("risk")]
        public double RiskPercent { set; get; }
        [JsonProperty("stop_price")]
        public double StopPrice { set; get; }
        [JsonProperty("start_watch_price")]
        public double StartWatchPrice { set; get; }

        public TradeRequest(RestApiClient2 api)
        {
            _api = api;
        }

        public void CreateBitmexTrade(DbPosition position, double stopValue, double riskPercent, double stopPrice, double startWatchPrice)
        {
            Position        = position;
            StopValue       = stopValue;
            StopPrice       = stopPrice;
            RiskPercent     = riskPercent;
            StartWatchPrice = startWatchPrice;

            _api.UserQuery("/v1/trade", HttpMethod.Post, JsonConvert.SerializeObject(this), true);
        }

        public DbMessage GetDbTrades(BitmexUser user, string instrument, int count)
        {
            var args = new Dictionary<string, string>();
            args.Add("account", user.Id);
            args.Add("count", count.ToString());
            args.Add("instrument", instrument);

            var json = _api.GetQuery("/v1/trade", null, RestApiClient.UrlEncode(args));
            return JsonConvert.DeserializeObject<DbMessage>(json);
        }

        public DbMessage GetDbTrades(BitmexUser user, DateTime startTime, DateTime endTime, string instrument = "XBTUSD")
        {
            var args = new Dictionary<string, string>();
            args.Add("account", user.Id);
            args.Add("start_date", startTime.ToShortDateString());
            args.Add("end_date", endTime.ToShortDateString());
            args.Add("instrument", instrument);

            var json = _api.GetQuery("/v1/trade", null, RestApiClient.UrlEncode(args));
            return JsonConvert.DeserializeObject<DbMessage>(json);
        }        
    }
}
