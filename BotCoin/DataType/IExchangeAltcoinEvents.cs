using BotCoin.DataType;
using System;

namespace BotCoin.Exchange
{
    public interface IExchangeAltcoinEvents
    {
        event EventHandler<OrderBookEventArgs> OnIotaOrderBook;
    }
}
