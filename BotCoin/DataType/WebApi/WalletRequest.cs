using BotCoin.ApiClient;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System.Net.Http;

namespace BotCoin.DataType.WebApi
{
    public class WalletRequest
    {
        [JsonIgnore]
        readonly RestApiClient2 _api;
        public BitmexWalletData Wallet { set; get; }

        public WalletRequest(RestApiClient2 api)
        {
            _api = api;
        }
                
        public ApiResult SaveWallet(BitmexWalletData wallet)
        {
            Wallet = wallet;
            return _api.UserQuery("/v1/wallet", HttpMethod.Post, JsonConvert.SerializeObject(this), true);
        }
    }
}
