using BotCoin.ApiClient;
using BotCoin.DataType.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BotCoin.DataType.WebApi
{
    public class IndicatorRequest
    {
        [JsonIgnore]
        readonly RestApiClient2 _api;

        public IndicatorRequest(RestApiClient2 api)
        {
            _api = api;
        }

        public DbIndicatorVwapLite[] GetVwapGains(DateTime date, string exchange)
        {
            var args = new Dictionary<string, string>();
            args.Add("date", date.ToShortDateString());
            args.Add("exchange", exchange);

            var json = _api.GetQuery("/v1/indicator/vwaps", null, RestApiClient.UrlEncode(args));
            var vwaps = JsonConvert.DeserializeObject<DbIndicatorVwapLite[]>(json);

            for (int i = 0; i < vwaps.Length; i++)
            {
                var ts = vwaps[i].Timestamp;
                vwaps[i].Timestamp = new DateTime(ts.Year, ts.Month, ts.Day, ts.Hour, ts.Minute, ts.Second, DateTimeKind.Utc);
            }
            return vwaps;
        }
    }
}
