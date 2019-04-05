using System;

namespace BotCoin.DataType.Database
{
    public class DbTradeBinance : IDbTradeObject
    {
        public DateTime TradeAt { set; get; }
        public double Price { set; get; }
        public double Quantity { set; get; }

        public DateTime Timestamp
        {
            set { TradeAt = value; }
            get { return TradeAt; }
        }

        public double Size
        {
            set { Quantity = value; }
            get { return Quantity; }
        }
    }
}
