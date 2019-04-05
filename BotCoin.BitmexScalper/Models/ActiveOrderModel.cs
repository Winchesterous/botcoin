using BotCoin.BitmexScalper.Controllers;
using BotCoin.BitmexScalper.Helpers;
using BotCoin.DataType.Exchange;
using System;
using System.Text;

namespace BotCoin.BitmexScalper.Models
{
    internal class ActiveOrderModel : OrderModel
    {
        public string OrderValue { set; get; }        
        public int? CumQty { set; get; }
        public int? LeavesQty { set; get; }
        public double? AvgPrice { set; get; }

        public static ActiveOrderModel ToModel(BitmexOrderData data, MainWindowController wndCtrl)
        {            
            var model = new ActiveOrderModel
            {
                OrderId = data.OrderId,
                Symbol  = data.Symbol,
                Status  = data.OrdStatus,
                Type    = data.OrdType,
                Side    = data.Side,
                Time    = data.Timestamp.ToLocalTime().ToLongTimeString(),
                FullTime = data.Timestamp.ToLocalTime().ToString()
            };

            model.SetOrderQty(data.OrderQty);
            model.SetPrice(data.Price, wndCtrl);
            model.SetExecInst(data.ExecInst);

            if (data.OrderQty.HasValue && data.Price.HasValue)                
                model.OrderValue = wndCtrl.GetOrderValue(data.OrderQty.Value, data.Price.Value, model.Symbol);

            return model;
        }
         
        public string SyncOrderFilled(BitmexOrderData order, MainWindowController ctrl)
        {
            var str = new StringBuilder();
            if (order.CumQty.HasValue)      // исполненная часть заявки
            {
                this.CumQty = order.CumQty.Value;
                str.AppendFormat(" CumQty={0}", order.CumQty.Value);
            }
            if (order.LeavesQty.HasValue)   // неисполненная часть заявки
            {
                this.LeavesQty = order.LeavesQty.Value;
                str.AppendFormat(" LeavesQty={0}", order.LeavesQty.Value);
            }
            if (order.AvgPx.HasValue)       // средневзвешенная цена сделок по заявке
            {
                this.AvgPrice = order.AvgPx.Value;
                str.AppendFormat(" AvgPx={0}", ctrl.ToStringPrice(order.AvgPx.Value, this.Symbol));
            }
            return str.Length == 0 ? null : str.ToString();
        }

        public override string Sync(OrderModel model, MainWindowController ctrl)
        {
            var str = base.Sync(model, ctrl);
            if (OrderQty.HasValue && this.Price.NotNull())
            {
                var price = Convert.ToDouble(this.Price);
                OrderValue = ctrl.GetOrderValue(OrderQty.Value, price, this.Symbol);
            }
            return str;
        }
    }
}
