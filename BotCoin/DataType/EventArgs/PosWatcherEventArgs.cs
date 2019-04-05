using System;

namespace BotCoin.DataType
{
    public class PosWatcherEventArgs : EventArgs
    {
        public readonly string Message;
        public readonly long PositionSize;

        public PosWatcherEventArgs(string msg)
        {
            Message = msg;
        }

        public PosWatcherEventArgs(long size)
        {
            PositionSize = size;
        }
    }
}
