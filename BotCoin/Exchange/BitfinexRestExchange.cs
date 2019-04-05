using BotCoin.ApiClient;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;

namespace BotCoin.Exchange
{
    [Exchange(Name="Bitfinex")]
    public class BitfinexRestExchange : BaseRestExchange
    {
        BitfinexClient _client;

        public BitfinexRestExchange(ExchangeSettingsData setting) : base(setting)
        {
            _client = new BitfinexClient(setting);
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Bitfinex;
        }

        public override OrderBook GetBtcOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.BTC, CurrencyName.USD);
        }

        public override OrderBook GetBchOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.BCH, CurrencyName.USD);
        }

        public override OrderBook GetEthOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.ETH, CurrencyName.USD);
        }

        public override OrderBook GetLtcOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.LTC, CurrencyName.USD);
        }
    }
}
