using System;

namespace BotCoin.DataType.Database
{
    public class DbSpread
    {
        public DbSpread()
        {
            Instrument2 = CurrencyName.BTC;
        }

        public DateTime CreatedAt { set; get; }
        public double SpreadPercent { set; get; }
        public CurrencyName Instrument1 { set; get; }
        public CurrencyName Instrument2 { set; get; }
        public ExchangeName Exchange1 { set; get; }
        public ExchangeName Exchange2 { set; get; }        
        public double BidAltcUsd { set; get; }
        public double AskAltcUsd { set; get; }
        public double BidAltcInstr2 { set; get; }
        public double AskAltcInstr2 { set; get; }
        public double BidUsd1 { set; get; }
        public double AskUsd1 { set; get; }
        public double BidUsd2 { set; get; }
        public double AskUsd2 { set; get; }
    }
}
