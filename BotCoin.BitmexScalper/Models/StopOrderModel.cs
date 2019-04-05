using BotCoin.BitmexScalper.Controllers;
using BotCoin.BitmexScalper.Helpers;
using BotCoin.DataType.Exchange;
using System;
using System.Text;

namespace BotCoin.BitmexScalper.Models
{
    internal class StopOrderModel : OrderModel
    {
        public string StopPrice { set; get; }        
        public double? StopPx { set; get; }
        public string TriggerPrice { set; get; }
        public string TrailValue { set; get; }

        public static StopOrderModel ToModel(BitmexOrderData order, MainWindowController wndCtrl, double? bidPrice = null, double? askPrice = null)
        {
            var model = new StopOrderModel
            {
                OrderId = order.OrderId,
                Symbol  = order.Symbol,
                Side    = order.Side,
                Type    = order.OrdType,
                Status  = order.OrdStatus,
                Time    = order.Timestamp.ToLocalTime().ToLongTimeString(),
                FullTime = order.Timestamp.ToLocalTime().ToString()
            };
            
            model.SetPrice(order.Price, wndCtrl);
            model.SetOrderQty(order.OrderQty);
            model.SetStopPrice(order.Side, order.StopPx, wndCtrl);
            model.SetTriggeringPrice(bidPrice, askPrice, wndCtrl);
            model.SetExecInst(order.ExecInst);
            model.SetTrailValue(order.PegOffsetValue, wndCtrl);

            return model;
        }
                
        private void SetTrailValue(double? trail, MainWindowController ctrl)
        {
            if (trail.HasValue)
                this.TrailValue = ctrl.ToStringPrice(trail.Value, this.Symbol);
        }

        internal bool SetStopPrice(string side, double? stopPrice, MainWindowController ctrl)
        {
            if (stopPrice.HasValue)
                this.StopPx = stopPrice.Value;
            else
                return false;

            if (side.Null())
                return false;

            this.StopPrice = String.Format("{0} {1}", side == "Buy" ? ">=" : "<=", ctrl.ToStringPrice(stopPrice.Value, this.Symbol));
            return true;
        }                

        public void SetTriggeringPrice(double? bidPrice, double? askPrice, MainWindowController ctrl)
        {
            if (!bidPrice.HasValue || !askPrice.HasValue) return;
            if (!this.StopPx.HasValue || !this.OrderQty.HasValue) return;

            var currentPrice = OrderQty.Value > 0 ? bidPrice.Value : askPrice.Value;
            var triggerPrice = currentPrice - StopPx.Value;
            
            string sign = "";
            if (triggerPrice > 0) sign = "+";

            this.TriggerPrice = String.Format(
                "{0} ({1}{2})",
                ctrl.ToStringPrice(currentPrice, this.Symbol), 
                sign, 
                ctrl.ToStringPrice(triggerPrice, this.Symbol)
                );
        }

        public override string Sync(OrderModel model, MainWindowController ctrl)
        {
            var str = new StringBuilder(base.Sync(model, ctrl));
            var m = (StopOrderModel)model;

            if (m.StopPx.HasValue)
            {
                if (SetStopPrice(this.Side, m.StopPx.Value, ctrl))
                    str.AppendFormat(" StopPx={0}", ctrl.ToStringPrice(m.StopPx.Value, this.Symbol));
            }
            return str.Length == 0 ? null : str.ToString();
        }
    }
}
