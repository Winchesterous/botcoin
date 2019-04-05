using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace BotCoin.ApiClient
{
    public class KrakenClient : RestApiClient
    {
        public KrakenClient(ExchangeSettingsData setting) : base(setting)
        {
        }

        Dictionary<string, string> CreateRequest(string path, out string postData, string args = null)
        {
            var nonce = DateTime.Now.Ticks;
            postData = "nonce=" + nonce;

            if (args != null)
                postData += args;

            var pathBytes = Encoding.UTF8.GetBytes(path);
            var hash256Bytes = CreateHash256(nonce + postData);
            var array = new byte[pathBytes.Length + hash256Bytes.Length];

            pathBytes.CopyTo(array, 0);
            hash256Bytes.CopyTo(array, pathBytes.Length);

            return new Dictionary<string, string>
            {
                { "API-Key", PublicKey },
                { "API-Sign", Convert.ToBase64String(CreateSignature512(array)) }
            };
        }

        public OrderBook GetOrderBook(string pairName)
        {
            var json = GetQuery(String.Format("{0}/0/public/Depth?pair={1}&count={2}", BaseUri, pairName, OrderBookLimit));
            var response = JsonConvert.DeserializeObject<KrakenOrderBook>(json);

            if (response.Error.Length > 0)
                throw new InvalidOperationException(response.GetError());

            if (pairName == "XXBTZUSD")
                return CreateOrderBook(response.Result.XXBTZUSD);

            if (pairName == "XETHZUSD")
                return CreateOrderBook(response.Result.XETHZUSD);

            if (pairName == "XLTCZUSD")
                return CreateOrderBook(response.Result.XLTCZUSD);

            if (pairName == "BCHUSD")
                return CreateOrderBook(response.Result.BCHUSD);

            if (pairName == "XRPUSD")
                return CreateOrderBook(response.Result.XXRPZUSD);

            if (pairName == "DASHUSD")
                return CreateOrderBook(response.Result.DASHUSD);

            throw new InvalidOperationException("[Kraken] Undefined currency pair.");
        }

        private OrderBook CreateOrderBook(KrakenOrderBook.OrderBook orderBook)
        {
            var asks = new Order[orderBook.Asks.Length];
            var bids = new Order[orderBook.Bids.Length];

            for (int i = 0; i < asks.Length; i++)
            {
                var jar = (JArray)orderBook.Asks[i];
                asks[i] = new Order { Price = jar[0].Value<double>(), Amount = jar[1].Value<double>() };
            }
            for (int i = 0; i < bids.Length; i++)
            {
                var jar = (JArray)orderBook.Bids[i];
                bids[i] = new Order { Price = jar[0].Value<double>(), Amount = jar[1].Value<double>() };
            }
            return new OrderBook { Asks = asks, Bids = bids };
        }

        public KrakenBalance GetBalances()
        {
            string path  = "/0/private/Balance", postData;
            var headers  = CreateRequest(path, out postData);
            var response = UserQuery(path, HttpMethod.Post, headers, postData);

            return JsonConvert.DeserializeObject<KrakenBalance>(response.Content);
        }        

        public KrakenOrderResponse CancelOrder(string id)
        {
            string path = "/0/private/CancelOrder", postData;
            var headers = CreateRequest(path, out postData, "&txid=" + id);
            var response = UserQuery(path, HttpMethod.Post, headers, postData);

            return JsonConvert.DeserializeObject<KrakenOrderResponse>(response.Content);
        }
    }
}
