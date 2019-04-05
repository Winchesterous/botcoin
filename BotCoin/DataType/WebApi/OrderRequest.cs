using BotCoin.ApiClient;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System.Net.Http;

namespace BotCoin.DataType.WebApi
{
    public class OrderRequest
    {
        [JsonIgnore]
        readonly RestApiClient2 _api;
        [JsonProperty("user")]
        public BitmexUser User { set; get; }
        [JsonProperty("order")]
        public BitmexOrderData Order { set; get; }

        public OrderRequest(RestApiClient2 api)
        {
            _api = api;
        }
                
        public ApiResult Save(BitmexOrderData order, BitmexUser user)
        {
            Order = order;
            User = user;

            return _api.UserQuery("/v1/order", HttpMethod.Post, JsonConvert.SerializeObject(this), true);
        }
    }
}
