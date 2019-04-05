using BotCoin.DataType.WebApi;
using BotCoin.Service;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace BotCoin.WebApi.Module
{
    public class ExchangeModule : NancyModule
    {
        public ExchangeModule() : base("/v1/exch")
        {
            var db = (DbRepositoryService)SelfHost.Log.DbRepository;

            Get["/settings/{name}"] = param =>
            {
                var exName = param.name;
                var settings = db.GetExchangeSettings();
                var setting = settings.Where(s => String.Compare(s.Exchange.ToString(), exName, true) == 0).SingleOrDefault();

                return setting == null ? "Not found" : JsonConvert.SerializeObject(setting);
            };            
            Get["/instruments/bitmex"] = param =>
            {
                var account = Request.Query["account"];
                return db.GetBitmexInstruments(account);
            };
            Post["/instruments/bitmex"] = _ =>
            {
                var request = JsonConvert.DeserializeObject<SettingRequest>(Request.Body.AsString());
                db.SaveBitmexInstruments(request.Instruments, request.AccountId);
                return String.Empty;
            };
        }
    }
}
