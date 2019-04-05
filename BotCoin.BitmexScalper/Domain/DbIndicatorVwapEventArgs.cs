using BotCoin.DataType.Database;
using System;

namespace BotCoin.BitmexScalper.Domain
{
    internal class DbIndicatorVwapEventArgs : EventArgs
    {
        public readonly DbIndicatorVwapLite[] VwapGains;
        public readonly DateTime Timestamp;

        public DbIndicatorVwapEventArgs(DbIndicatorVwapLite[] vwaps)
        {
            VwapGains = vwaps;
            Timestamp = vwaps[0].Timestamp;
        }
    }
}
