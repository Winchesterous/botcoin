using BotCoin.ApiClient;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using System;

namespace BotCoin.Exchange
{
    [Exchange(Name = "Bitmex")]
    public class BitmexRestExchange : BaseRestExchange
    {
        public readonly BitmexClient Client;
        
        public BitmexRestExchange(ExchangeSettingsData setting) : base(setting)
        {
            Client = new BitmexClient(setting);
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Bitmex;
        }

        public override OrderBook GetBtcOrderBook()
        {
            var orders = Client.GetOrderBook("XBTUSD");
			//...
            return base.GetBtcOrderBook();
        }

        public override string CancelOrders(string[] orderIds, string symbol = null)
        {
            var result = Client.CancelOrders(orderIds);

            if (result.Length != orderIds.Length)
                return "[Cancel Error] CancelOrders hasn't response";

            if (result[0].HasError)
                return "[Cancel Error] " + result[0].Error;
            
            return null;
        }

        public override bool CancelOrder(string orderId, string symbol = null)
        {
            var result = Client.CancelOrder(orderId);

            if (result.Length != 1)
                throw new InvalidOperationException("[Bitmex] CancelOrder hasn't response.");

            if (result[0].HasError)
                throw new InvalidOperationException("[Bitmex] " + result[0].Error);

            return String.Compare(result[0].OrdStatus, "canceled", true) == 0;
        }

        public BitmexTradeData[] GetTrades(string symbol, DateTime time1, DateTime time2, int count = 500, int startPoint = 0)
        {
            return Client.GetTrades(symbol, time1, time2, count, startPoint);
        }
    }
}
