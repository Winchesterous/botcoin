using System;
using BotCoin.BitmexScalper.Domain;
using BotCoin.DataType.Database;

namespace BotCoin.BitmexScalper.Models.State
{
    class InitState : IPositionState
    {
        public InitState(PositionWatcher watcher, string instrument)
        {
        }

        public void CreateOrder(long qty, double price)
        {
        }

        public void SetPrices(double bid, double ask)
        {
        }

        public bool OrderFilled(DataType.Exchange.BitmexOrderData ord)
        {
            return false;
        }
                
        public bool RejectWatching(double bidPrice, double askPrice)
        {
            return false;
        }

        public void CreateMarketOrder(long qty)
        {
        }

        public string GetStateName()
        {
            return "Init";
        }

        public DbPositionState GetArgs()
        {
            return new DbPositionState { }; 
        }

        public void Init(DbPositionState state)
        {
        }

        public void Dispose()
        {
        }

        public void ChangeState(string orderId, long qty, double? priceStep)
        {
        }

        public void Copy(DbPositionState state1, DbPositionState state2)
        {
        }
    }
}
