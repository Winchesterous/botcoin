using System;

namespace BotCoin.DataType.Database
{
    public class DbIndicatorVwapLite
    {
        public int Id { set; get; }
        public DateTime Timestamp { set; get; }
        public string Instrument { set; get; }
        public double VwapGainRatio15Min { set; get; }
        public int ExtremClosePrice { set; get; }
    }

    public class DbIndicatorVwap : DbIndicatorVwapLite
    {
        public string TimePeriod { set; get; }
        public string Exchange { set; get; }
        public string Instrument1 { set; get; }
        public string Instrument2 { set; get; }
        public int TradesCount { set; get; }
        public int TotalTradesCount { set; get; }
        public double TotalTradesRatio { set; get; }
        public double TradesCountRatio { set; get; }
        public double HighPrice { set; get; }
        public double LowPrice { set; get; }
        public double OpenPrice { set; get; }
        public double ClosePrice { set; get; }        
        public double SumVolume { set; get; }
        public double SumTypeVol { set; get; }        
        public double? PriceVwapRatio1 { set; get; }
        public double? PriceVwapRatio2 { set; get; }
        public double? CumVwapGain15Min1 { set; get; }
        public double? CumVwapGain15Min2 { set; get; }
        public double? VwapGain15Min1 { set; get; }
        public double? VwapGain15Min2 { set; get; }
        public double? VwapRatioPcnt { set; get; }
        public double Vwap { set; get; }

        public double TypicalPrice
        {
            get { return (HighPrice + LowPrice + ClosePrice) / 3; }
        }

        public double GetVwapPriceSpread()
        {
            double divergence = 0;
            if (PriceVwapRatio1 > 0 && PriceVwapRatio2 > 0)
            {
                divergence = Math.Abs(PriceVwapRatio1.Value - PriceVwapRatio2.Value);
            }
            else if (PriceVwapRatio1 < 0 && PriceVwapRatio2 < 0)
            {
                divergence = Math.Abs(PriceVwapRatio1.Value - PriceVwapRatio2.Value);
            }
            else if (PriceVwapRatio1 >= 0 && PriceVwapRatio2 <= 0)
            {
                divergence = Math.Abs(PriceVwapRatio1.Value - PriceVwapRatio2.Value);
            }
            else if (PriceVwapRatio2 >= 0 && PriceVwapRatio1 <= 0)
            {
                divergence = Math.Abs(PriceVwapRatio2.Value - PriceVwapRatio1.Value);
            }
            else
                throw new InvalidOperationException();

            return divergence;
        }

        public double? GetCumVwapGainSpread()
        {
            if (!CumVwapGain15Min1.HasValue) return null;
            if (!CumVwapGain15Min2.HasValue) return null;

            double divergence = 0;
            if (CumVwapGain15Min1 > 0 && CumVwapGain15Min2 > 0)
            {
                divergence = Math.Abs(CumVwapGain15Min1.Value - CumVwapGain15Min2.Value);
            }
            else if (CumVwapGain15Min1 < 0 && CumVwapGain15Min2 < 0)
            {
                divergence = Math.Abs(CumVwapGain15Min1.Value - CumVwapGain15Min2.Value);
            }
            else if (CumVwapGain15Min1 >= 0 && CumVwapGain15Min2 <= 0)
            {
                divergence = Math.Abs(CumVwapGain15Min1.Value - CumVwapGain15Min2.Value);
            }
            else if (CumVwapGain15Min2 >= 0 && CumVwapGain15Min1 <= 0)
            {
                divergence = Math.Abs(CumVwapGain15Min2.Value - CumVwapGain15Min1.Value);
            }
            else
                throw new InvalidOperationException();

            return divergence;
        }

        public double? GetVwapsSpread()
        {
            if (!VwapGain15Min1.HasValue) return null;
            if (!VwapGain15Min2.HasValue) return null;

            double divergence = 0;
            if (VwapGain15Min1 > 0 && VwapGain15Min2 > 0)
            {
                divergence = Math.Abs(VwapGain15Min1.Value - VwapGain15Min2.Value);
            }
            else if (VwapGain15Min1 < 0 && VwapGain15Min2 < 0)
            {
                divergence = Math.Abs(VwapGain15Min1.Value - VwapGain15Min2.Value);
            }
            else if (VwapGain15Min1 >= 0 && VwapGain15Min2 <= 0)
            {
                divergence = Math.Abs(VwapGain15Min1.Value - VwapGain15Min2.Value);
            }
            else if (VwapGain15Min2 >= 0 && VwapGain15Min1 <= 0)
            {
                divergence = Math.Abs(VwapGain15Min2.Value - VwapGain15Min1.Value);
            }
            else
                throw new InvalidOperationException();

            return divergence;
        }
    }
}
