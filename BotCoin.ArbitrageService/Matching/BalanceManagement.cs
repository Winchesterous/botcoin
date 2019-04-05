using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Exchange;
using BotCoin.Instruments;
using System;
using System.Configuration;

namespace BotCoin.ArbitrageService
{
    internal class BalanceManagement
    {
        readonly ArbitrageProfitStrategy _profitStrategy;
        readonly double ProfitRatio;

        public BalanceManagement()
        {
            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            _profitStrategy = new ArbitrageProfitStrategy();

            ProfitRatio = config.Settings.TradingStrategy.MinMatchingRatio;
        }

        private void SetUsdAmounts(IExchange ex1, IExchange ex2, MatchingData data)
        {
            data.BuyUsdAmount  = data.AskPrice1 * data.Amount;
            data.SellUsdAmount = data.BidPrice2 * data.Amount;
            data.Profit        = Math.Round(data.SellUsdAmount - data.BuyUsdAmount, ex1.CountryCurrencyDecimal);
            data.Fees          = (data.BuyUsdAmount * ex1.TradingFee) + (data.SellUsdAmount * ex2.TradingFee);
            data.Profit        = Math.Round(data.Profit - data.Fees, ex1.CountryCurrencyDecimal);
            data.ProfitRatio   = Math.Round(data.Profit / data.SellUsdAmount * 100, ex1.CountryCurrencyDecimal);            
        }

        private void InitPrices(MatchingData data, double bidPrice, double askPrice, double bidOrderAmount, double askOrderAmount)
        {
            data.AskPrice1 = askPrice;
            Utils.ThrowIf(data.AskPrice1 == 0, "AskPrice1 == 0");
            
            data.BidPrice2 = bidPrice;
            Utils.ThrowIf(data.BidPrice2 == 0, "BidPrice2 == 0");

            data.AskAmount = askOrderAmount;
            data.BidAmount = bidOrderAmount;
        }

        public void SetTradingState(IExchange ex, Instrument ins)
        {
            if (ex.GetMinTradeValue(CurrencyName.USD) > ex.UsdBalance)
            {
                ex.TradingState = TradingState.NoUsd;
            }
            if (ins.GetCryptoBalance(ex) < ins.GetBalanceStep())
            {
                ex.TradingState = TradingState.NoCrypt;
            }
        }

        private TradingState CheckBalancesImpl(IExchange ex1, IExchange ex2, Instrument ins, MatchingData data)
        {
            double amount = data.Amount;            
#region BUY            
            double minValue = data.AskPrice1 * ((BaseExchange)ex1).MinUsdValueRatio;
            Func<double> reduceAmount = () => (ex1.UsdBalance - minValue) / data.AskPrice1;
            Func<TradingState> rejectUsd = () => { ex1.TradingState = TradingState.NoUsd; return ex1.TradingState; };
            Func<TradingState> rejectCrypt = () => { ex2.TradingState = TradingState.NoCrypt; return ex2.TradingState; };

            if (ex1.UsdBalance <= ex1.GetMinTradeValue(CurrencyName.USD))
                return rejectUsd();

            if (ex1.UsdBalance < data.BuyUsdAmount)
            {
                if (ex1.UsdBalance - minValue <= ex1.GetMinTradeValue(CurrencyName.USD))
                    return rejectUsd();

                data.Amount = reduceAmount();
            }
            else
            {
                if (ex1.UsdBalance - data.BuyUsdAmount <= ex1.GetMinTradeValue(CurrencyName.USD))
                    data.Amount = reduceAmount();
            }
#endregion
#region SELL
            var cryptoBalance = ins.GetCryptoBalance(ex2);

            if (cryptoBalance <= ex2.GetMinTradeValue(ins.GetInstrument(), data.BidPrice2))
                return rejectCrypt();

            if (cryptoBalance < data.Amount)
                data.Amount = cryptoBalance;
#endregion
            if (amount != data.Amount)
                SetUsdAmounts(ex1, ex2, data);

            ex1.TradingState = ex2.TradingState = TradingState.Ok;
            return TradingState.Ok;
        }

        public TradingState Trading(IExchange ex1, IExchange ex2, Instrument ins, MatchingData data, double maxBid, double minAsk, int depth = 0)
        {
            data.SetExchange1(ex1);
            data.SetExchange2(ex2);

            InitPrices(data, maxBid, minAsk, ins.GetBidOrderAmount(ex2), ins.GetAskOrderAmount(ex1));
            SetUsdAmounts(ex1, ex2, data);

            if (data.ProfitRatio < 0)
                return depth == 1 ? TradingState.Negative : Trading(ex2, ex1, ins, data, maxBid, minAsk, 1);

            var state = CheckBalancesImpl(ex1, ex2, ins, data);
            if (state != TradingState.Ok)
                return state;

            state = _profitStrategy.GetProfitState(data, ins.GetProfitRatios(), multiplier =>
                {
                    if (multiplier != 1) SetUsdAmounts(ex1, ex2, data);
                    return CheckBalancesImpl(ex1, ex2, ins, data);
                });

            if (state != TradingState.Ok)
                return state;

            if (data.Profit < ins.GetMinUsdProfit())
                return TradingState.NoProfit;

            return TradingState.Ok;
        }

        public double GetProfitRatio(IExchange ex1, IExchange ex2, Instrument ins, MatchingData data)
        {
            data.AskPrice1 = ins.GetAskPrice(ex1);
            data.BidPrice2 = ins.GetBidPrice(ex2);
            SetUsdAmounts(ex1, ex2, data);

            return data.ProfitRatio;
        }

        public TradingState BackTrading(IExchange ex1, IExchange ex2, Instrument ins, MatchingData data)
        {
            data.SetExchange1(ex1);
            data.SetExchange2(ex2);

            InitPrices(data, ins.GetBidPrice(ex2), ins.GetAskPrice(ex1), ins.GetBidOrderAmount(ex2), ins.GetAskOrderAmount(ex1));

            var amount = data.Amount;
            SetUsdAmounts(ex1, ex2, data);

            if (data.ProfitRatio < ProfitRatio)
                return TradingState.Negative;
                        
            var buyAmount = Math.Min(data.AskAmount, data.BidAmount);
            if (buyAmount < data.Amount)
                return TradingState.Reject;

            if (data.ProfitRatio > 0)
            {
                data.Amount = Math.Min(buyAmount, data.Amount);
            }

            double avgAmount = (ins.GetCryptoBalance(ex1) + ins.GetCryptoBalance(ex2)) / 2;
            if (buyAmount < avgAmount)
                return TransactionState.Reject;

            data.Amount = Math.Min(buyAmount, data.Amount);
            SetUsdAmounts(ex1, ex2, data);

            var state = CheckBalancesImpl(ex1, ex2, ins, data);
            if (state != TradingState.Ok)
                return state;

            return TradingState.Back;
        }
    }
}
