using System;

namespace BotCoin.DataType.Database
{
    public class DbBitstampOrderBookState
    {
        public decimal? MaxBid { set; get; }
        public decimal? MinAsk { set; get; }
    }
}
