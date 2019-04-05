using BotCoin.ApiClient;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BotCoin.DataType.WebApi
{
    public class ChartRequest
    {
        [JsonIgnore]
        readonly RestApiClient2 _api;

        [JsonProperty("date_from")]
        public DateTime Date1 { set; get; }
        [JsonProperty("date_to")]
        public DateTime Date2 { set; get; }

        public ChartRequest(RestApiClient2 api)
        {
            _api = api;
        }

        public BitstampCandle GetBitstampCandle(DateTime dt, string instrument, int periodSecs)
        {
            var args = new Dictionary<string, string>();
            args.Add("time", dt.ToString());
            args.Add("instrument", instrument);
            args.Add("periodSecs", periodSecs.ToString());
            args.Add("exchange", ExchangeName.Bitstamp.ToString());

            var json = _api.GetQuery("/v1/charts/candle", null, RestApiClient.UrlEncode(args));
            return JsonConvert.DeserializeObject<BitstampCandle>(json);
        }

        public BitstampCandle[] GetDailyBitstampCandles(DateTime dtStart, string instrument, int periodMinutes, DateTime? dtEnd = null)
        {
            var args = new Dictionary<string, string>();
            args.Add("startDate", dtStart.ToString());            
            args.Add("periodMinutes", periodMinutes.ToString());
            args.Add("exchange", ExchangeName.Bitstamp.ToString());
            args.Add("instrument", instrument);

            if (dtEnd.HasValue)
                args.Add("endDate", dtEnd.Value.ToString());

            var json = _api.GetQuery("/v1/charts/candles", null, RestApiClient.UrlEncode(args));
            return JsonConvert.DeserializeObject<BitstampCandle[]>(json);
        }

        public BitstampTicker[] GetBitstampVwaps(DateTime dtStart, int periodMinutes, DateTime? dtEnd)
        {
            var args = new Dictionary<string, string>();
            args.Add("startDate", dtStart.ToShortDateString());
            args.Add("exchange", ExchangeName.Bitstamp.ToString());
            args.Add("periodMinutes", periodMinutes.ToString());
            if (dtEnd.HasValue)
                args.Add("endDate", dtEnd.Value.ToString());

            var json = _api.GetQuery("/v1/charts/vwaps", null, RestApiClient.UrlEncode(args));
            return JsonConvert.DeserializeObject<BitstampTicker[]>(json);
        }

        public DateTime[] GetChartDates()
        {
            var json = _api.GetQuery("/v1/charts/dates", null, "");
            var obj = JsonConvert.DeserializeObject<DateTime[]>(json);
            return obj != null ? obj : null;
        }
    }
}
