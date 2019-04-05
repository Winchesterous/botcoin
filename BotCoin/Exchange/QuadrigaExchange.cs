using BotCoin.ApiClient;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using System;
using System.Linq;

namespace BotCoin.Exchange
{
    [Exchange(Name="Quadriga")]
    public class QuadrigaExchange : BaseRestExchange
    {
        QuadrigaClient _client;

        public QuadrigaExchange(ExchangeSettingsData setting) : base(setting)
        {
            _client = new QuadrigaClient(setting);
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Quadriga;
        }

        public override OrderBook GetBtcOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.BTC, CurrencyName.CAD);
        }

        public override OrderBook GetBchOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.BCH, CurrencyName.CAD);
        }

        public override OrderBook GetEthOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.ETH, CurrencyName.CAD);
        }

        public override OrderBook GetLtcOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.LTC, CurrencyName.CAD);
        }

        public override UserAccount GetBalances()
        {
            var balance = _client.GetBalances();
            return new UserAccount
            {
                Exchange   = ExchangeName.Quadriga,
                Balance    = balance.CAD,
                BtcBalance = balance.BTC,
                EthBalance = balance.ETH,
                BchBalance = balance.BCH,
                LtcBalance = balance.LTC
            };
        }

        public override OrderResponse PlaceOrder(double price, double amount, CurrencyName currency, OrderSide side, TradeOrderType type = TradeOrderType.Limit)
        {
            QuadrigaOrderResponse response = null;
            string path = side == OrderSide.BID ? "buy" : "sell";
            string book = currency.ToString().ToLower() + "_cad"; 

            if (type == TradeOrderType.Limit)
            {
                response = _client.PlaceLimitOrder(path, amount, book, price);
            }
            else if (type == TradeOrderType.Market)
            {
                response = _client.PlaceMarketOrder(path, amount, book, price);
            }
            else
                throw new ArgumentException("Invalid order type");

            return response.Error == null 
                ? new OrderResponse(response.Datetime, response.OrderId) 
                : new OrderResponse() { ErrorReason = response.Error.Message };
        }

        public override bool CancelOrder(string orderId, string symbol = null)
        {
            var response = _client.CancelOrder(orderId);
            if (response.Error != null)
                throw new InvalidOperationException("[Quadriga] CancelOrder " + response.Error.Message);

            return true;
        }

        public override OrderResponse[] GetActiveOrders(CurrencyName currency = CurrencyName.Undefined)
        {
            var orders = _client.GetActiveOrders(currency.ToString().ToLower() + "_cad");
            return orders.Select(o => new OrderResponse(o)).ToArray();
        }
    }
}
