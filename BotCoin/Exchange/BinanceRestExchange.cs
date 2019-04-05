using Binance.Net;
using Binance.Net.Objects;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using System;
using System.Linq;

namespace BotCoin.Exchange
{
    [Exchange(Name = "Binance")]
    public class BinanceRestExchange : BaseRestExchange
    {
        readonly BinanceClient _client;

        public BinanceRestExchange(ExchangeSettingsData setting) : base(setting)
        {
            _client = new BinanceClient(setting.PublicKey, setting.SecretKey);
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Binance;
        }

        public OrderBook GetOrderBook(string symbol, int limit = 20)
        {
            var orders = _client.GetOrderBook(symbol, limit);
            if (orders.Error != null)
                throw new InvalidOperationException(String.Format("Binance: {0} {1}", orders.Error.Code, orders.Error.Message));

            return new OrderBook
            {
                Bids = orders.Data.Bids.Select(a => new Order { Price = a.Price, Amount = a.Quantity }).ToArray(),
                Asks = orders.Data.Asks.Select(a => new Order { Price = a.Price, Amount = a.Quantity }).ToArray()
            };
        }

        public override OrderBook GetBtcOrderBook()
        {
            return GetOrderBook("BTCUSDT");
        }

        public TimeSpan GetTimeDiff()
        {
            var ts = DateTime.UtcNow;
            var time = _client.GetServerTime().Data;
            return ts - time;
        }

        public override UserAccount GetBalances()
        {
            ApiResult<BinanceAccountInfo> account = null;
            Func<string, double> getValue = symbol => account.Data.Balances.Where(b => String.Compare(b.Asset, symbol, true) == 0).Single().Free;

            account = _client.GetAccountInfo();
            return new UserAccount
            {
                Exchange    = ExchangeName.Binance,
                Balance     = getValue("USDT"),
                BtcBalance  = getValue("BTC"),
                BchBalance  = getValue("BCC"),
                LtcBalance  = getValue("LTC"),
                EthBalance  = getValue("ETH")
            };
        }

        public override OrderResponse PlaceOrder(double price, double amount, CurrencyName currency, DataType.OrderSide orderSide, TradeOrderType type = TradeOrderType.Limit)
        {
            if (!(currency == CurrencyName.BTC || currency == CurrencyName.BCH ||
                currency == CurrencyName.ETH || currency == CurrencyName.LTC))
            {
                return new OrderResponse { ErrorReason = "Invalid instrument" };
            }
            var currencyName = currency.ToString();
            if (currency == CurrencyName.BCH)
                currencyName = "BCC";

            var symbol = currencyName + "USDT";
            var side = orderSide == DataType.OrderSide.BID ? Binance.Net.Objects.OrderSide.Buy : Binance.Net.Objects.OrderSide.Sell;

            //"LIMIT", "LIMIT_MAKER", "MARKET", "STOP_LOSS_LIMIT", "TAKE_PROFIT_LIMIT"

            var result = _client.PlaceOrder(symbol, side, OrderType.Limit, TimeInForce.GoodTillCancel, amount, price);

            return result.Success
                ? new OrderResponse(result.Data.TransactTime, result.Data.OrderId.ToString())
                : new OrderResponse { ErrorReason = result.Error.Message, ErrorCode = result.Error.Code };
        }

        public override bool CancelOrder(string orderId, string symbol = null)
        {
            var result = _client.CancelOrder(symbol);   // cancel active order
            return base.CancelOrder(orderId);
        }
    }
}
