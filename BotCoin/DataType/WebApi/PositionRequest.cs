using BotCoin.ApiClient;
using BotCoin.DataType.Database;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace BotCoin.DataType.WebApi
{
    public class PositionRequest
    {
        [JsonIgnore]
        readonly RestApiClient2 _api;
        [JsonProperty("trade")]
        public BitmexTradeHistory Trade { set; get; }
        [JsonProperty("user")]
        public BitmexUser User { set; get; }
        [JsonProperty("pos_state")]
        public DbPositionState[] PositionStates { set; get; }
        [JsonProperty("instrument")]
        public string Instrument { set; get; }

        public PositionRequest(RestApiClient2 api)
        {
            _api = api;
        }

        public DbMessage SavePosition(BitmexTradeHistory trade, BitmexUser account)
        {
            Trade = trade;
            User = account;

            var result = _api.UserQuery("/v1/position", HttpMethod.Post, JsonConvert.SerializeObject(this), true);
            return JsonConvert.DeserializeObject<DbMessage>(result.Content);
        }

        public void SavePositionState(BitmexUser user, List<DbPositionState> states)
        {            
            PositionStates = states.ToArray();
            User           = user;

            var host = Dns.GetHostName();
            foreach (var state in PositionStates) state.HostName = host;

            _api.UserQuery("/v1/position/states", HttpMethod.Post, JsonConvert.SerializeObject(this), true);
        }

        public DbPositionState GetPositionState(BitmexUser user, string instrument)
        {
            var args = new Dictionary<string, string>();
            args.Add("account", user.Id);
            args.Add("host_name", Dns.GetHostName());
            args.Add("instrument", instrument);

            var json = _api.GetQuery("/v1/position/state", null, RestApiClient.UrlEncode(args));
            return JsonConvert.DeserializeObject<DbPositionState>(json);
        }
    }
}
