using System;

namespace BotCoin.DataType.Database
{
    public class DbInstrumentSpread
    {
        public DateTime CreatedAt { set; get; }        
        public CurrencyName Instrument1 { set; get; }
        public CurrencyName Instrument2 { set; get; }
        public ExchangeName Exchange1 { set; get; }
        public ExchangeName Exchange2 { set; get; }
        public double Spread1 { set; get; }
        public double Spread2 { set; get; }
        public double Bid1 { set; get; }
        public double Ask1 { set; get; }
        public double Bid2 { set; get; }
        public double Ask2 { set; get; }
    }
}
