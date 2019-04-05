using BotCoin.DataType;
using BotCoin.Exchange;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotCoin.ArbitrageService
{
    internal class OrderPlacement
    {
        readonly Dictionary<ExchangeName, int> FastExchanges;

        public OrderPlacement()
        {
            FastExchanges = new Dictionary<ExchangeName, int>
            {
                { ExchangeName.Bitstamp, 1 },
                { ExchangeName.Cex, 1 }
            };
        }

        public void ExecuteOrdersAsync(IExchange ex1, IExchange ex2, MatchingData data)
        {
            string order1 = null;
            string order2 = null;
            Task<string> task1 = null;
            Task<string> task2 = null;

            if (FastExchanges.ContainsKey(ex1.GetExchangeName()))
            {
                task1 = Task<string>.Run(() => PlaceOrder(ex1, data.AskPrice1, data.Amount, OrderSide.BID, data));
                order1 = task1.Result;
            }
            else
                order1 = PlaceOrder(ex1, data.AskPrice1, data.Amount, OrderSide.BID, data);

            if (FastExchanges.ContainsKey(ex2.GetExchangeName()))
            {
                task2 = Task<string>.Run(() => PlaceOrder(ex2, data.BidPrice2, data.Amount, OrderSide.ASK, data));
                order2 = task2.Result;
            }
            else
                order2 = PlaceOrder(ex2, data.BidPrice2, data.Amount, OrderSide.ASK, data);

            if (order1 == null && order2 == null)
            {
                data.TransactionState = TradingState.Fail12;
                return;
            }

            if (order1 == null)
                data.TransactionState = TradingState.Fail1;
            else
                data.Order1 = order1;

            if (order2 == null)
                data.TransactionState = TradingState.Fail2;
            else
                data.Order2 = order2;

            ex1.RemoveOrders();
            ex2.RemoveOrders();
        }
        
        private string PlaceOrder(IExchange ex, double price, double amount, OrderSide side, MatchingData data)
        {
            OrderResponse order = null;
            try
            {
                order = ex.PlaceOrder(Math.Round(price, ex.CountryCurrencyDecimal),
                                      Math.Round(amount, ex.CryptoCurrencyDecimal),
                                      data.Instrument,
                                      side);                
            }
            catch (Exception e)
            {
                if (side == OrderSide.BID) data.FailReason1 = e.Message;
                else data.FailReason2 = e.Message;
                return null;
            }
            if (order.Success)
            {
                if (side == OrderSide.BID) data.FailReason1 = order.ErrorReason;
                else data.FailReason2 = order.ErrorReason;
                return null;
            }

            if (side == OrderSide.BID)
                data.BuyUsdAmount = (1 - ex.TradingFee) * data.BuyUsdAmount;
            else
                data.SellUsdAmount = (1 - ex.TradingFee) * data.SellUsdAmount;

            return order.OrderId;
        }
    }
}
