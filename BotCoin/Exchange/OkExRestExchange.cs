using BotCoin.ApiClient;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using System;

namespace BotCoin.Exchange
{
    [Exchange(Name = "OkEx")]
    public class OkExRestExchange : BaseRestExchange
    {
        readonly OkExClient _client;

        public OkExRestExchange(ExchangeSettingsData setting) : base(setting)
        {
            _client = new OkExClient(setting);
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.OkEx;
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

        public override UserAccount GetBalances()
        {
            var balance = _client.GetUserInfo();
            var assets = balance.Info.Funds.Free;

            return new UserAccount
            {
                Exchange = ExchangeName.OkEx,
                Balance = 0,    //???
                BtcBalance = assets.BTC,
                BchBalance = assets.BCH,
                EthBalance = assets.ETH,
                LtcBalance = assets.LTC
            };
        }

        public override OrderResponse PlaceOrder(double price, double amount, CurrencyName inst, OrderSide side, TradeOrderType type = TradeOrderType.Limit)
        {
            OkExOrderResponse response = null;
            var symbol = inst.ToString().ToLower() + "_usd";

            if (type == TradeOrderType.Limit)
            {
                response = _client.PlaceLimitOrder(side == OrderSide.BID ? "buy" : "sell", symbol, amount, price);
            }
            else
                throw new ArgumentException("Invalid order type");

            return response.Result
                ? new OrderResponse(DateTime.UtcNow, response.OrderId)
                : new OrderResponse() { ErrorReason = String.Format("[OkEx] {0}", response.ErrorCode) };
        }

        public override bool CancelOrder(string orderId, string symbol = null)
        {
            var result = _client.CancelOrder(symbol, orderId);
            throw new NotImplementedException();
        }
    }
}
