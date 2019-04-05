using BotCoin.DataType;
using System;

namespace BotCoin.ArbitrageService
{
    internal class MatchingEventArgs : EventArgs
    {
        public readonly ExchangeName Exchange;
        public readonly CurrencyName Instrument2;
        public readonly double BidPrice;
        public readonly double AskPrice;
        public readonly double TradingFee;

        public MatchingEventArgs(ExchangeName exchange, CurrencyName instrument2, double bidPrice, double askPrice, double fee)
        {
            Instrument2 = instrument2;
            Exchange    = exchange;
            BidPrice    = bidPrice;
            AskPrice    = askPrice;
            TradingFee  = fee;
        }
    }
}