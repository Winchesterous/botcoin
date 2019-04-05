using BotCoin.DataType.WebApi;
using BotCoin.Service;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;

namespace BotCoin.WebApi.Module
{
    public class PositionModule : NancyModule
    {
        public PositionModule() : base("/v1")
        {
            var db = (DbRepositoryService)SelfHost.Log.DbRepository;

            Post["/position"] = _ =>
            {
                var request = JsonConvert.DeserializeObject<PositionRequest>(Request.Body.AsString());
                return db.SaveBitmexPosition(request);
            };
            Post["/position/states"] = _ =>
            {
                var request = JsonConvert.DeserializeObject<PositionRequest>(Request.Body.AsString());
                db.SaveBitmexPositionState(request);
                return string.Empty;
            };
            Get["/position/state"] = _ =>
            {
                var account = Request.Query["account"].ToString();
                var hostName = Request.Query["host_name"].ToString();
                var instrument = Request.Query["instrument"].ToString();
                return db.GetBitmexPositionState(account, hostName, instrument);
            };
        }
    }
}
