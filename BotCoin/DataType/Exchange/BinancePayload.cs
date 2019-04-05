using Newtonsoft.Json;

namespace BotCoin.DataType.Exchange
{
    public class BinancePayload
    {
        [JsonProperty("e")]
        public string EventType { set; get; }
        [JsonProperty("E")]
        public long EventTime { set; get; }
    }
}
