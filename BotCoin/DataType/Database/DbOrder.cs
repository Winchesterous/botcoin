using System;

namespace BotCoin.DataType.Database
{
    public class DbOrder
    {
        public string Symbol { set; get; }
        public string OrderId { set; get; }
        public string OrdType { set; get; }
        public string OrdSide { set; get; }
        public string OrdStatus { set; get; }
        public int? OrderQty { set; get; }
        public int? CumQty { set; get; }
        public int? LeavesQty { set; get; }
        public double? Price { set; get; }
        public double? AvgPrice { set; get; }
        public double? StopPrice { set; get; }
        public DateTime? CreatedAt { set; get; }
    }
}
