using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace BotCoin.ApiClient
{
    public class CexClient : RestApiClient
    {
        readonly string UserName;

        public CexClient(ExchangeSettingsData setting) : base(setting)
        {
            UserName = setting.ClientId;

            if (String.IsNullOrEmpty(UserName))
                throw new ArgumentException("UserId is undefined");

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
        
        public override string UserQuery(string path, string method, Dictionary<string, string> args = null, string data = null, bool isPrivate = false)
        {
            if (isPrivate)
            {
                var currentNonce = GetTimestampInMilliseconds();
                var message = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", currentNonce, UserName, PublicKey);

                args.Add("key", PublicKey);
                args.Add("signature", CreateSignature256(message));
                args.Add("nonce", Convert.ToString(currentNonce));
            }

            return base.UserQuery(path, method, args, String.Empty, isPrivate);
        }

        public UserAccount GetBalances()
        {
            string jsonString = UserQuery("balance", "POST", new Dictionary<string, string>(), null, true);
            throw new NotImplementedException();
        }

        public OrderBook GetOrderBook(CurrencyName crypto, CurrencyName currency)
        {
            var response = GetQuery(String.Format("{0}order_book/{1}/{2}", BaseUri, crypto, currency));
            return JsonConvert.DeserializeObject<OrderBook>(response);
        }
    }
}
