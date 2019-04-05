using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;

namespace BotCoin.ApiClient
{
    public class BitbayClient : RestApiClient
    {
        public BitbayClient(ExchangeSettingsData setting) : base(setting)
        {
        }

        Dictionary<string, string> CreateHeaders(Dictionary<string, string> args, out string message)
        {
            message = UrlEncode(args);
            return new Dictionary<string, string>
            {
                { "API-Key", PublicKey },
                { "API-Hash", CreateSignature512(message) }
            };
        }

        public OrderBook GetOrderBook(CurrencyName currency, CurrencyName instrument)
        {
            string instrumentName = instrument.ToString().ToLower();
            if (instrument == CurrencyName.BCH) instrumentName = "bcc";
            if (instrument == CurrencyName.DSH) instrumentName = "dash";

            var json = GetQuery(String.Format("{0}PUBLIC/{1}{2}/orderbook.json", BaseUri, instrumentName, currency.ToString().ToLower()));
            var response = JsonConvert.DeserializeObject<BitbayOrderBook>(json);
            var orderBook = new OrderBook { Currency = currency };

            var count = response.Asks.Length;
            if (count > OrderBookLimit) count = OrderBookLimit;
            orderBook.Asks = new Order[count];

            count = response.Bids.Length;
            if (count > OrderBookLimit) count = OrderBookLimit;
            orderBook.Bids = new Order[count];

            for (int i = 0; i < orderBook.Asks.Length; i++)
                orderBook.Asks[i] = new Order { Price = response.Asks[i][0], Amount = response.Asks[i][1] };

            for (int i = 0; i < orderBook.Bids.Length; i++)
                orderBook.Bids[i] = new Order { Price = response.Bids[i][0], Amount = response.Bids[i][1] };

            return orderBook;
        }

        public BitBayUserInfo GetUserInfo()
        {
            string message;
            var headers = CreateHeaders(new Dictionary<string, string>()
            {
                { "method", "info" },
                { "moment", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()}
            },
            out message);

            var response = UserQuery("Trading/tradingApi.php", HttpMethod.Post, headers, message);
            return JsonConvert.DeserializeObject<BitBayUserInfo>(response.Content);
        }

        public BitBayPlaceOrder PlaceOrder(string type, CurrencyName instrument, double amount, CurrencyName currency, double price)
        {
            string message;
            var headers = CreateHeaders(new Dictionary<string, string>()
            {
                { "type", type },
                { "currency", instrument.ToString() },
                { "amount", amount.ToString(CultureInfo.InvariantCulture) },
                { "payment_currency", currency.ToString() },
                { "rate", price.ToString(CultureInfo.InvariantCulture) },
                { "method", "trade" },
                { "moment", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()}
            },
            out message);

            var response = UserQuery("Trading/tradingApi.php", HttpMethod.Post, headers, message);
            return JsonConvert.DeserializeObject<BitBayPlaceOrder>(response.Content);
        }

        public bool CancelOrder(string id)
        {
            string message;
            var headers = CreateHeaders(new Dictionary<string, string>()
            {
                { "id", id },
                { "method", "cancel" },
                { "moment", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()}
            },
            out message);

            var response = UserQuery("Trading/tradingApi.php", HttpMethod.Post, headers, message);
            return JsonConvert.DeserializeObject<BitBayError>(response.Content).Code == 0;
        }
    }
}
