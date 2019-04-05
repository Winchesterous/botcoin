using BotCoin.ApiClient;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace BotCoin.DataType.WebApi
{
    public class SettingRequest
    {
        [JsonIgnore]
        readonly RestApiClient2 _api;
        [JsonProperty("commission")]
        public BitmexInstrumentSettings[] Instruments { set; get; }
        [JsonProperty("account_id")]
        public string AccountId { set; get; }

        public SettingRequest(RestApiClient2 api)
        {
            _api = api;
        }

        public static ExchangeSettingsData Get(RestApiClient2 api, string exName)
        {
            var json = api.GetQuery("/v1/exch/settings/" + exName);
            return JsonConvert.DeserializeObject<ExchangeSettingsData>(json);
        }

        public void SaveBitmexInstruments(string accountId, BitmexInstrumentSettings[] comm)
        {
            Instruments = comm;
            AccountId = accountId;

            _api.UserQuery("/v1/exch/instruments/bitmex", HttpMethod.Post, JsonConvert.SerializeObject(this), true);
        }

        public BitmexInstrumentSettings[] GetBitmexInstruments(string accountId)
        {
            var args = new Dictionary<string, string>();
            args.Add("account", accountId);

            var json = _api.GetQuery("/v1/exch/instruments/bitmex", null, RestApiClient.UrlEncode(args));
            return JsonConvert.DeserializeObject<BitmexInstrumentSettings[]>(json);
        }
    }
}
