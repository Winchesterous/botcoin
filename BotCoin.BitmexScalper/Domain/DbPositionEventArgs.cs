using BotCoin.DataType.Database;
using System;

namespace BotCoin.BitmexScalper.Domain
{
    internal class DbPositionEventArgs : EventArgs
    {
        public readonly DbPosition[] Positions;
        public readonly DbTrade[] Trades;

        public DbPositionEventArgs(DbPosition[] positions)
        {
            Positions = positions;
        }

        public DbPositionEventArgs(DbTrade[] trades)
        {
            Trades = trades;
        }
    }
}
