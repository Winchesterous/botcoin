using BotCoin.BitmexScalper.Helpers;
using BotCoin.BitmexScalper.Models;
using BotCoin.DataType.Exchange;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BotCoin.BitmexScalper
{
    public partial class LimitOrderControl : UserControl
    {
        OrderModel _selectedOrder;
        double _price;
        int _qty;

        public LimitOrderControl()
        {
            InitializeComponent();
        }

        internal MainWindow MainWnd { set; get; }

        internal void CreateOrder(BitmexOrderData order, string text)
        {
            var model = ActiveOrderModel.ToModel(order, MainWnd.Controller);
            MainWnd.InsertOrder<ActiveOrderModel>(gridOrders, model, null, lbActiveOrders);

            MainWindow.HandleException(() =>
            {
                order.Message = String.Format("{0} ID{1}", text, order.OrderId.GetOrderId());
                MainWnd.LogOrderEvent(order.Message);
            });
        }

        internal void UpdateFilled(BitmexOrderData order)
        {
            int idx = MainWnd.ContainsOrder<ActiveOrderModel>(gridOrders, order);
            if (idx != -1)
            {
                var model = (ActiveOrderModel)gridOrders.Items[idx];
                MainWindow.HandleException(() =>
                {
                    var msg = model.SyncOrderFilled(order, MainWnd.Controller);
                    if (msg != null)
                    {
                        order.Message = String.Format("{0}{1} ({2})", order.OrdStatus, msg, order.OrderId.GetOrderId());
                        MainWnd.LogOrderEvent(order.Message);
                        MainWnd.UpdateOrder<ActiveOrderModel>(gridOrders, order, model, idx, lbActiveOrders);
                    }
                    if (order.OrdStatus == "Filled")
                    {
                        MainWnd.RemoveOrder<ActiveOrderModel>(gridOrders, order, lbActiveOrders);
                    }
                });
            }
        }

        internal void UpdateLimit(BitmexOrderData order)
        {
            int idx = MainWnd.ContainsOrder<ActiveOrderModel>(gridOrders, order);
            if (idx != -1)
            {
                var model = (ActiveOrderModel)gridOrders.Items[idx];
                MainWindow.HandleException(() =>
                {
                    var msg = model.Sync(ActiveOrderModel.ToModel(order, MainWnd.Controller), MainWnd.Controller);
                    if (msg != null)
                    {
                        order.Message = String.Format("Update{0} ID{1}", msg, order.OrderId.GetOrderId());
                        MainWnd.LogOrderEvent(order.Message);
                        MainWnd.UpdateOrder<ActiveOrderModel>(gridOrders, order, model, idx, lbActiveOrders);
                    }
                });
            }
        }

        public void CancelLimit(BitmexOrderData order)
        {
            MainWnd.RemoveOrder<ActiveOrderModel>(gridOrders, order, lbActiveOrders, () =>
            {
                MainWindow.HandleException(() =>
                {
                    order.Message = "Cancel ID" + order.OrderId.GetOrderId();
                    MainWnd.LogOrderEvent(order.Message);
                });
            });
        }

        private void OnActiveOrderListBoxItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
                MainWnd.CancelOrder(((ActiveOrderModel)e.AddedItems[0]).OrderId);
        }

        private void OnGridOrderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                _selectedOrder = (ActiveOrderModel)e.AddedItems[0];
                _price = Convert.ToDouble(_selectedOrder.Price);
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
    }
}
