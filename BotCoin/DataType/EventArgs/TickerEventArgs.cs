using BotCoin.DataType.Exchange;
using System;

namespace BotCoin.DataType
{   
    public class TickerEventArgs : EventArgs
    {
        public static DateTime StartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public TickerEventArgs()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public TickerEventArgs(BinanceTicker ticker) : this()
        {
            var data = ticker.Data;
            var symbol = OrderBookEventArgs.ParseSymbol(data.Symbol);

            BinanceTicker = data;
            Instrument1   = symbol[0];
            Instrument2   = symbol[1];
            OpenTime      = StartTime.AddMilliseconds(data.OpenTimeStatistics);
            CloseTime     = StartTime.AddMilliseconds(data.CloseTimeStatistics);
            Exchange      = ExchangeName.Binance;
        }

        public TickerEventArgs(GdaxTicker data) : this()
        {
            GdaxTicker  = data;
            Instrument1 = CurrencyName.BTC;
            Instrument2 = CurrencyName.USD;
            Exchange    = ExchangeName.Gdax;
        }

        public TickerEventArgs(BitmexInstrumentData instrument) : this()
        {
            BitmexTicker = instrument;
            Exchange = ExchangeName.Bitmex;
        }

        public TickerEventArgs(BitmexTradeData[] trades) : this()
        {
            BitmexTrades = trades;
            Exchange = ExchangeName.Bitmex;
        }

        public DateTime CreatedAt { private set; get; }
        public DateTime OpenTime { set; get; }
        public DateTime CloseTime { set; get; }
        public CurrencyName Instrument1 { set; get; }
        public CurrencyName Instrument2 { set; get; }
        public BinanceTickerPayload BinanceTicker { set; get; }
        public BitstampTicker BitstampTicker { set; get; }
        public GdaxTicker GdaxTicker { set; get; }
        public ExchangeName Exchange { set; get; }
        public BitmexInstrumentData BitmexTicker { set; get; }
        public BitmexTradeData[] BitmexTrades { set; get; }
    }
}
