using System.Collections.Generic;

namespace BotCoin.DataType.Database
{
    public class DbMessage
    {
        public List<DbIndicatorVwapLite> VwapGains { set; get; }
        public DbPosition[] Positions { set; get; }
        public DbTrade[] Trades { set; get; }
        public double? OpenPosFee { set; get; }
    }
}
