using BotCoin.DataType.Exchange;
using System;

namespace BotCoin.DataType
{
    public class BitmexEventArgs : EventArgs
    {        
        public BitmexEventArgs()
        {
            Instrument1 = CurrencyName.BTC;
            Instrument2 = CurrencyName.USD;
            CreatedAt = DateTime.UtcNow;
        }

        public BitmexEventArgs(BitmexInstrumentData data) : this()
        {
            BtxInstrument = data;
        }

        public BitmexEventArgs(BitmexTradeHistory data) : this()
        {
            BtxExecution = data;
        }

        public BitmexEventArgs(BitmexTradeData[] trades) : this()
        {
            BtxTrades = trades;
        }

        public CurrencyName Instrument1 { set; get; }
        public CurrencyName Instrument2 { set; get; }
        public DateTime CreatedAt { set; get; }
        public BitmexInstrumentData BtxInstrument { set; get; }
        public BitmexTradeData[] BtxTrades { set; get; }
        public BitmexTradeHistory BtxExecution { set; get; }
    }
}
