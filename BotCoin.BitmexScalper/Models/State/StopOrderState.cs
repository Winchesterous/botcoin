using BotCoin.BitmexScalper.Domain;
using System;

namespace BotCoin.BitmexScalper.Models.State
{
    class StopOrderState : PositionStateBase
    {
        public StopOrderState(PositionWatcher watcher, string symbol, string ordSide) 
            : base(watcher, symbol, ordSide)
        {
            _ordSide = ordSide == "Buy" ? "Sell" : "Buy";
        }

        public override string GetStateName()
        {
            return "StopMarket";
        }

        public override void CreateOrder(long qty, double stopPrice)
        {
            _order = _watcher.Controller.CreateStopMarketOrder(_symbol, _ordSide, Math.Abs(qty), stopPrice, "STOPLS");
        }
    }
}
