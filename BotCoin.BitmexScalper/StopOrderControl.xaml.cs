using BotCoin.BitmexScalper.Helpers;
using BotCoin.BitmexScalper.Models;
using BotCoin.DataType.Exchange;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BotCoin.BitmexScalper
{
    public partial class StopOrderControl : UserControl
    {
        OrderModel _selectedOrder;
        double _price, _stopPrice;
        int _qty;

        public StopOrderControl()
        {
            InitializeComponent();
        }

        internal MainWindow MainWnd { set; get; }

        public void UpdateTriggerPrice(string symbol, double bid, double ask)
        {
            if (gridStops.HasItems)
                gridStops.Items.UpdateStopOrderTriggerPrice(symbol, bid, ask, MainWnd.Controller);
        }

        internal void CreateStopOrder(BitmexOrderData order, double bidPrice, double askPrice)
        {
            var model = StopOrderModel.ToModel(order, MainWnd.Controller, bidPrice, askPrice);
            MainWnd.InsertOrder<StopOrderModel>(gridStops, model, null, lbStopOrders);

            //if (order.PegOffsetValue.HasValue)
            //{
            //    MainWnd.ChangeControl(() => btnCancelTrail.IsEnabled = true); //!! may be send event
            //    _trailingStopId = order.OrderId;
            //}
            MainWindow.HandleException(() =>
            {
                var price = MainWnd.Controller.ToStringPrice(order.StopPx.Value, order.Symbol);
                var orderId = order.OrderId.GetOrderId();
                order.Message = String.Format("Stop Px={0} {1} ID{2}", price, order.Symbol, orderId);

                MainWnd.LogOrderEvent(order.Message);
            });
        }

        public void UpdateStopLimit(BitmexOrderData order, double bidPrice, double askPrice)
        {
            int idx = MainWnd.ContainsOrder<StopOrderModel>(gridStops, order);
            if (idx != -1)
            {
                var newModel = StopOrderModel.ToModel(order, MainWnd.Controller, bidPrice, askPrice);
                if (newModel.StopPx.HasValue)
                {
                    // TODO: ??
                    //var lbItem = GetListBoxItem<StopOrderModel>(lbStopOrders, order);
                    //lbItem.StopPx = newModel.StopPx.Value;                   
                }
                var model = (StopOrderModel)gridStops.Items[idx];
                MainWindow.HandleException(() =>
                {
                    var msg = model.Sync(newModel, MainWnd.Controller);
                    if (msg != null)
                    {
                        order.Message = String.Format("Update Stop{0} ID{1}", msg, order.OrderId.GetOrderId());
                        MainWnd.LogOrderEvent(order.Message);
                        MainWnd.UpdateOrder<StopOrderModel>(gridStops, order, model, idx);
                    }
                });
            }
        }

        //private T GetListBoxItem<T>(ListBox lb, BitmexOrderData order) where T : OrderModel
        //{
        //    int idx = lb.Items.ContainsOrderItem<StopOrderModel>(order.OrderId);
        //    return (T)lb.Items[idx];
        //}

        public void CancelStopLimit(BitmexOrderData order)
        {
            //if (String.Compare(_trailingStopId, order.OrderId, true) == 0)
            //{
            //    //MainWnd.ChangeControl(() => btnCancelTrail.IsEnabled = false);
            //    _trailingStopId = null;
            //}
            MainWnd.RemoveOrder<StopOrderModel>(gridStops, order, lbStopOrders, () =>
            {
                MainWindow.HandleException(() =>
                {
                    if (order.IsCanceled) order.Message = "Cancel";
                    if (order.IsRejected) order.Message = "Filled";
                    if (order.HasTriggered)
                    {
                        order.Message = "Triggered Stop";
                        order.OrdStatus = order.Triggered;
                    }
                    var msg = String.Format("{0} ID{1}", order.Message, order.OrderId.GetOrderId());
                    MainWnd.LogOrderEvent(msg);
                });
            });
        }

        private void OnGridStopOrderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                _selectedOrder = (StopOrderModel)e.AddedItems[0];

                if (String.Compare(_selectedOrder.Price, "Market", true) != 0)
                    _price = Convert.ToDouble(_selectedOrder.Price);

                _stopPrice = ((StopOrderModel)_selectedOrder).StopPx.Value;
            }
            _qty = _selectedOrder.OrderQty.Value;
        }

        private void OnTemplateQtyTextChanged(object sender, TextChangedEventArgs e)
        {
            _qty = Convert.ToInt32(((TextBox)sender).Text);
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            MainWnd.OnKeyDown(sender, e);
        }

        private void OnChangeOrderQtyClick(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder.OrderQty == _qty) { MainWindow.Warning("Quantity hasn't been changed"); return; }
            MainWnd.HandleActionWithMetrics((Button)sender, () => MainWnd.Controller.UpdateLimitOrder(_selectedOrder.OrderId, _qty), "UPD ORD");
        }

        private void OnTemplatePriceTextChanged(object sender, TextChangedEventArgs e)
        {
            _price = Convert.ToDouble(((TextBox)sender).Text);
        }

        private void OnChangeOrderPriceClick(object sender, RoutedEventArgs e)
        {
            var price = Convert.ToDouble(_selectedOrder.Price);
            if (price == _price) { MainWindow.Warning("Price hasn't been changed"); return; }

            MainWnd.HandleActionWithMetrics((Button)sender, () => MainWnd.Controller.UpdateLimitOrder(_selectedOrder.OrderId, null, _price), "UPD ORD");
        }

        private void OnTemplateStopPriceTextChanged(object sender, TextChangedEventArgs e)
        {
            _stopPrice = Convert.ToDouble(((TextBox)sender).Text);
        }

        private void OnChangeOrderStopPriceClick(object sender, RoutedEventArgs e)
        {
            var price = Convert.ToDouble(((StopOrderModel)_selectedOrder).StopPx);
            if (price == _stopPrice) { MainWindow.Warning("Stop price hasn't been changed."); return; }

            MainWnd.HandleActionWithMetrics((Button)sender, () => MainWnd.Controller.UpdateLimitOrder(_selectedOrder.OrderId, null, null, _stopPrice), "UPD ORD");
        }

        private void OnStopOrderListBoxItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                var dlg = new ConfirmDialog();
                if (dlg.ShowDialog().Value)
                {
                    MainWnd.CancelOrder(((StopOrderModel)e.AddedItems[0]).OrderId);
                }
                else
                    lbStopOrders.UnselectAll();
            }
        }
    }
}
