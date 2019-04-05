using BotCoin.ApiClient;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using System;
using System.Linq;

namespace BotCoin.Exchange
{
    [Exchange(Name = "Bittrex")]
    public class BittrexExchange : BaseRestExchange
    {
        readonly BittrexClient _client;

        public BittrexExchange(ExchangeSettingsData setting) : base(setting)
        {
            _client = new BittrexClient(setting);
        }
                
        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Bittrex;
        }

        public override OrderBook GetMarketSummaries()
        {
            return _client.GetMarketSummaries();
        }

        public override OrderBook GetBtcOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.BTC);
        }

        public override OrderBook GetEthOrderBook()
        {
            return _client.GetOrderBook(CurrencyName.ETH);
        }

        public override UserAccount GetBalances()
        {
            var userInfo = _client.GetBalances();
            if (!userInfo.Success)
                throw new InvalidOperationException(userInfo.Message);

            Func<string, double> available = symbol =>
            {
                var value = userInfo.Result.Where(b => String.Compare(b.Currency, symbol, true) == 0).SingleOrDefault();
                return value == null ? 0 : value.Available;
            };

            return new UserAccount
            {
                Exchange = ExchangeName.Bittrex,
                BtcBalance = available("BTC"),
                EthBalance = available("ETH")
            };
        }

        public override bool CancelOrder(string orderId, string symbol = null)
        {
            var result = _client.CancelOrder(orderId);
            throw new NotImplementedException();
        }

        public override OrderResponse PlaceOrder(double price, double amount, CurrencyName currency, OrderSide side, TradeOrderType type = TradeOrderType.Limit)
        {
            BittrexOrderResponse response = null;
            string path = side == OrderSide.BID ? "buylimit" : "selllimit";
            string market = "USDT-" + currency;

            if (type == TradeOrderType.Limit)
            {
                response = _client.PlaceLimitOrder(path, amount, price, market);
            }
            else
                throw new ArgumentException("Invalid order type");

            return response.Success
                ? new OrderResponse(response.Result.OrderId)
                : new OrderResponse() { ErrorReason = response.Message };
        }
    }
}
