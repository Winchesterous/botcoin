using BotCoin.DataType.WebApi;
using BotCoin.Service;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using System;

namespace BotCoin.WebApi.Module
{
    public class LogModule : NancyModule
    {
        public LogModule() : base("/v1/log")
        {
            var log = SelfHost.Log;

            Post["/info"] = _ =>
            {
                var request = JsonConvert.DeserializeObject<LogRequest>(Request.Body.AsString());
                log.WriteInfo(request.SessionId, request.Message, request.ServName, request.ExchangeId);
                return String.Empty;
            };

            Post["/warn"] = _ =>
            {
                var request = JsonConvert.DeserializeObject<LogRequest>(Request.Body.AsString());
                log.WriteWarning(request.SessionId, request.Message, request.ServName, request.ExchangeId);
                return String.Empty;
            };

            Post["/fail"] = _ =>
            {
                var request = JsonConvert.DeserializeObject<LogRequest>(Request.Body.AsString());
                log.WriteError(request.SessionId, request.Message, request.ServName, request.ExchangeId);
                return String.Empty;
            };

            Post["/scalper"] = _ =>
            {
                var request = JsonConvert.DeserializeObject<LogRequest>(Request.Body.AsString());
                ((DbRepositoryService)log.DbRepository).LogScalperEvent(request.SessionId, request.Timestamp.Value, request.EventType, request.Message);
                return String.Empty;
            };
        }
    }
}
