using BotCoin.DataType.WebApi;
using BotCoin.Service;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using System;

namespace BotCoin.WebApi.Module
{
    public class OrderModule : NancyModule
    {
        public OrderModule() : base("/v1")
        {
            var db = (DbRepositoryService)SelfHost.Log.DbRepository;

            Action action = () =>
            {
                var request = JsonConvert.DeserializeObject<OrderRequest>(Request.Body.AsString());
                db.SaveBitmexOrder(request);
            };

            Post["/order"] = _ =>
            {
                action();
                return String.Empty;
            };

            Delete["/order"] = _ =>
            {
                action();
                return String.Empty;
            };

            Put["/order"] = _ =>
            {
                action();
                return String.Empty;
            };
        }
    }
}
