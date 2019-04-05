using BotCoin.DataType;
using System;

namespace BotCoin.Instruments
{
    public class LtcInstrument : InstrumentBaseType
    {
        public LtcInstrument(string instrument) : base(instrument)
        {
        }

        public override string ToStringPrice(double price)
        {
            return price.ToString("0.00000");
        }

        public override double SetBidPrice(ExchangePricesEventArgs e)
        {
            _bidPrice = e.LtcPrice[0];
            return _bidPrice;
        }

        public override double SetAskPrice(ExchangePricesEventArgs e)
        {
            _askPrice = e.LtcPrice[1];
            return _askPrice;
        }
                
        public override double ConvertToPrice(long qty, double price)
        {
            return qty * price;
        }

        public override double RoundPrice(double price)
        {
            return Math.Round(price, 5);
        }
    }
}
