using BotCoin.BitmexScalper.Domain;
using BotCoin.DataType.Exchange;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BotCoin.BitmexScalper.Models.State
{
    class LimitState : PositionStateBase
    {
        protected readonly double _rejectPrice;
        readonly List<string> _orders;
        readonly Stopwatch _sw;
        readonly bool _closeOrder;
        
        protected LimitState(PositionWatcher watcher, string symbol, string ordSide, double rejectPrice, bool closeOrder)
            : base(watcher, symbol, ordSide)
        {
            _rejectPrice = rejectPrice;
            _closeOrder  = closeOrder;
            _orders      = new List<string>();
            _sw          = new Stopwatch();
        }

        public override void CreateOrder(long qty, double price)
        {
            _qty = qty;
            _order = _watcher.Controller.CreateLimitOrder(_symbol, _ordSide, qty, price, _closeOrder, "OPN");

            if (_order.OrdStatus == "New")
                _watcher.AddOrder(_order.OrderId);
            
            _sw.Restart();
        }

        protected void CreateNewOrder(string side, long qty, double price, Func<bool> func)
        {
            if (_order == null)
                throw new InvalidOperationException("LimitState.CreateNewOrder is null");

            _sw.Stop();

            if (func())
                CreateOrder(qty, price);
        }

        public override void Dispose()
        {
            if (_order != null)
            {
                Thread.Sleep(100);
                Task.Run(() =>
                {
                    try
                    {
                        _watcher.Controller.CancelOrders(_order.OrderId);
                    }
                    catch (Exception ex)
                    {
                        _watcher.SendWatcherEvent(() => "Dispose() LimitState. " + ex.Message);
                    }
                });
            }
            _sw.Stop();
        }

        public override bool OrderFilled(BitmexOrderData ord)
        {
            if (!base.OrderFilled(ord))
                return false;

            _order = null;
            return true;
        }
    }
}
