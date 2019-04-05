using BotCoin.DataType;
using BotCoin.Exchange;
using System;

namespace BotCoin.TradeDataBotService
{
    class Gdax
    {
        public static void SubscribeInstruments(GdaxExchange ex, Action<object, ExchangePricesEventArgs> callback)
        {
            ex.Subscription(true, "full");
            ex.InstrumentReceived += (s, e) => callback(ex, e);
            ex.BeforeReconnect += (s, e) => Subscribe(ex, false);
            ex.AfterReconnect += (s, e) => Subscribe(ex, true);
        }

        public static void SubscribeTicker(GdaxExchange ex, Action<object, TickerEventArgs> callback)
        {
            ex.Subscription(true, "ticker");
            ex.TickerReceived += (s, e) => callback(ex, e);
        }

        public static void Subscribe(GdaxExchange ex, bool enable)
        {
            ex.Subscription(enable, "ticker", "full");
        }
    }
}
