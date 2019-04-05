using BotCoin.DataType;
using System;

namespace BotCoin.Instruments
{
    public class EthInstrument : InstrumentBaseType
    {
        // https://www.bitmex.com/app/seriesGuide/ETH

        const double BitcoinMultiplier = 0.000001;

        public EthInstrument(string instrument) : base(instrument)
        {
        }

        public override string ToStringPrice(double price)
        {
            return price.ToString("0.00");
        }

        public override double GetPnl(long qty, double openPrice, double closePrice)
        {
            return qty * (closePrice - openPrice) * BitcoinMultiplier;
        }

        public override double SetBidPrice(ExchangePricesEventArgs e)
        {
            _bidPrice = e.EthPrice[0];
            return _bidPrice;
        }

        public override double SetAskPrice(ExchangePricesEventArgs e)
        {
            _askPrice = e.EthPrice[1];
            return _askPrice;
        }

        public override string FormatOrderValue(long qty, double price)
        {
            return String.Format("{0} XBT", Math.Round(qty / price, 4).ToString("0.0000"));
        }

        public override double ConvertToPrice(long qty, double price)
        {
            return qty * price * BitcoinMultiplier;
        }

        public override double RoundPrice(double price)
        {
            return GetModulo(Math.Round(price, 2), 0.05, 0.01, 3);
        }
    }
}
