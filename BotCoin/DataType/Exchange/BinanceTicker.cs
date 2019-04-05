using Newtonsoft.Json;

namespace BotCoin.DataType.Exchange
{
    public class BinanceTicker
    {
        public BinanceTickerPayload Data { set; get; }
        public string Stream { set; get; }
    }

    public class BinanceTickerPayload : BinancePayload
    {        
        [JsonProperty("s")]
        public string Symbol { set; get; }
        [JsonProperty("p")]
        public double PriceChange  { set; get; }
        [JsonProperty("P")]
        public double PriceChangePercent { set; get; }
        [JsonProperty("w")]
        public double Vwap { set; get; }
        [JsonProperty("x")]
        public double ClosePricePrevDay { set; get; }
        [JsonProperty("c")]
        public double ClosePriceCurrDay { set; get; }
        [JsonProperty("Q")]
        public double QtyClose { set; get; }
        [JsonProperty("b")]
        public double BestBidPrice { set; get; }
        [JsonProperty("B")]
        public double BestBidQty { set; get; }
        [JsonProperty("a")]
        public double BestAskPrice { set; get; }
        [JsonProperty("A")]
        public double BestAskQty { set; get; }
        [JsonProperty("o")]
        public double OpenPrice { set; get; }
        [JsonProperty("h")]
        public double HighPrice { set; get; }
        [JsonProperty("l")]
        public double LowPrice { set; get; }
        [JsonProperty("v")]
        public double TotalTradedBaseVolume { set; get; }
        [JsonProperty("q")]
        public double TotalTradeQuoteVolume { set; get; }
        [JsonProperty("O")]
        public long OpenTimeStatistics { set; get; }
        [JsonProperty("C")]
        public long CloseTimeStatistics { set; get; }
        [JsonProperty("F")]
        public string FirstTradeId { set; get; }
        [JsonProperty("L")]
        public string LastTradeId { set; get; }
        [JsonProperty("n")]
        public int TradesCount { set; get; }
    }
}
