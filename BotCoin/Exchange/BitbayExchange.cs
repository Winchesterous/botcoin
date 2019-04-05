using BotCoin.ApiClient;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using System;

namespace BotCoin.Exchange
{
    [Exchange(Name = "Bitbay")]
    public class BitbayExchange : BaseRestExchange
    {
        readonly BitbayClient _client;

        public BitbayExchange(ExchangeSettingsData setting) : base(setting)
        {
            _client = new BitbayClient(setting);
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Bitbay;
        }

        public override OrderBook GetBtcOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.PLN, CurrencyName.BTC);
        }

        public override OrderBook GetBchOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.PLN, CurrencyName.BCH);
        }

        public override OrderBook GetLtcOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.PLN, CurrencyName.LTC);
        }

        public override OrderBook GetEthOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.PLN, CurrencyName.ETH);
        }

        public override OrderBook GetDashOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.PLN, CurrencyName.DSH);
        }

        public override OrderBook GetXrpOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.PLN, CurrencyName.XRP);
        }

        public override UserAccount GetBalances()
        {
            var userInfo = _client.GetUserInfo();
            if (!userInfo.Success)
                throw new InvalidOperationException(userInfo.Message);

            var balances = userInfo.Balances;
            return new UserAccount
            {
                Exchange    = ExchangeName.Bitbay,
                BtcBalance  = balances.BTC.Available,
                EthBalance  = balances.ETH.Available,
                BchBalance  = balances.BCC.Available,
                LtcBalance  = balances.LTC.Available,
                XrpBalance  = balances.XRP.Available,
                DashBalance = balances.DASH.Available
            };
        }

        public override OrderResponse PlaceOrder(double price, double amount, CurrencyName instrument, OrderSide side, TradeOrderType type = TradeOrderType.Limit)
        {
            var result = _client.PlaceOrder(side == OrderSide.BID ? "buy" : "sell", instrument, amount, CurrencyName.PLN, price);
            return result.Success ? new OrderResponse(result.OrderId) : new OrderResponse() { ErrorReason = "Error" };
        }

        public override bool CancelOrder(string orderId, string symbol = null)
        {
            return _client.CancelOrder(orderId);
        }
    }
}
