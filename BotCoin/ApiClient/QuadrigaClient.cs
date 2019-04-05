using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace BotCoin.ApiClient
{
    public class QuadrigaClient : RestApiClient
    {
        readonly string ClientID;

        public QuadrigaClient(ExchangeSettingsData setting) : base(setting)
        {
            ClientID = setting.ClientId;
        }

        long Nonce
        {
            get { return DateTimeOffset.UtcNow.UtcTicks; }
        }

        public OrderBook GetOrderBook(CurrencyName instrument, CurrencyName currency)
        {
            var json = GetQuery(String.Format("{0}order_book?book={1}_{2}", BaseUri, instrument.ToString().ToLower(), currency.ToString().ToLower()));
            var response = JsonConvert.DeserializeObject<QuadrigaOrderBook>(json);

            if (response.Error != null)
                throw new System.Net.WebException(response.Error.Message);

            var orderBook = new OrderBook { Currency = currency, Timestamp = response.Timestamp };

            orderBook.Asks = new Order[response.Asks.Length];
            orderBook.Bids = new Order[response.Bids.Length];

            for (int i = 0; i < orderBook.Asks.Length; i++)
                orderBook.Asks[i] = new Order { Price = response.Asks[i][0], Amount = response.Asks[i][1] };

            for (int i = 0; i < orderBook.Bids.Length; i++)
                orderBook.Bids[i] = new Order { Price = response.Bids[i][0], Amount = response.Bids[i][1] };

            return orderBook;
        }

        public QuadrigaUserInfo GetBalances()
        {
            var nonce = Nonce;
            var request = new QuadrigaApiRequest
            {
                key       = PublicKey,
                nonce     = nonce,
                signature = CreateSignature256(String.Format("{0}{1}{2}", nonce, ClientID, PublicKey))
            };

            var response = UserQuery("balance", HttpMethod.Post, null, JsonConvert.SerializeObject(request), true);
            return JsonConvert.DeserializeObject<QuadrigaUserInfo>(response.Content);
        }

        public QuadrigaOrderResponse CancelOrder(string id)
        {
            var nonce = Nonce;
            var request = new QuadrigaCancelOrderRequest
            {
                id        = id,
                key       = PublicKey,
                nonce     = nonce,
                signature = CreateSignature256(String.Format("{0}{1}{2}", nonce, ClientID, PublicKey))
            };

            var response = UserQuery("cancel_order", HttpMethod.Post, null, JsonConvert.SerializeObject(request), true);

            return (String.Compare(response.Content, "\"true\"", true) == 0)
                ? new QuadrigaOrderResponse()
                : JsonConvert.DeserializeObject<QuadrigaOrderResponse>(response.Content);
        }

        public QuadrigaOrderResponse PlaceMarketOrder(string path, double amount, string book, double price)
        {
            var nonce = Nonce;
            var request = new QuadrigaLimitOrderRequest
            {
                amount    = amount,
                book      = book,
                key       = PublicKey,
                nonce     = nonce,
                signature = CreateSignature256(String.Format("{0}{1}{2}", nonce, ClientID, PublicKey))
            };

            var response = UserQuery(path, HttpMethod.Post, null, JsonConvert.SerializeObject(request), true);
            return JsonConvert.DeserializeObject<QuadrigaOrderResponse>(response.Content);
        }
                
        public QuadrigaOrderResponse PlaceLimitOrder(string path, double amount, string book, double price)
        {
            var nonce = Nonce;
            var request = new QuadrigaLimitOrderRequest
            {
                amount    = amount,
                price     = price,
                book      = book,
                key       = PublicKey,
                nonce     = nonce,
                signature = CreateSignature256(String.Format("{0}{1}{2}", nonce, ClientID, PublicKey))
            };

            var response = UserQuery(path, HttpMethod.Post, null, JsonConvert.SerializeObject(request), true);
            return JsonConvert.DeserializeObject<QuadrigaOrderResponse>(response.Content);
        }

        public QuadrigaOrderResponse[] GetActiveOrders(string pair)
        {
            var nonce = Nonce;
            var request = new QuadrigaLimitOrderRequest
            {
                book      = pair,
                key       = PublicKey,
                nonce     = nonce,
                signature = CreateSignature256(String.Format("{0}{1}{2}", nonce, ClientID, PublicKey))
            };

            var response = UserQuery("open_orders", HttpMethod.Post, null, JsonConvert.SerializeObject(request), true);
            return JsonConvert.DeserializeObject<QuadrigaOrderResponse[]>(response.Content);
        }
    }
}
