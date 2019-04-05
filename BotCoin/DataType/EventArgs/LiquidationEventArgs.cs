using BotCoin.DataType.Exchange;
using System;

namespace BotCoin.DataType
{
    public class LiquidationEventArgs : EventArgs
    {
        public LiquidationEventArgs(BitmexLiquidationData[] data)
        {
            Timestamp = DateTime.UtcNow;
            Data = data;
        }

        public readonly BitmexLiquidationData[] Data;
        public DateTime Timestamp { private set; get; }
    }
}
