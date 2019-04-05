using BotCoin.DataType.WebApi;
using BotCoin.Service;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using System;

namespace BotCoin.WebApi.Module
{
    public class WalletModule : NancyModule
    {
        public WalletModule() : base("/v1/wallet")
        {
            var db = (DbRepositoryService)SelfHost.Log.DbRepository;

            Post["/"] = _ =>
            {
                var request = JsonConvert.DeserializeObject<WalletRequest>(Request.Body.AsString());
                db.SaveWallet(request);
                return String.Empty;
            };
        }
    }
}
