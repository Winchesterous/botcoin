using Newtonsoft.Json;

namespace BotCoin.DataType.Exchange
{
    public class BinanceOrderBook
    {
        public BinanceOrderBookPayload Data { set; get; }
        public string Stream { set; get; }
    }

    public class BinanceOrderBookPayload : BinancePayload
    {
        public long LastUpdateId { set; get; }
        public object[][] Bids { set; get; }
        public object[][] Asks { set; get; }
        [JsonProperty("t")]
        public string TradeId { set; get; }
        [JsonProperty("T")]
        public long TradeTime { set; get; }
        [JsonProperty("s")]
        public string Symbol { set; get; }
        [JsonProperty("p")]
        public double Price { set; get; }
        [JsonProperty("q")]
        public double Amount { set; get; }
        [JsonProperty("b")]
        public string BuyerOrderId { set; get; }
        [JsonProperty("a")]
        public string SellerOrderId { set; get; }
        [JsonProperty("m")]
        public bool IsBuyerMarketMaker { set; get; }
    }
}
