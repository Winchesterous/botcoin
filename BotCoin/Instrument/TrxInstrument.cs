using BotCoin.DataType;
using System;

namespace BotCoin.Instruments
{
    public class TrxInstrument : InstrumentBaseType
    {
        public TrxInstrument(string instrument) : base(instrument)
        {
        }

        public override string ToStringPrice(double price)
        {
            return price.ToString("0.00000000");
        }

        public override double SetBidPrice(ExchangePricesEventArgs e)
        {
            _bidPrice = e.TrxPrice[0];
            return _bidPrice;
        }

        public override double SetAskPrice(ExchangePricesEventArgs e)
        {
            _askPrice = e.TrxPrice[1];
            return _askPrice;
        }

        public override double RoundPrice(double price)
        {
            return Math.Round(price, 8);
        }
    }
}
