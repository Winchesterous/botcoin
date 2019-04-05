using System;

namespace BotCoin.DataType.Exchange
{
    public class QuadrigaOrder
    {
        public string Id { get; set; }
        public string Side { get; set; }
        public string OrderType { get; set; }
        public double Price { get; set; }
        public double AvgPrice { get; set; }
        public string State { get; set; }
        public string Market { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Volume { get; set; }
        public double RemainingVolume { get; set; }
        public double ExecutedVolume { get; set; }
        public int TradesCount { get; set; }
    }
}