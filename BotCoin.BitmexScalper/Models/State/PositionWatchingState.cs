using System;
using BotCoin.BitmexScalper.Domain;
using BotCoin.DataType.Exchange;
using BotCoin.DataType.Database;
using BotCoin.BitmexScalper.Controllers;

namespace BotCoin.BitmexScalper.Models.State
{
    class PositionWatchingState : IPositionState
    {
        double _startWatchPrice, _stopPrice;
        bool _isLongPosition;
        long _qty;

        readonly Action<double> createBuyState;
        readonly Action<double> createSellState;
        readonly PositionWatcher _watcher;
        readonly MainWindowController _controller;

        public PositionWatchingState(PositionWatcher watcher, string symbol)
        {
            _watcher = watcher;
            Action<LimitState, double> initWatching = (state, price) =>
            {
                state.CreateOrder(_qty, price);
                watcher.PositionState = state;
                watcher.StartTimer();
            };
            createBuyState = price =>
            {
                var state = (LimitState)new BuyLimitState(watcher, symbol, _stopPrice, true);
                initWatching(state, price);
            };
            createSellState = price =>
            {
                var state = (LimitState)new SellLimitState(watcher, symbol, _stopPrice, true);
                initWatching(state, price);
            };
            _controller = watcher.Controller;
        }

        public PositionWatchingState(PositionWatcher watcher, string symbol, bool isLongPosition, double startWatchPrice, long qty, double stopPrice) 
            : this(watcher, symbol)
        {
            _startWatchPrice = startWatchPrice;
            _isLongPosition  = isLongPosition;
            _stopPrice       = stopPrice;
            _qty             = qty;

            _watcher.SendWatcherEvent(() => GetFormatedPrices());
        }

        public void Init(DbPositionState state)
        {
            _isLongPosition  = state.LongPosition == 1;
            _startWatchPrice = state.StartWatchPrice.Value;
            _stopPrice       = state.StopPrice.Value;
            _qty             = state.OrderQty.Value;

            _watcher.SendWatcherEvent(() => GetFormatedPrices());
        }

        public DbPositionState GetArgs()
        {
            return new DbPositionState
            {
                LongPosition    = _isLongPosition ? 1 : 0,
                StartWatchPrice = _startWatchPrice,
                StopPrice       = _stopPrice,
                OrderQty        = _qty
            };
        }        

        public void SetPrices(double bid, double ask)
        {
            if (_isLongPosition)
            {
                if (ask <= _startWatchPrice)
                    createSellState(ask);
            }
            else
            {
                if (bid >= _startWatchPrice)
                    createBuyState(ask);
            }
        }

        public void ChangeState(string orderId, long qty, double? priceStep)
        {
            if (!priceStep.HasValue)
            {
                _controller.UpdateStopOrder(orderId, qty);
                _qty = qty;
            }
            else
            {
                if (_isLongPosition)
                {
                    _startWatchPrice += priceStep.Value;
                    _stopPrice += priceStep.Value;
                }
                else
                {
                    _startWatchPrice -= priceStep.Value;
                    _stopPrice -= priceStep.Value;
                }
                _controller.UpdateStopOrder(orderId, qty, _stopPrice);
                _watcher.SendWatcherEvent(() => GetFormatedPrices());
            }
        }

        private string GetFormatedPrices()
        {
            return String.Format("StopPx={0}, StartWatchPx={1}", _stopPrice, _startWatchPrice);
        }

        public bool RejectWatching(double bidPrice, double askPrice)
        {
            return false;
        }

        public bool OrderFilled(BitmexOrderData ord)
        {
            return false;
        }

        public string GetStateName()
        {
            return "Position";
        }

        public void Dispose()
        {
        }

        public void Copy(DbPositionState state1, DbPositionState state2)
        {
            if (state1 == null) return;

            state1.StopOrderId = state2.StopOrderId;
            state1.StopLoss   = state2.StopLoss;
            state1.StopSlip    = state2.StopSlip;
        }

        #region Not supported
        public void CreateMarketOrder(long qty)
        {
            throw new NotSupportedException();
        }

        public void CreateOrder(long qty, double price)
        {
            throw new NotSupportedException();
        }        
        #endregion
    }
}
