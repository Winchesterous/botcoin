using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BotCoin.ApiClient
{
    public class BittrexClient : RestApiClient
    {
        public BittrexClient(ExchangeSettingsData setting) : base(setting)
        {
        }

        Dictionary<string, string> CreateRequest(string requestUri, out string urlParams, string args = null)
        {
            var nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            urlParams = String.Format("{0}?apikey={1}&nonce={2}", requestUri, PublicKey, nonce);

            if (args != null)
                urlParams += args;

            return new Dictionary<string, string> { { "apisign", CreateSignature512(BaseUri + urlParams) } };
        }

        public OrderBook GetMarketSummaries()
        {
            var json = GetQuery(BaseUri + "public/getmarketsummaries");
            var market = JsonConvert.DeserializeObject<BittrexMarket>(json);
            var orders = new OrderBook();

            if (!market.Success)
                throw new InvalidOperationException("Bittrex get market summaries");

            // ...

            return orders;
        }

        public OrderBook GetOrderBook(CurrencyName ins)
        {
            string insName = ins.ToString();

            if (ins == CurrencyName.BCH) insName = "BCC";
            if (ins == CurrencyName.DSH) insName = "DASH";

            var json = GetQuery(String.Format("{0}public/getorderbook?market=USDT-{1}&type=both", BaseUri, insName));
            var response = JsonConvert.DeserializeObject<BittrexOrderBook>(json);
            var orderBook = new OrderBook { Currency = ins };

            int askLen = response.Result.Sell.Length;
            if (askLen > OrderBookLimit) askLen = OrderBookLimit;

            orderBook.Asks = new Order[askLen];
            for (int i = 0; i < orderBook.Asks.Length; i++)
            {
                var ask = response.Result.Sell[i];
                orderBook.Asks[i] = new Order { Price = ask.Rate, Amount = ask.Quantity };
            }

            int bidLen = response.Result.Buy.Length;
            if (bidLen > OrderBookLimit) bidLen = OrderBookLimit;

            orderBook.Bids = new Order[bidLen];
            for (int i = 0; i < orderBook.Bids.Length; i++)
            {
                var bid = response.Result.Buy[i];
                orderBook.Bids[i] = new Order { Price = bid.Rate, Amount = bid.Quantity };
            }

            return orderBook;
        }

        public Order[] GetOrderBook(CurrencyName ins1, CurrencyName ins2)
        {
            bool getAsks = true;

            if (ins1 != CurrencyName.BTC)
            {
                ins2 = ins1;
                ins1 = CurrencyName.BTC;
                getAsks = false;
            }

            string insName1 = ins1.ToString();
            string insName2 = ins2.ToString();

            if (ins1 == CurrencyName.BCH) insName1 = "BCC";
            if (ins2 == CurrencyName.BCH) insName2 = "BCC";
            if (ins1 == CurrencyName.DSH) insName1 = "DASH";
            if (ins2 == CurrencyName.DSH) insName2 = "DASH";

            var json = GetQuery(String.Format("{0}public/getorderbook?market={1}-{2}&type=both", BaseUri, insName1, insName2));
            var response = JsonConvert.DeserializeObject<BittrexOrderBook>(json);
            var orderBook = new OrderBook { Currency = ins2 };

            if (getAsks)
            {
                int askLen = response.Result.Sell.Length;
                if (askLen > OrderBookLimit) askLen = OrderBookLimit;

                orderBook.Asks = new Order[askLen];
                for (int i = 0; i < orderBook.Asks.Length; i++)
                {
                    var ask = response.Result.Sell[i];
                    orderBook.Asks[i] = new Order { Price = ask.Rate, Amount = ask.Quantity };
                }
                return orderBook.Asks;
            }
            else
            {
                int bidLen = response.Result.Buy.Length;
                if (bidLen > OrderBookLimit) bidLen = OrderBookLimit;

                orderBook.Bids = new Order[bidLen];
                for (int i = 0; i < orderBook.Bids.Length; i++)
                {
                    var bid = response.Result.Buy[i];
                    orderBook.Bids[i] = new Order { Price = bid.Rate, Amount = bid.Quantity };
                }
                return orderBook.Bids;
            }
        }

        public BittrexBalances GetBalances()
        {
            string args;

            var headers = CreateRequest("account/getbalances", out args);
            var json = GetQuery(BaseUri + args, headers);

            return JsonConvert.DeserializeObject<BittrexBalances>(json);
        }

        public BittrexOrderResponse CancelOrder(string id)
        {
            string args;

            var headers = CreateRequest("market/cancel", out args, "&uuid=" + id);
            var json    = GetQuery(BaseUri + args, headers);

            return JsonConvert.DeserializeObject<BittrexOrderResponse>(json);
        }

        public BittrexOrderResponse PlaceLimitOrder(string path, double amount, double price, string market)
        {
            string args;
            var parameters = new Dictionary<string, string>
            {
                { "market", market },
                { "quantity", amount.ToString(CultureInfo.InvariantCulture) },
                { "rate", price.ToString(CultureInfo.InvariantCulture) }
            };
            var headers = CreateRequest("market/" + path, out args, "&" + UrlEncode(parameters, true));
            var json    = GetQuery(BaseUri + args, headers);

            return JsonConvert.DeserializeObject<BittrexOrderResponse>(json);
        }
    }
}
