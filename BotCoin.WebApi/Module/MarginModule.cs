using BotCoin.DataType.WebApi;
using BotCoin.Service;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using System;

namespace BotCoin.WebApi.Module
{
    public class MarginModule : NancyModule
    {
        public MarginModule() : base("/v1/margin")
        {
            var db = (DbRepositoryService)SelfHost.Log.DbRepository;

            Post["/"] = _ =>
            {
                var request = JsonConvert.DeserializeObject<MarginRequest>(Request.Body.AsString());
                db.SaveMargin(request);
                return String.Empty;
            };
        }
    }
}
