using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BotCoin.DataType.Exchange
{
    public class OkExTradeItem
    {
        public long TradeId { set; get; }
        public double Price { set; get; }
        public double Amount { set; get; }
        public DateTime Timespan { set; get; }
        public DateTime CreatedAt { set; get; }
        public string TradeType { set; get; }
        public CurrencyName Instrument1 { set; get; }
        public CurrencyName Instrument2 { set; get; }
    }

    public class OkExTrade
    {
        public int Binary { set; get; }
        public string Channel { set; get; }
        public string[][] Data { set; get; }
        public OkExTradeItem[] Items { private set; get; }

        public static CurrencyName[] ParseInstruments(string channel, string suffix)
        {
            var len = "ok_sub_spot_".Length;
            var s1 = channel.Substring(len, channel.Length - len);
                s1 = s1.Substring(0, s1.IndexOf(suffix));

            return s1.Split('_').Select(s => (CurrencyName)Enum.Parse(typeof(CurrencyName), s, true)).ToArray();
        }

        public void ParseData()
        {
            var instruments = ParseInstruments(Channel, "_deals");
            var items = new List<OkExTradeItem>();
            var dt = DateTime.UtcNow;

            foreach (var data in Data)
            {
                items.Add(new OkExTradeItem
                {
                    CreatedAt   = dt,
                    TradeId     = Int64.Parse(data[0]),
                    Price       = Double.Parse(data[1]),
                    Amount      = Double.Parse(data[2]),
                    Timespan    = DateTime.Parse(data[3]),
                    TradeType   = data[4].ToUpper(),
                    Instrument1 = instruments[0],
                    Instrument2 = instruments[1]
                });
            }
            Items = items.OrderByDescending(i => i.TradeId).ToArray();
        }
    }

    public class OkExError
    {
        [JsonProperty("error_code")]
        public int ErrorCode { set; get; }
        [JsonProperty("error_msg")]
        public string ErrorMessage { set; get; }
        public bool Result { set; get; }
    }

    public class OkExResponseData: OkExError
    {
        public string Channel { set; get; }
        public long Timestamp { set; get; }
        public double[][] Asks { set; get; }
        public double[][] Bids { set; get; }
    }

    public class OkExBalance : OkExError
    {
        public class OkExUserInfo
        {
            public OkExBalanceFund Funds { set; get; }
        }
        public class OkExBalanceFund
        {
            public OkExBalanceFundItem Borrow { set; get; }
            public OkExBalanceFundAsset Asset { set; get; }
            public OkExBalanceFundItem Free { set; get; }
            public OkExBalanceFundItem FreeZed { set; get; }
        }
        public class OkExBalanceFundItem
        {
            public double BTC { set; get; }
            public double BCH { set; get; }
            public double ETH { set; get; }
            public double LTC { set; get; }
        }
        public class OkExBalanceFundAsset
        {
            public double Total { set; get; }
            public double Net { set; get; }
        }
        
        public OkExUserInfo Info { set; get; }
    }

    public class OkExOrderResponse : OkExError
    {
        [JsonProperty("order_id")]
        public string OrderId { set; get; }
    }
}
