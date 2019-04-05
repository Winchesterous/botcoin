using BotCoin.BitmexScalper.Domain;

namespace BotCoin.BitmexScalper.Models.State
{
    class BuyMarketState : PositionStateBase
    {
        public BuyMarketState(PositionWatcher watcher, string symbol) 
            : base(watcher, symbol)
        { 
        }

        public override string GetStateName()
        {
            return "BuyMarket";
        }

        public override void CreateOrder(long qty, double price)
        {
            _order = _watcher.Controller.CreateMarketOrder(_symbol, "Buy", qty, "OPN");
        }
    }
}
