using System;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;

namespace BotCoin.Instruments
{
    public class InstrumentBaseType : ICryptoIntrument
    {
        protected BitmexInstrumentSettings _setting;
        protected string _instrumentName;
        protected double _bidPrice, _askPrice;

        public double TickSize
        {
            get { return _setting.TickSize.Value; }
        }

        protected InstrumentBaseType(string instrument)
        {
            _instrumentName = instrument;
        }

        protected double GetModulo(double value, double modulo, double step, int round)
        {            
            for ( ; ; )
            {
                var d = Math.Round(value % modulo, round);
                if (d == modulo) break;

                value = Math.Round(value + step, round - 1);
            }
            return value;
        }

        public string GetSymbol()
        {
            return _instrumentName;
        }

        public double GetBidPrice()
        {
            return _bidPrice;
        }

        public double GetAskPrice()
        {
            return _askPrice;
        }

        public void SetCommission(BitmexInstrumentSettings setting)
        {
            if (setting == null) throw new ArgumentNullException("Commission is undefined");
            _setting = setting;
        }

        public double GetTakerFee()
        {
            return _setting.TakerFee.Value;
        }

        public double GetMakerFee()
        {
            return _setting.MakerFee.Value;
        }

        public virtual double ConvertToPrice(long qty, double price)
        {
            return qty * price;
        }

        public virtual double SetBidPrice(ExchangePricesEventArgs e)
        {
            throw new NotSupportedException();
        }

        public virtual double SetAskPrice(ExchangePricesEventArgs e)
        {
            throw new NotSupportedException();
        }

        public virtual void SetProfit()
        {
            throw new NotImplementedException();
        }

        public virtual double GetPnl(long qty, double openPrice, double closePrice)
        {
            return qty * (closePrice - openPrice);
        }

        public virtual string ToStringPrice(double price)
        {
            throw new NotSupportedException();
        }

        public virtual string FormatOrderValue(long qty, double price)
        {
            return String.Format("{0} XBT", Math.Round(qty * price, 4).ToString("0.0000"));
        }

        public virtual double RoundPrice(double price)
        {
            throw new NotSupportedException();
        }
    }
}
