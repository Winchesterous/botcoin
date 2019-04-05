using BotCoin.ApiClient;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using System;
using System.Linq;

namespace BotCoin.Exchange
{
    [Exchange(Name="Kuna")]
    public class KunaExchange : BaseRestExchange
    {
        readonly KunaClient _client;

        public KunaExchange(ExchangeSettingsData setting) : base(setting)
        {
            _client = new KunaClient(setting);
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Kuna;
        }

        public override OrderBook GetBtcOrderBook()
        {
            return _client.GetOrderBook("btcuah");
        }

        public override OrderBook GetBchOrderBook()
        {
            return _client.GetOrderBook("bchuah");
        }

        public override OrderBook GetLtcOrderBook()
        {
            return _client.GetOrderBook("ltcuah");
        }

        public override OrderBook GetEthOrderBook()
        {
            return _client.GetOrderBook("ethuah");
        }

        public override OrderBook GetXrpOrderBook()
        {
            return _client.GetOrderBook("xrpuah");
        }

        public override OrderResponse PlaceOrder(double price, double amount, CurrencyName currency, OrderSide side, TradeOrderType type = TradeOrderType.Limit)
        {
            string market = currency.ToString().ToLower() + "uah";
            var order = _client.PlaceOrder(market, side == OrderSide.BID ? "buy" : "sell", amount, price);

            return order.Error == null
                ? new OrderResponse(order.CreatedAt, order.Id)
                : new OrderResponse { ErrorReason = order.Error.Message };
        }

        public override bool CancelOrder(string orderId, string symbol = null)
        {
            var result = _client.CancelOrder(orderId);
            if (!result.Success)
                throw new InvalidOperationException("[Kuna]" + result.Error.Message);

            return true;
        }

        public override UserAccount GetBalances()
        {
            var userInfo = _client.GetUserInfo();
            if (userInfo.Error != null)
                throw new InvalidOperationException("[Kuna] " + userInfo.Error.Message);

            var accounts = new UserAccount { Exchange = ExchangeName.Kuna };

            foreach (var account in userInfo.Accounts)
            {
                CurrencyName currency;
                if (Enum.TryParse(account.Currency.ToUpper(), out currency))
                {
                    switch (currency)
                    {
                    case CurrencyName.UAH: accounts.Balance    = Math.Round(account.Balance, 4); break;
                    case CurrencyName.BTC: accounts.BtcBalance = account.Balance; break;
                    case CurrencyName.BCH: accounts.BchBalance = account.Balance; break;
                    case CurrencyName.ETH: accounts.EthBalance = account.Balance; break;
                    case CurrencyName.XRP: accounts.XrpBalance = account.Balance; break;
                    }
                }
            }
            return accounts;
        }

        public override OrderResponse[] GetActiveOrders(CurrencyName currency = CurrencyName.Undefined)
        {
            var orders = _client.GetActiveOrders(currency.ToString().ToLower() + "uah");
            return orders.Select(o => new OrderResponse(o)).ToArray();
        }
    }
}
