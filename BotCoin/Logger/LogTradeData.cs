using BotCoin.DataType;
using System;

namespace BotCoin.Logger
{
    public class LogTradeData
    {
        public DateTime CreatedAt { set; get; }
        public DateTime? Timestamp { set; get; }
        public string Exchange { set; get; }
        public CurrencyName Instrument { set; get; }
        public double Price { set; get; }
        public double Amount { set; get; }
        public long OrderId { set; get; }
        public long? BuyOrderId { set; get; }
        public long? SellOrderId { set; get; }
        public string OrderType { set; get; }
    }
}
