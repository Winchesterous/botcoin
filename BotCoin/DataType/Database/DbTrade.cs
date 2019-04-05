using System;

namespace BotCoin.DataType.Database
{
    public class DbTrade : DbPositionBase
    {
        public DateTime StartTime { set; get; }
        public DateTime? EndTime { set; get; }
        public string TradeType { set; get; }
        public string ElapsedTime { set; get; }
        public double OpenPrice { set; get; }
        public double? ClosePrice { set; get; }
        public double RiskPcnt { set; get; }
        public double StopValue { set; get; }
        public double? Balance { set; get; }
        public double? PriceGain { set; get; }
        public double? FeePaidXBT { set; get; }
        public double? RealisedPnlXBT { set; get; }

        public string Qty
        {
            get { return Math.Abs(OrderQty).ToString(); }
        }
        public string PriceGainUsd
        {
            get { return PriceGain.HasValue ? PriceGain.Value.ToString("0.0") : null; }
        }
        public string FeePaidXbt
        {
            get { return FeePaidXBT.HasValue ? FeePaidXBT.Value.ToString("0.00000000") : null; }
        }
        public string ProfitXbt
        {
            get
            {
                if (!FeePaidXBT.HasValue || !RealisedPnlXBT.HasValue) return null;
                return (RealisedPnlXBT.Value - FeePaidXBT.Value).ToString("0.00000000");
            }
        }
        public string RealisedPnlXbt
        {
            get { return RealisedPnlXBT.HasValue ? RealisedPnlXBT.Value.ToString("0.00000000") : null; }
        }
        public string BalanceXbt
        {
            get { return Balance.HasValue ? Balance.Value.ToString("0.0000") : null; }
        }
        public string StopUsd
        {
            get { return StopValue.ToString("0.0"); }
        }
        public string Time1
        {
            get { return ToLocalTime(StartTime); }
        }
        public string Time2
        {
            get { return EndTime.HasValue ? ToLocalTime(EndTime.Value) : null; }
        }
        public string TakeStopRatio
        {
            get { return Math.Round(PriceGain.Value / StopValue, 2).ToString("0.00"); }
        }
    }
}
