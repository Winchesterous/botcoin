using BotCoin.DataType.Exchange;
using System;

namespace BotCoin.DataType.Database
{
    public class DbPosition : DbPositionBase
    {
        public DateTime TransactTime { set; get; }
        public long LeavesQty { set; get; }
        public long CumQty { set; get; }
        public string Side { set; get; }                
        public long ExecCost { set; get; }
        public double? ExecCostXBT { set; get; }        
        public double Price { set; get; }
        public double FeeRate { set; get; }
        public bool IsOpen { set; get; }

        public string Qty
        {
            get { return OrderQty.ToString(); }
        }
        public string CostXBT
        {
            get
            {
                if (ExecCostXBT.HasValue) return ExecCostXBT.Value.ToString();
                return BitmexMargin.ToBtc(ExecCost).ToString("0.00000000");
            }
        }
        public string LvsQty
        {
            get { return LeavesQty.ToString(); }
        }
        public string CummQty
        {
            get { return CumQty.ToString(); }
        }
        public string PriceValue
        {
            get { return Price.ToString("0.0"); }
        }
        public string CommissionPcnt
        {
            get { return String.Format("{0}%", Math.Round(FeeRate * 100, 3)); }
        }
        public string Time
        {
            get { return ToLocalTime(TransactTime); }
        }        
    }
}
