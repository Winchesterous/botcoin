using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;

namespace BotCoin.ApiClient
{
    public class HitBtcClient : RestApiClient
    {
        readonly Dictionary<string, string> AuthHeader;

        public HitBtcClient(ExchangeSettingsData setting) : base(setting)
        {
            var auth = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Format("{0}:{1}", PublicKey, SecretKey)));
            AuthHeader = new Dictionary<string, string>() { { "Authorization", auth } };
        }

        public HitBtcAccount[] GetBalances()
        {
            var json = GetQuery(BaseUri + "account/balance", AuthHeader);
            return JsonConvert.DeserializeObject<HitBtcAccount[]>(json);
        }

        public HitBtcOrderResponse CancelOrder(string orderId)
        {
            var response = UserQuery(BaseUri + "order/" + orderId, HttpMethod.Delete, AuthHeader);
            var result = JsonConvert.DeserializeObject<HitBtcOrderResponse>(response.Content);

            if (response.StatusCode != (int)System.Net.HttpStatusCode.OK)
                result.Error.HttpStatus = (int)response.StatusCode;

            return result;
        }
    }
}
