using BotCoin.DataType;
using BotCoin.DataType.Database;
using BotCoin.Service;
using System;

namespace BotCoin.ArbitrageService
{
    internal class SpreadMono
    {
        readonly DbRepositoryService _dbRepo;
                
        double[] iotaBtc = new double[2];
        double[] iotaUsd = new double[2];
        double[] btc     = new double[2];
        double   tradingFee;

        public SpreadMono(MatchingEngine matching, DbRepositoryService dbRepo, ExchangeName exchange)
        {
            _dbRepo = dbRepo;

            matching.BTC += (s, e) =>
            {
                if (e.Exchange == exchange)
                {
                    btc[0] = e.BidPrice;
                    btc[1] = e.AskPrice;
                    tradingFee = e.TradingFee;
                }
                SaveSpread();
            };
            matching.IOTA += (s, e) =>
            {
                if (e.Exchange == exchange)
                {
                    if (e.Instrument2 == CurrencyName.Undefined)
                        throw new InvalidOperationException("SpreadMono");

                    if (e.Instrument2 == CurrencyName.BTC)
                    {
                        iotaBtc[0] = e.BidPrice;
                        iotaBtc[1] = e.AskPrice;
                    }
                    else if (e.Instrument2 == CurrencyName.USDT)
                    {
                        iotaUsd[0] = e.BidPrice;
                        iotaUsd[1] = e.AskPrice;
                    }
                    tradingFee = e.TradingFee;
                }
                SaveSpread();
            };
        }

        private void SaveSpread()
        {
            CalculateSpread(() =>
            {
                var btcBalance = 0.5;
                var buyAltc1   = btcBalance / iotaBtc[1] * tradingFee;
                var buyUsd     = btcBalance / btc[1] * tradingFee;
                var buyAltc2   = buyUsd / iotaUsd[0] * tradingFee;

                var data = new DbSpread
                {
                    Instrument1    = CurrencyName.IOTA,
                    CreatedAt     = DateTime.UtcNow,
                    Exchange1     = ExchangeName.OkEx,
                    Exchange2     = ExchangeName.OkEx,
                    SpreadPercent = Math.Round(buyAltc2 - buyAltc1, 2)
                };
                _dbRepo.SaveSpread(data);
            });
        }

        private void CalculateSpread(Action action)
        {
            if (btc[0]     != 0 && btc[1]     != 0 &&
                iotaBtc[0] != 0 && iotaBtc[1] != 0 &&
                iotaUsd[0] != 0 && iotaUsd[1] != 0)
            {
                tradingFee = 1 - tradingFee;
                action();

                iotaBtc[0] = iotaBtc[1] =
                iotaUsd[0] = iotaUsd[1] =
                btc[0]     = btc[1]     = 0;
            }
        }
    }
}
