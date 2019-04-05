using System;

namespace BotCoin.DataType.Exchange
{
    public class BittrexOrderBook : BittrexError
    {
        public class BittrexOrderBookItem
        {
            public BittrexOrder[] Buy { get; set; }
            public BittrexOrder[] Sell { get; set; }
        }
        public BittrexOrderBookItem Result { set; get; }
    }

    public class BittrexMarket : BittrexError
    {
        public class BittrexMarketItem
        {
            public string MarketName { set; get; }
            public double High { set; get; }
            public double Low { set; get; }
            public double Volume { set; get; }
            public double Last { set; get; }
            public double BaseVolume { set; get; }
            public DateTime TimeStamp { set; get; }
            public double Bid { set; get; }
            public double Ask { set; get; }
        }
        public BittrexMarketItem[] Result { set; get; }
    }
}
