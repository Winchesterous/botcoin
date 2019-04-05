using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;

namespace BotCoin.ApiClient
{
    public class BitstampClient : RestApiClient
    {
        readonly string ClientId;

        public BitstampClient(ExchangeSettingsData setting) : base(setting)
        {
            ClientId = setting.ClientId;            
        }

        Dictionary<string, string> CreateParameters(Dictionary<string, string> args = null)
        {
            var nonce = DateTimeOffset.UtcNow.UtcTicks.ToString();

            if (args == null)
                args = new Dictionary<string, string>();

            args["key"]       = PublicKey;
            args["nonce"]     = nonce;
            args["signature"] = CreateSignature256(String.Format("{0}{1}{2}", nonce, ClientId, PublicKey)).ToUpper();

            return args;
        }

        public ExchangeConfiguration[] GetExchangeConfiguration()
        {
            var response = GetQuery("trading-pairs-info/");
            return JsonConvert.DeserializeObject<ExchangeConfiguration[]>(response);
        }

        public BitstampTicker GetTicker()
        {
            var response = GetQuery("ticker_hour/btcusd/");
            return JsonConvert.DeserializeObject<BitstampTicker>(response);
        }

        public OrderBook GetOrderBook(string currency)
        {
            var response = GetQuery("order_book/" + currency);
            return JsonConvert.DeserializeObject<OrderBook>(response);
        }

        public BitstampAccount GetAccountBalance()
        {
            var args = CreateParameters();
            var response = UserQuery("balance/", HttpMethod.Post, null, UrlEncode(args));

            return JsonConvert.DeserializeObject<BitstampAccount>(response.Content);
        }

        public BitstampOrderData[] GetActiveOrders(string pair)
        {
            var args = CreateParameters();
            var response = UserQuery(String.Format("open_orders/{0}/", pair), HttpMethod.Post, null, UrlEncode(args));

            return JsonConvert.DeserializeObject<BitstampOrderData[]>(response.Content);
        }

        public BitstampOrderResponse CancelOrder(string symbol, string orderId)
        {
            var args = CreateParameters(new Dictionary<string, string> { { "id", orderId } });
            var response = UserQuery("cancel_order/", HttpMethod.Post, null, UrlEncode(args));

            return JsonConvert.DeserializeObject<BitstampOrderResponse>(response.Content);
        }

        public BitstampOrderResponse PlaceLimitOrder(string type, string symbol, double amount, double price)
        {
            var args = CreateParameters(new Dictionary<string, string>
            {
                { "amount", amount.ToString(CultureInfo.InvariantCulture) },
                { "price", price.ToString(CultureInfo.InvariantCulture) }
            });
            var response = UserQuery(String.Format("{0}/{1}/", type, symbol), HttpMethod.Post, null, UrlEncode(args));
            return JsonConvert.DeserializeObject<BitstampOrderResponse>(response.Content);
        }
    }
}
