using BotCoin.DataType;
using System;

namespace BotCoin.Exchange
{
    public interface IAnalyticsEvents
    {
        event EventHandler<ExchangePricesEventArgs> InstrumentReceived;
        //event EventHandler<TradeEventArgs> TradeReceived;
    }
}
