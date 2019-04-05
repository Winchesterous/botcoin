using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;

namespace BotCoin.ApiClient
{
    public class OkExClient : RestApiClient
    {
        public OkExClient(ExchangeSettingsData setting) : base(setting)
        {
        }

        private void SortAsks(OrderBook orderBook)
        {
            var comparer = Comparer<double>.Default;
            Array.Sort<Order>(orderBook.Asks, (x, y) => comparer.Compare(x.Price, y.Price));
        }

        private string CreatePostData(SortedDictionary<string, string> args = null)
        {
            if (args == null)
                args = new SortedDictionary<string, string>();

            args.Add("api_key", PublicKey);
            var postData = String.Format("{0}&secret_key={1}", UrlEncode(args), SecretKey);
            
            args.Add("sign", CreateSignatureMD5(postData));
            return UrlEncode(args);
        }

        public OrderBook GetOrderBook(CurrencyName instrument, CurrencyName currency)
        {
            var json = GetQuery(String.Format("{0}depth.do?symbol={1}_{2}", BaseUri, instrument.ToString().ToLower(), currency.ToString().ToLower()));
            var response = JsonConvert.DeserializeObject<OrderBook>(json);
            SortAsks(response);

            return response;
        }

        public OkExBalance GetUserInfo()
        {
            var response = UserQuery("userinfo.do", HttpMethod.Post, null, CreatePostData());
            return JsonConvert.DeserializeObject<OkExBalance>(response.Content);
        }

        public OkExOrderResponse CancelOrder(string symbol, string orderId)
        {
            var postData = CreatePostData(new SortedDictionary<string, string>()
            {
                { "symbol", symbol },
                { "order_id", orderId },
            });
            var response = UserQuery("cancel_order.do", HttpMethod.Post, null, postData);
            return JsonConvert.DeserializeObject<OkExOrderResponse>(response.Content);
        }

        public OkExOrderResponse PlaceLimitOrder(string type, string symbol, double amount, double price)
        {
            var postData = CreatePostData(new SortedDictionary<string, string>()
            {
                { "symbol", symbol },
                { "type", type },
                { "price", price.ToString(CultureInfo.InvariantCulture) },
                { "amount", amount.ToString(CultureInfo.InvariantCulture) }
            });
            var response = UserQuery("trade.do", HttpMethod.Post, null, postData);
            return JsonConvert.DeserializeObject<OkExOrderResponse>(response.Content);
        }
    }
}