using BotCoin.ApiClient;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;

namespace BotCoin.Exchange
{
    [Exchange(Name="Cex")]
    public class CexRestExchange : BaseRestExchange
    {
        public readonly CexClient Client;

        public CexRestExchange(ExchangeSettingsData setting) : base(setting)
        {
            Client = new CexClient(setting);
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Cex;
        }

        public override OrderBook GetBtcOrderBook()
        {
            return Client.GetOrderBook(CurrencyName.BTC, CurrencyName.USD);
        }

        public override OrderBook GetBchOrderBook()
        {
            return Client.GetOrderBook(CurrencyName.BCH, CurrencyName.USD);
        }

        public override OrderBook GetEthOrderBook()
        {
            return Client.GetOrderBook(CurrencyName.ETH, CurrencyName.USD);
        }

        public override UserAccount GetBalances()
        {
            return Client.GetBalances();
        }
    }
}
