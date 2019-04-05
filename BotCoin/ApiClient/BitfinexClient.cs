using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;

namespace BotCoin.ApiClient
{
    public class BitfinexClient : RestApiClient
    {
        public BitfinexClient(ExchangeSettingsData setting) : base(setting)
        {
        }

        public BitfinexClient(string url) : base(url)
        {
        }

        public OrderBook GetOrderBook(CurrencyName instrument, CurrencyName currency)
        {
            var str = String.Format("{0}book/{1}{2}?limit_bids={3}&limit_asks={3}", BaseUri, instrument.ToString().ToLower(), currency.ToString().ToLower(), OrderBookLimit);
            var json = GetQuery(str);
            var response = JsonConvert.DeserializeObject<BitfinexOrderBook>(json);

            var asks = new Order[response.Asks.Length];
            var bids = new Order[response.Bids.Length];

            for (int i = 0; i < asks.Length; i++)
                asks[i] = new Order { Price = response.Asks[i].Price, Amount = response.Asks[i].Amount };

            for (int i = 0; i < bids.Length; i++)
                bids[i] = new Order { Price = response.Bids[i].Price, Amount = response.Bids[i].Amount };

            return new OrderBook { Asks = asks, Bids = bids, Currency = currency };
        }

        public ExchangeTicker GetTicker(string pair)
        {
            var response = GetQuery(String.Format("{0}pubticker/{1}", BaseUri, pair));
            return JsonConvert.DeserializeObject<ExchangeTicker>(response);
        }
    }
}
