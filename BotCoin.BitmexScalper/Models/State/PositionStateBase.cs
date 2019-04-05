using BotCoin.BitmexScalper.Domain;
using BotCoin.DataType.Database;
using BotCoin.DataType.Exchange;
using System;

namespace BotCoin.BitmexScalper.Models.State
{
    class PositionStateBase : IPositionState
    {
        protected readonly PositionWatcher _watcher;
        protected BitmexOrderData _order;
        protected string _ordSide, _symbol;
        protected long _qty;

        protected PositionStateBase(PositionWatcher watcher, string symbol)
        {
            _watcher = watcher;
            _symbol = symbol;
        }

        protected PositionStateBase(PositionWatcher watcher, string symbol, string ordSide)
            : this(watcher, symbol)
        {
            _ordSide = ordSide;
        }

        public virtual bool OrderFilled(BitmexOrderData ord)
        {
            if (ord.IsFilled)
            {
                return String.Compare(OrderId, ord.OrderId, true) == 0;
            }
            return false;
        }

        public string OrderId
        {
            get { return _order.OrderId; }
        }

        public virtual void SetPrices(double bidPrice, double askPrice)
        {
        }

        public virtual void CreateOrder(long qty, double price)
        {
            throw new NotSupportedException();
        }

        public virtual bool RejectWatching(double bidPrice, double askPrice)
        {
            return false;
        }

        public virtual void CreateMarketOrder(long qty)
        {
            throw new NotSupportedException();
        }

        public virtual string GetStateName()
        {
            throw new NotSupportedException();
        }

        public virtual DbPositionState GetArgs()
        {
            return new DbPositionState { };
        }

        public virtual void Init(DbPositionState state)
        {
        }

        public virtual void Dispose()
        {
        }

        public virtual void ChangeState(string orderId, long qty, double? priceStep)
        {
        }

        public virtual void Copy(DbPositionState state1, DbPositionState state2)
        {
        }
    }
}
