using BotCoin.BitmexScalper.Domain;

namespace BotCoin.BitmexScalper.Models.State
{
    class SellMarketState : PositionStateBase
    {
        public SellMarketState(PositionWatcher watcher, string symbol) 
            : base(watcher, symbol)
        { 
        }

        public override string GetStateName()
        {
            return "SellMarket";
        }

        public override void CreateOrder(long qty, double price)
        {
            _order = _watcher.Controller.CreateMarketOrder(_symbol, "Sell", qty, "OPN");
        }
    }
}
