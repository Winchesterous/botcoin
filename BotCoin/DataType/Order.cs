using System;

namespace BotCoin.DataType
{
    public class Order
    {
        public string Id { set; get; }
        public double Price { set; get; }
        public double Size { set; get; }
        public double Amount { set; get; }
        public DateTime? Timestamp { set; get; }
        public double RemainingSize { set; get; }
    }
}
