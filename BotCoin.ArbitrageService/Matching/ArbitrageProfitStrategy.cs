using BotCoin.DataType;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BotCoin.ArbitrageService
{
    public class ArbitrageProfitStrategy
    {        
        public TradingState GetProfitState(MatchingData data, List<Tuple<double, double>> profitRatios, Func<double, TradingState> checkBalances)
        {
            if (data.ProfitRatio < profitRatios.Last().Item1)
                return TradingState.NoProfit;

            double buyAmount = Math.Min(data.AskAmount, data.BidAmount);
            double balanceMultiplier = 1;

            foreach (var ratio in profitRatios)
            {
                if (data.ProfitRatio >= ratio.Item1)
                {
                    balanceMultiplier = ratio.Item2;
                    break;
                }
            }
            data.Amount = Math.Min(buyAmount, data.Amount * balanceMultiplier);

            return checkBalances(balanceMultiplier);
        }
    }
}
