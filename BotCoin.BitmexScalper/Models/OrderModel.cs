using BotCoin.BitmexScalper.Controllers;
using BotCoin.BitmexScalper.Helpers;
using System;
using System.Text;

namespace BotCoin.BitmexScalper.Models
{
    internal class OrderModel
    {
        public string OrderId { set; get; }
        public string ExecInst { set; get; }
        public string Symbol { set; get; }
        public string Side { set; get; }        
        public string Price { set; get; }        
        public int? OrderQty { set; get; }
        public string Type { set; get; }
        public string Status { set; get; }
        public string FullTime { set; get; }
        public string Time { set; get; }

        public void SetOrderQty(int? orderQty)
        {
            if (orderQty.HasValue) 
                this.OrderQty = orderQty.Value;
        }

        public void SetPrice(double? price, MainWindowController wndCtrl)
        {
            if (price.HasValue)
            {
                this.Price = wndCtrl.ToStringPrice(price.Value, this.Symbol);
            }
            else
            {
                if (Status == "New" && Type == "Stop")
                    this.Price = "Market";
            }
        }

        public virtual String Sync(OrderModel model, MainWindowController ctrl)
        {
            var str = new StringBuilder();
            if (model.Side.NotNull())
            {
                this.Side = model.Side;
                str.AppendFormat(" Side='{0}'", model.Side);
            }
            if (model.OrderQty.HasValue)
            {
                this.OrderQty = model.OrderQty.Value;
                str.AppendFormat(" Qty={0}", OrderQty);
            }
            if (model.Price.NotNull())
            {
                this.Price = model.Price;
                str.AppendFormat(" Px={0}", model.Price);
            }            
            return str.Length == 0 ? null : String.Format("{0} {1}", str.ToString(), this.Symbol);
        }

        protected void SetExecInst(string execInst)
        {
            if (!String.IsNullOrEmpty(execInst))
            {
                if (execInst.Contains("ReduceOnly"))
                    this.ExecInst = "ReduceOnly";

                if (execInst.Contains("Close"))
                    this.ExecInst = "Close";
            }
        }
    }
}
