using BotCoin.DataType.WebApi;
using BotCoin.Service;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using System;

namespace BotCoin.WebApi.Module
{
    public class PriceLevelModule : NancyModule
    {
        public PriceLevelModule() : base("/v1")
        {
            var db = (DbRepositoryService)SelfHost.Log.DbRepository;

            Get["/pricelevels"] = _ =>
            {
                bool onlyActive = this.Request.Query["only_active"];
                var result = db.GetPriceLevels(onlyActive);
                return JsonConvert.SerializeObject(result);
            };

            Get["/pricelevel"] = _ =>
            {
                string levelId = this.Request.Query["level_id"];
                var result = db.GetPriceLevelById(levelId);
                return JsonConvert.SerializeObject(result);
            };

            Post["/pricelevel"] = _ =>
            {
                var request = JsonConvert.DeserializeObject<PriceLevelRequest>(Request.Body.AsString());
                var level = db.AddPriceLevel(request.Price, request.IsLevelUp, request.Timeframe, request.Date1, request.Date2);
                return JsonConvert.SerializeObject(level);
            };

            Put["/breakdown"] = _ =>
            {
                var request = JsonConvert.DeserializeObject<PriceLevelRequest>(Request.Body.AsString());
                db.AddBreakDown(request.LevelId, request.IsFalseBreakdown, request.Date1);
                return String.Empty;
            };

            Put["/pricelevel"] = _ =>
            {
                var request = JsonConvert.DeserializeObject<PriceLevelRequest>(Request.Body.AsString());
                db.RestorePriceLevel(request.LevelId);
                return String.Empty;
            };

            Delete["/pricelevel"] = _ =>
            {
                var request = JsonConvert.DeserializeObject<PriceLevelRequest>(Request.Body.AsString());
                db.RemovePriceLevel(request.LevelId, request.RealRemove);
                return String.Empty;
            };
        }
    }
}
