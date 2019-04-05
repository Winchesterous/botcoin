using BotCoin.DataType.Database;
using BotCoin.DataType.Exchange;
using System;

namespace BotCoin.BitmexScalper.Models.State
{
    interface IPositionState : IDisposable
    {        
        void CreateOrder(long qty, double price);
        void SetPrices(double bid, double ask);
        bool OrderFilled(BitmexOrderData ord);
        bool RejectWatching(double bidPrice, double askPrice);
        void CreateMarketOrder(long qty);
        string GetStateName();
        DbPositionState GetArgs();
        void Init(DbPositionState state);
        void ChangeState(string orderId, long qty, double? priceStep);
        void Copy(DbPositionState state1, DbPositionState state2);
    }
}
