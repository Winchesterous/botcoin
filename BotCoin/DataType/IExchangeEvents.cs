using BotCoin.DataType;
using System;

namespace BotCoin.Exchange
{
    public interface IExchangeEvents
    {
        event EventHandler<OrderBookEventArgs> OnBtcOrderBook;
        event EventHandler<OrderBookEventArgs> OnBchOrderBook;
        event EventHandler<OrderBookEventArgs> OnEthOrderBook;
        event EventHandler<OrderBookEventArgs> OnLtcOrderBook;
        event EventHandler<OrderBookEventArgs> OnXrpOrderBook;
        event EventHandler<OrderBookEventArgs> OnDashOrderBook;
    }
}
