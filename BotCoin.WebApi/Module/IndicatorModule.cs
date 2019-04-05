using BotCoin.Service;
using Nancy;
using Newtonsoft.Json;
using System;

namespace BotCoin.WebApi.Module
{
    public class IndicatorModule : NancyModule
    {
        public IndicatorModule() : base("/v1/indicator")
        {
            var db = (DbRepositoryService)SelfHost.Log.DbRepository;

            Get["/vwaps"] = p =>
            {
                var date = DateTime.Parse(Request.Query["date"]);
                var exchange = Request.Query["exchange"].ToString();

                var vwaps = db.GetVwapGains(date, exchange);
                var settings = new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc };

                return JsonConvert.SerializeObject(vwaps, settings);
            };
        }
    }
}
