using System;

namespace BotCoin.DataType.Exchange
{
    public class BitfinexOrderBook
    {
        public class BitfinexOrder
        {
            public double Price { set; get; }
            public double Amount { set; get; }
            public double Timestamp { set; get; }
        }
        public BitfinexOrder[] Bids { set; get; }
        public BitfinexOrder[] Asks { set; get; }
    }
}
