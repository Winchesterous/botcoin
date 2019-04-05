using Newtonsoft.Json;
using System;

namespace BotCoin.DataType.Exchange
{
    public class HitBtcRequest
    {
        public class HitBtcRequestParams
        {
            [JsonProperty("symbol")]
            public string Symbol { set; get; }
        }
        public HitBtcRequestParams Params { set; get; }

        [JsonProperty("method")]
        public string Method { set; get; }

        [JsonProperty("id")]
        public string Id { set; get; }
    }

    public class HitBtcError
    {
        public int Code { set; get; }
        public string Message { set; get; }
        public string Description { set; get; }
        public int HttpStatus { set; get; }
    }

    public class HitBtcOrderResponse
    {
        public HitBtcError Error { set; get; }
    }

    public class HitBtcOrderBook : HitBtcOrderResponse
    {
        public class HitBtcOrderBookItem
        {
            public string Symbol { set; get; }
            public string Sequence { set; get; }
            public Order[] Ask { set; get; }
            public Order[] Bid { set; get; }
        }        
        public HitBtcOrderBookItem Params { set; get; }
        public bool Result { set; get; }
        public string Method { set; get; }
    }

    public class HitBtcTrade : HitBtcOrderResponse
    {
        public class HitBtcTradeItem
        {
            public long Id { set; get; }
            public double Price { set; get; }
            public double Quantity { set; get; }
            public string Side { set; get; }
            public DateTime Timestamp { set; get; }
        }
        public HitBtcTradeItem Params { set; get; }
        public bool Result { set; get; }
        public string Method { set; get; }
    }

    public class HitBtcAccount
    {
        public string Currency { set; get; }
        public double Available { set; get; }
        public double Reserved { set; get; }
    }    
}
