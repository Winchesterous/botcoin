using BotCoin.BitmexScalper.Controllers;
using BotCoin.BitmexScalper.Models;
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace BotCoin.BitmexScalper.Helpers
{
    internal static class ItemCollectionExtensions
    {        
        public static void UpdateStopOrderTriggerPrice(this ItemCollection items, string symbol, double bidPrice, double askPrice, MainWindowController wndCtrl)
        {
            foreach (StopOrderModel item in items)
            {
                if (item.Symbol == symbol)
                    item.SetTriggeringPrice(bidPrice, askPrice, wndCtrl);
            }
            CollectionViewSource.GetDefaultView(items).Refresh();
        }

        public static bool RemoveFromCollection<T>(this ItemCollection items, string orderId) where T : OrderModel
        {
            T obj = null;
            foreach (T i in items)
            {
                if (String.Compare(i.OrderId, orderId, true) == 0)
                {
                    obj = i;
                    break;
                }
            }
            if (obj != null)
                items.Remove(obj);

            return obj != null;
        }

        public static int ContainsOrderItem<T>(this ItemCollection items, string orderId) where T : OrderModel
        {
            for (int idx = 0; idx < items.Count; idx++)
            {
                var item = (T)items[idx];
                if (String.Compare(item.OrderId, orderId, true) == 0)
                    return idx;
            }
            return -1;
        }
    }
}
