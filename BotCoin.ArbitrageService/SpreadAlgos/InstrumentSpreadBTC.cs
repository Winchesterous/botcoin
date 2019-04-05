using BotCoin.DataType;
using BotCoin.DataType.Database;
using BotCoin.Service;
using System;

namespace BotCoin.ArbitrageService
{
    internal class InstrumentSpreadBTC
    {
        readonly DbRepositoryService _dbRepo;

        double ratio1, ratio2;
        double[] ex1 = new double[2];
        double[] ex2 = new double[2];

        public InstrumentSpreadBTC(MatchingEngine matching, DbRepositoryService dbRepo)
        {
            _dbRepo = dbRepo;

            matching.BTC += (s, e) =>
            {
                if (e.Exchange == ExchangeName.Binance)
                {
                    ex1[0] = e.BidPrice;
                    ex1[1] = e.AskPrice;
                }
                else if (e.Exchange == ExchangeName.HitBtc)
                {
                    ex2[0] = e.BidPrice;
                    ex2[1] = e.AskPrice;
                }
                else return;
                SaveSpread();
            };
        }

        private void SaveSpread()
        {
            CalculateSpread(() =>
            {
                double spread1 = 0, spread2 = 0;

                if (ex1[0] > ex2[1])
                {
                    spread1 = Math.Round(100 - (ex2[1] / ex1[0] * 100), 4);
                }
                if (ex2[0] > ex1[1])
                {
                    spread2 = Math.Round(100 - (ex1[1] / ex2[0] * 100), 4);
                }
                var data = new DbInstrumentSpread
                {
                    Instrument1 = CurrencyName.BTC,
                    Instrument2 = CurrencyName.BTC,
                    CreatedAt = DateTime.UtcNow,
                    Exchange1 = ExchangeName.Binance,
                    Exchange2 = ExchangeName.HitBtc,
                    Spread1 = spread1,
                    Spread2 = spread2,
                    Bid1 = ex1[0],
                    Ask1 = ex1[1],
                    Bid2 = ex2[0],
                    Ask2 = ex2[1]
                };
                _dbRepo.SaveSpread(data);
            });
        }

        private void CalculateSpread(Action action)
        {
            if (ex1[0] != 0 && ex1[1] != 0 &&
                ex2[0] != 0 && ex2[1] != 0)
            {
                if (ratio1 == 0)
                {
                    ratio1 = 1;
                    ratio2 = Math.Round(ex1[1] / ex2[1], 4);
                }
                action();
                ex1[0] = ex1[1] = ex2[0] = ex2[1] = 0;
            }
        }
    }
}
