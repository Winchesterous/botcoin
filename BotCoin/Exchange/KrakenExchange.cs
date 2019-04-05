using BotCoin.ApiClient;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;

namespace BotCoin.Exchange
{
    [Exchange(Name="Kraken")]
    public class KrakenExchange : BaseRestExchange
    {
        readonly KrakenClient _client;

        public KrakenExchange(ExchangeSettingsData setting) : base(setting)
        {
            _client = new KrakenClient(setting);
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Kraken;
        }

        public override OrderBook GetBtcOrderBook()
        {
            return _client.GetOrderBook("XXBTZUSD");
        }

        public override OrderBook GetEthOrderBook()
        {
            return _client.GetOrderBook("XETHZUSD");
        }

        public override OrderBook GetLtcOrderBook()
        {
            return _client.GetOrderBook("XLTCZUSD");
        }

        public override OrderBook GetBchOrderBook()
        {
            return _client.GetOrderBook("BCHUSD");
        }

        public override OrderBook GetXrpOrderBook()
        {
            return _client.GetOrderBook("XRPUSD");
        }

        public override OrderBook GetDashOrderBook()
        {
            return _client.GetOrderBook("DASHUSD");
        }

        public override UserAccount GetBalances()
        {
            var userInfo = _client.GetBalances();
            //if (userInfo.Error.Length > 0)
            //    throw new InvalidOperationException(userInfo.Message);

            return new UserAccount
            {
                Exchange = ExchangeName.Kraken
            };
        }

        public override bool CancelOrder(string orderId, string symbol = null)
        {
            var result = _client.CancelOrder(orderId);
            return result.Error.Length == 0;
        }
    }
}
