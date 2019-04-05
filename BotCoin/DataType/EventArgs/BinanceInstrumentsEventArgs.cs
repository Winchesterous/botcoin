using BotCoin.DataType.Exchange;
using System;

namespace BotCoin.DataType
{
    public class BinanceInstrumentsEventArgs : EventArgs
    {
        public readonly BinanceOrderBook OrderBook;

        public BinanceInstrumentsEventArgs(BinanceOrderBook orders)
        {
            OrderBook = orders;
        }
    }
}
