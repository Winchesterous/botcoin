using System;

namespace BotCoin.DataType.Database
{
    public interface IDbTradeObject
    {
        DateTime Timestamp { set; get; }
        double Price { set; get; }
        double Size { set; get; }
    }

    public class DbTradeBitmex : IDbTradeObject
    {
        public DateTime Timestamp { set; get; }
        public double Price { set; get; }
        public double Size { set; get; }
    }
}
