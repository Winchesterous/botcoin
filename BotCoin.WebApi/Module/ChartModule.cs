using BotCoin.DataType;
using BotCoin.Service;
using Nancy;
using Newtonsoft.Json;
using System;

namespace BotCoin.WebApi.Module
{
    public class ChartModule : NancyModule
    {
        public ChartModule() : base("/v1/charts")
        {
            var db = (DbRepositoryService)SelfHost.Log.DbRepository;

            Func<string, string> exName = uri =>
            {
                var ex = (ExchangeName)Enum.Parse(typeof(ExchangeName), this.Request.Query["exchange"]);
                return ex != ExchangeName.Bitstamp ? String.Format("{0} {1} exchange doesn't supported", uri, ex.ToString()) : null;
            };

            Get["/candle"] = p =>
            {
                var ex = exName("/v1/chart/candle");
                if (ex != null) return ex;

                DateTime time       = Request.Query["time"];
                string   instrument = Request.Query["instrument"];
                int      period     = Request.Query["periodSecs"];

                var candle = db.GetBitstampCandle(time, instrument, period);
                return JsonConvert.SerializeObject(candle);
            };

            Get["/candles"] = p =>
            {
                var ex = exName("/v1/chart/candles");
                if (ex != null) return ex;

                DateTime  startDate  = Request.Query["startDate"];
                DateTime? endDate    = Request.Query["endDate"];
                string    instrument = Request.Query["instrument"];
                int       period     = Request.Query["periodMinutes"];

                var candles = db.GetBitstampDailyCandles(startDate, instrument, period, endDate);
                return JsonConvert.SerializeObject(candles.ToArray());
            };

            Get["/vwaps"] = p =>
            {
                var ex = exName("/v1/chart/vwaps");
                if (ex != null) return ex;

                DateTime startDate = this.Request.Query["startDate"];
                DateTime? endDate = this.Request.Query["endDate"];
                int period = this.Request.Query["periodMinutes"];

                var prices = db.GetBitstampVwap(startDate, period, endDate);
                return JsonConvert.SerializeObject(prices);
            };

            Get["/dates"] = p =>
            {
                var dates = db.GetChartDates();
                return JsonConvert.SerializeObject(dates);
            };
        }
    }
}
