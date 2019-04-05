using BotCoin.BitmexScalper.Domain;

namespace BotCoin.BitmexScalper.Models.State
{
    class SellLimitState : LimitState
    {
        public SellLimitState(PositionWatcher watcher, string symbol, double rejectPrice, bool closeOrder = false)
            : base(watcher, symbol, "Sell", rejectPrice, closeOrder)
        {
        }

        public override string GetStateName()
        {
            return "SellLimit";
        }

        public override void SetPrices(double bidPrice, double askPrice)
        {
            CreateNewOrder("Sell", _qty, askPrice, () => askPrice < _order.Price);
        }

        public override bool RejectWatching(double bidPrice, double askPrice)
        {
            return askPrice <= _rejectPrice;
        }

        public override void CreateMarketOrder(long qty)
        {
            var state = new SellMarketState(_watcher, _symbol);
            state.CreateOrder(qty, 0);
        }
    }
}
