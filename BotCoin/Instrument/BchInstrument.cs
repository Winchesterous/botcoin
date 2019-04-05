using BotCoin.DataType;
using System;

namespace BotCoin.Instruments
{
    public class BchInstrument : InstrumentBaseType
    {
        public BchInstrument(string instrument) : base(instrument)
        {
        }

        public override string ToStringPrice(double price)
        {
            return price.ToString("0.0000");
        }

        public override double SetBidPrice(ExchangePricesEventArgs e)
        {
            _bidPrice = e.BchPrice[0];
            return _bidPrice;
        }

        public override double SetAskPrice(ExchangePricesEventArgs e)
        {
            _askPrice = e.BchPrice[1];
            return _askPrice;
        }

        public override double RoundPrice(double price)
        {
            return Math.Round(price, 4);
        }
    }
}
