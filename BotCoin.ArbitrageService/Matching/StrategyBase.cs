using BotCoin.DataType;
using BotCoin.Exchange;
using BotCoin.Instruments;
using System;
using System.Collections.Generic;

namespace BotCoin.ArbitrageService
{
    internal class StrategyBase
    {
        protected bool GetBestPrices(Dictionary<ExchangeName, IExchange> exchanges,
                                     Instrument ins1,
                                     Instrument ins2,
                                     out double maxBid,
                                     out double minAsk,
                                     out IExchange ex1,
                                     out IExchange ex2)
        {
            maxBid = 0; minAsk = Double.MaxValue;
            ex1 = null; ex2 = null;

            foreach (var ex in exchanges.Values)
            {
                lock (ex)
                {
                    if (ex.TradingState != TradingState.Ok)
                        continue;

                    var price = ins1.GetBidPrice(ex);
                    if (price != 0 && price > maxBid)
                    {
                        maxBid = price;
                        ex2 = ex;
                    }
                    price = ins2.GetAskPrice(ex);
                    if (price != 0 && price < minAsk)
                    {
                        minAsk = price;
                        ex1 = ex;
                    }
                }
            }

            if (maxBid == 0)
                return false;

            if (minAsk == Double.MaxValue)
                return false;

            if (ex1.GetExchangeName() == ex2.GetExchangeName())
                return false;

            return true;
        }
    }
}
