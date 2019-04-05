using BotCoin.DataType;
using BotCoin.DataType.Database;
using BotCoin.Service;
using System;

namespace BotCoin.ArbitrageService
{
    internal class InstrumentSpread
    {
        readonly DbRepositoryService _dbRepo;

        double xrpRatio, dashRatio;
        double[] xrp = new double[2];
        double[] dash = new double[2];

        public InstrumentSpread(MatchingEngine matching, DbRepositoryService dbRepo)
        {
            _dbRepo = dbRepo;

            matching.XRP += (s, e) =>
            {
                if (e.Instrument2 == CurrencyName.ETH)
                {
                    xrp[0] = e.BidPrice;
                    xrp[1] = e.AskPrice;
                }
                SaveSpreadOkexBinance();
            };
            matching.DASH += (s, e) =>
            {
                if (e.Instrument2 == CurrencyName.ETH)
                {
                    dash[0] = e.BidPrice;
                    dash[1] = e.AskPrice;
                }
                SaveSpreadOkexBinance();
            };
        }

        private void SaveSpreadOkexBinance()
        {
            CalculateSpread(() =>
            {
                var x = xrp[1] * xrpRatio;
                var d = dash[1] * dashRatio;
                var spread = Math.Round(x - d, 8);
                var ratio = Math.Round(Math.Abs(spread) / Math.Max(d, x), 4);
                if (spread < 0)
                    ratio *= -1;

                var data = new DbInstrumentSpread
                {
                    Instrument1 = CurrencyName.XRP,
                    Instrument2 = CurrencyName.DSH,
                    CreatedAt = DateTime.UtcNow,
                    Exchange1 = ExchangeName.Binance,
                    Exchange2 = ExchangeName.Binance,
                    SpreadRatio = ratio,
                    Bid1 = xrp[0],
                    Ask1 = xrp[1],
                    Bid2 = dash[0],
                    Ask2 = dash[1]
                };
                _dbRepo.SaveSpread(data);
            });
        }

        private void CalculateSpread(Action action)
        {
            if (xrp[0] != 0 && xrp[1] != 0 &&
                dash[0] != 0 && dash[1] != 0)
            {
                if (dashRatio == 0)
                {
                    xrpRatio = Math.Round(1 / xrp[1], 4);
                    dashRatio = Math.Round(1 / dash[1], 4);
                }
                action();

                xrp[0] = xrp[1] = dash[0] = dash[1] = 0;
            }
        }
    }
}
