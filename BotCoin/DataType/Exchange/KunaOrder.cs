using Newtonsoft.Json;
using System;

namespace BotCoin.DataType.Exchange
{
    public class KunaOrder
    {
        public string Id { get; set; }

        public string Side { get; set; }

        [JsonProperty("ord_type")]
        public string OrdType { get; set; }

        public double Price { get; set; }

        [JsonProperty("avg_price")]
        public double AvgPrice { get; set; }

        public string State { get; set; }

        public string Market { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        public double Volume { get; set; }

        [JsonProperty("remaining_volume")]
        public double RemainingVolume { get; set; }

        [JsonProperty("executed_volume")]
        public double ExecutedVolume { get; set; }

        [JsonProperty("trades_count")]
        public int TradesCount { get; set; }

        public KunaError Error { set; get; }

        public TradeOrderType OrderType
        {
            get { return (TradeOrderType)Enum.Parse(typeof(TradeOrderType), OrdType); }
        }

        public bool Success
        {
            get { return Error == null; }
        }
    }
}