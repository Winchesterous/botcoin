using BotCoin.ApiClient;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System.Net.Http;

namespace BotCoin.DataType.WebApi
{
    public class MarginRequest
    {
        [JsonIgnore]
        readonly RestApiClient2 _api;
        public BitmexMarginData Margin { set; get; }

        public MarginRequest(RestApiClient2 api)
        {
            _api = api;
        }

        public ApiResult SaveChanges(BitmexMarginData margin)
        {
            Margin = margin;
            return _api.UserQuery("/v1/margin", HttpMethod.Post, JsonConvert.SerializeObject(this), true);
        }
    }
}
