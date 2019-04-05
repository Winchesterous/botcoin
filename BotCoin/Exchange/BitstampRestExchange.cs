using BotCoin.ApiClient;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using System;
using System.Linq;
using System.Text;

namespace BotCoin.Exchange
{
    [Exchange(Name = "Bitstamp")]
    public class BitstampRestExchange : BaseRestExchange
    {
        internal readonly BitstampClient Client;

        public BitstampRestExchange(ExchangeSettingsData setting) : base(setting)
        {
            Client = new BitstampClient(setting);
        }

        public override void InitExchange()
        {
            base.InitExchange();
            base.InitBitstampConfiguration(Client);
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Bitstamp;
        }

        public BitstampTicker GetTicker()
        {
            return Client.GetTicker();
        }

        public override OrderBook GetBtcOrderBook()
        {
            return Client.GetOrderBook("btcusd");
        }

        public override OrderBook GetBchOrderBook()
        {
            return Client.GetOrderBook("bchusd");
        }

        public override OrderBook GetLtcOrderBook()
        {
            return Client.GetOrderBook("ltcusd");
        }

        public override OrderBook GetEthOrderBook()
        {
            return Client.GetOrderBook("ethusd");
        }       

        public override UserAccount GetBalances()
        {
            var account = Client.GetAccountBalance();
            if (account.HasError)
                throw new InvalidOperationException("[Bitstamp] GetBalances " + account.Reason);
            
            return new UserAccount
            {
                Exchange = ExchangeName.Bitstamp,
                Balance = account.UsdAvailable,
                BtcBalance = account.BtcAvailable,
                BchBalance = account.BchAvailable,
                EthBalance = account.EthAvailable,
                LtcBalance = account.LtcAvailable
            };
        }

        public override OrderResponse[] GetActiveOrders(CurrencyName currency = CurrencyName.Undefined)
        {
            var currencyPair = currency.ToString().ToLower() + "usd";
            return Client.GetActiveOrders(currencyPair).Select(o => new OrderResponse(o)).ToArray();
        }

        public override bool CancelOrder(string orderId, string symbol = null)
        {
            var result = Client.CancelOrder(symbol, orderId);
            if (!String.IsNullOrEmpty(result.Error))
                throw new InvalidOperationException("[Bitstamp] " + result.Error);

            return true;
        }

        public override OrderResponse PlaceOrder(double price, double amount, CurrencyName inst, OrderSide side, TradeOrderType type = TradeOrderType.Limit)
        {
            BitstampOrderResponse response = null;
            var symbol = inst.ToString().ToLower() + "usd";

            if (type == TradeOrderType.Limit)
            {
                response = Client.PlaceLimitOrder(side == OrderSide.BID ? "buy" : "sell", symbol, amount, price);
            }
            else
                throw new ArgumentException("[Bitstamp] Invalid order type");

            if (String.Compare(response.Status, "error", true) != 0)
                return new OrderResponse(response.Datetime, response.OrderId);

            var str = new StringBuilder("[Bitstamp] PlaceOrder");
            foreach (var error in response.Reason.All)
            {
                str.AppendFormat(" {0}", error);
            }
            return new OrderResponse() { ErrorReason = str.ToString() };
        }        
    }
}
