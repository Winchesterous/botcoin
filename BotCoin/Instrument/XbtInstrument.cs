using BotCoin.DataType;
using System;

namespace BotCoin.Instruments
{
    public class XbtInstrument : InstrumentBaseType
    {
        public XbtInstrument(string instrument) : base(instrument)
        {
        }

        public override string ToStringPrice(double price)
        {
            return price.ToString("0.0");
        }

        public override double GetPnl(long qty, double openPrice, double closePrice)
        {
            return qty * (1 / openPrice - 1 / closePrice);
        }

        public override double SetBidPrice(ExchangePricesEventArgs e)
        {
            _bidPrice = e.BtcPrice[0];
            return _bidPrice;
        }

        public override double SetAskPrice(ExchangePricesEventArgs e)
        {
            _askPrice = e.BtcPrice[1];
            return _askPrice;
        }
        
        public override string FormatOrderValue(long qty, double price)
        {
            return String.Format("{0} XBT", Math.Round(ConvertToPrice(qty, price), 4).ToString("0.0000"));
        }

        public override double ConvertToPrice(long qty, double price)
        {
            return qty / price;
        }

        public override double RoundPrice(double price)
        {
            return GetModulo(Math.Round(price, 1), 0.5, 0.1, 2);
        }
    }
}
