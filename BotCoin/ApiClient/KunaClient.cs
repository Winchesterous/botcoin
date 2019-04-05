using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;

namespace BotCoin.ApiClient
{
    public class KunaClient : RestApiClient
    {
        public KunaClient(ExchangeSettingsData setting) : base(setting)
        {
        }

        SortedDictionary<string, string> CreateSignHeaders(string path, string method, SortedDictionary<string, string> args = null)
        {
            if (args == null)
                args = new SortedDictionary<string, string>();

            args["access_key"] = PublicKey;
            args["tonce"]      = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            args["signature"]  = CreateSignature256(String.Format("{0}|/api/v2/{1}|{2}", method, path, UrlEncode(args)));

            return args;
        }

        public OrderBook GetOrderBook(string market)
        {
            var json = GetQuery(String.Format("{0}order_book?market={1}", BaseUri, market));
            var response = JsonConvert.DeserializeObject<KunaOrderBook>(json);
            var orderBook = new OrderBook { Currency = CurrencyName.UAH };

            orderBook.Asks = new Order[response.Asks.Count];
            orderBook.Bids = new Order[response.Bids.Count];

            for (int i = 0; i < orderBook.Asks.Length; i++)
            {
                var ask = response.Asks[i];
                orderBook.Asks[i] = new Order { Id = ask.Id, Price = ask.Price, Amount = ask.Volume, Timestamp = ask.CreatedAt };
            }
            for (int i = 0; i < orderBook.Bids.Length; i++)
            {
                var bid = response.Bids[i];
                orderBook.Bids[i] = new Order { Id = bid.Id, Price = bid.Price, Amount = bid.Volume, Timestamp = bid.CreatedAt };
            }
            return orderBook;
        }

        public KunaUserInfo GetUserInfo()
        {
            var content = UrlEncode(CreateSignHeaders("members/me", "GET"));
            var response = GetQuery(BaseUri + "members/me", null, content);

            return JsonConvert.DeserializeObject<KunaUserInfo>(response);
        }

        public KunaOrdersHistory[] GetOrdersHistory(CurrencyName inst)
        {
            var args = CreateSignHeaders("trades/my", "GET", new SortedDictionary<string, string>
            {
                { "market", inst.ToString().ToLower() + "uah" }
            });

            var response = UserQuery("trades/my", HttpMethod.Get, null, UrlEncode(args));
            return JsonConvert.DeserializeObject<KunaOrdersHistory[]>(response.Content);
        }

        public KunaOrder PlaceOrder(string market, string side, double volume, double price)
        {
            var args = new SortedDictionary<string, string>()
            {
                { "side", side },
                { "volume", volume.ToString(CultureInfo.InvariantCulture) },
                { "market", market },
                { "price", price.ToString(CultureInfo.InvariantCulture) }
            };
            var content  = UrlEncode(CreateSignHeaders("orders", "POST", args));
            var response = UserQuery("orders", HttpMethod.Post, null, content);
            var obj      = JsonConvert.DeserializeObject<KunaOrder>(response.Content);

            if (response.StatusCode != (int)HttpStatusCode.Created)
                obj.Error.HttpCode = (int)response.StatusCode;

            return obj;
        }

        public KunaOrder CancelOrder(string orderId)
        {
            var args     = new SortedDictionary<string, string> { { "id", orderId } };
            var content  = UrlEncode(CreateSignHeaders("order/delete", "POST", args));
            var response = UserQuery("order/delete", HttpMethod.Post, null, content);
            var obj      = JsonConvert.DeserializeObject<KunaOrder>(response.Content);

            if (response.StatusCode != (int)HttpStatusCode.Created)
                obj.Error.HttpCode = (int)response.StatusCode;

            return obj;
        }

        public KunaOrder[] GetActiveOrders(string market)
        {
            var args     = new SortedDictionary<string, string> { { "market", market } };
            var content  = UrlEncode(CreateSignHeaders("orders", "GET", args));
            var response = GetQuery(BaseUri + "orders", null, content);

            return JsonConvert.DeserializeObject<KunaOrder[]>(response);
        }
    }
}
