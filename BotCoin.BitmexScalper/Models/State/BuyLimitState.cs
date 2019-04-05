using BotCoin.BitmexScalper.Domain;

namespace BotCoin.BitmexScalper.Models.State
{
    class BuyLimitState : LimitState
    {
        public BuyLimitState(PositionWatcher watcher, string symbol, double rejectPrice, bool closeOrder = false) 
            : base(watcher, symbol, "Buy", rejectPrice, closeOrder)
        {
        }

        public override string GetStateName()
        {
            return "BuyLimit";
        }

        public override void SetPrices(double bidPrice, double askPrice)
        {
            CreateNewOrder("Buy", _qty, bidPrice, () => bidPrice > _order.Price);
        }

        public override bool RejectWatching(double bidPrice, double askPrice)
        {
            return bidPrice >= _rejectPrice;
        }

        public override void CreateMarketOrder(long qty)
        {
            var state = new BuyMarketState(_watcher, _symbol);
            state.CreateOrder(qty, 0);
        }
    }
}
