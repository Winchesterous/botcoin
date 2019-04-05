using BotCoin.DataType;
using System;

namespace BotCoin.Instruments
{
    public class EosInstrument : InstrumentBaseType
    {
        public EosInstrument(string instrument) : base(instrument)
        {
        }

        public override string ToStringPrice(double price)
        {
            return price.ToString("0.0000000");
        }

        public override double SetBidPrice(ExchangePricesEventArgs e)
        {
            _bidPrice = e.EosPrice[0];
            return _bidPrice;
        }

        public override double SetAskPrice(ExchangePricesEventArgs e)
        {
            _askPrice = e.EosPrice[1];
            return _askPrice;
        }

        public override double RoundPrice(double price)
        {
            return Math.Round(price, 7);
        }
    }
}
