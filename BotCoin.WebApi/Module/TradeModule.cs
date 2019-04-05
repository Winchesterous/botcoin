using BotCoin.DataType.WebApi;
using BotCoin.Service;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using System;

namespace BotCoin.WebApi.Module
{
    public class TradeModule : NancyModule
    {
        public TradeModule() : base("/v1/trade")
        {
            var db = (DbRepositoryService)SelfHost.Log.DbRepository;

            Post["/"] = _ =>
            {
                var request = JsonConvert.DeserializeObject<TradeRequest>(Request.Body.AsString());
                db.SaveBitmexTrade(request);
                return string.Empty;
            };
            Get["/"] = p =>
            {
                DateTime? startDate = null;
                DateTime? endDate = null;
                int? count = null;

                var instrument = Request.Query["instrument"].ToString();
                var account = Request.Query["account"].ToString();

                if (Request.Query["count"].HasValue)
                {
                    count = Int32.Parse(Request.Query["count"].ToString());
                    startDate = DateTime.UtcNow.AddHours(-24);
                    endDate = DateTime.UtcNow;
                }
                else
                {
                    startDate = DateTime.Parse(Request.Query["start_date"]);
                    endDate = DateTime.Parse(Request.Query["end_date"]);
                }
                return db.GetTrades(account, instrument, startDate.Value, endDate.Value, count);
            };
        }
    }
}
