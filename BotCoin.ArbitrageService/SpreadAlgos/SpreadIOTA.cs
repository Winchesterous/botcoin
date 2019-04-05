using BotCoin.DataType;
using BotCoin.DataType.Database;
using BotCoin.Service;
using System;

namespace BotCoin.ArbitrageService
{
    internal class SpreadIOTA
    {
        readonly DbRepositoryService _dbRepo;

        double[] iotaBinance = new double[2];
        double[] btcBinance = new double[2];
        double[] iotaOkex = new double[2];
        double[] btcOkex = new double[2];
        double binanceFee, okexFee;

        public SpreadIOTA(MatchingEngine matching, DbRepositoryService dbRepo)
        {
            _dbRepo = dbRepo;

            matching.BTC += (s, e) =>
            {
                if (e.Exchange == ExchangeName.Binance)
                {
                    btcBinance[0] = e.BidPrice;
                    btcBinance[1] = e.AskPrice;
                    binanceFee = e.TradingFee;
                }
                else if (e.Exchange == ExchangeName.OkEx)
                {
                    btcOkex[0] = e.BidPrice;
                    btcOkex[1] = e.AskPrice;
                    okexFee = e.TradingFee;
                }
                SaveSpreadOkexBinance();
            };
            matching.IOTA += (s, e) =>
            {
                if (e.Exchange == ExchangeName.Binance)
                {
                    iotaBinance[0] = e.BidPrice;
                    iotaBinance[1] = e.AskPrice;
                    binanceFee = e.TradingFee;
                }
                else if (e.Exchange == ExchangeName.OkEx)
                {
                    if (e.Instrument2 == CurrencyName.USDT)
                    {
                        iotaOkex[0] = e.BidPrice;
                        iotaOkex[1] = e.AskPrice;
                        okexFee = e.TradingFee;
                    }
                }
                SaveSpreadOkexBinance();
            };
        }

        private void SaveSpreadOkexBinance()
        {
            CalculateSpread(() =>
            {
                var btcBalance = 1;
                var usdOkexSell = btcBalance * btcOkex[0] * okexFee;
                var iotaOkexBuy = usdOkexSell / iotaOkex[1] * okexFee;
                var iotaBinanceSell = btcBalance / iotaBinance[0] * binanceFee;
                var spreadPercent = 100 - (iotaOkexBuy / iotaBinanceSell * 100);

                var iotaStep = 800;
                var iotaBinanceBuy = iotaStep * iotaBinance[0] * binanceFee;
                var usdOkexSell = iotaStep * iotaOkex[0] * okexFee;
                var iotaOkexSell = usdOkexSell / btcOkex[1];
                var spread = iotaOkexSell - iotaBinanceBuy;

                var data = new DbSpread
                {
                    Instrument1 = CurrencyName.IOTA,
                    CreatedAt = DateTime.UtcNow,
                    Exchange1 = ExchangeName.OkEx,
                    Exchange2 = ExchangeName.Binance,
                    SpreadPercent = Math.Round(spread, 8),  // Math.Round(spreadPercent, 2),
                    BidAltcUsd = iotaOkex[0],
                    AskAltcUsd = iotaOkex[1],
                    BidAltcInstr2 = iotaBinance[0],
                    AskAltcInstr2 = iotaBinance[1],
                    BidUsd1 = btcOkex[0],
                    AskUsd1 = btcOkex[1],
                    BidUsd2 = btcBinance[0],
                    AskUsd2 = btcBinance[1]
                };
                _dbRepo.SaveSpread(data);
            });
        }

        private void CalculateSpread(Action action)
        {
            if (iotaBinance[0] != 0 && iotaBinance[1] != 0 &&
                btcBinance[0] != 0 && btcBinance[1] != 0 &&
                iotaOkex[0] != 0 && iotaOkex[1] != 0 &&
                btcOkex[0] != 0 && btcOkex[1] != 0)
            {
                binanceFee = 1 - binanceFee;
                okexFee = 1 - okexFee;

                action();

                iotaBinance[0] = iotaBinance[1] =
                btcBinance[0] = btcBinance[1] =
                iotaOkex[0] = iotaOkex[1] =
                btcOkex[0] = btcOkex[1] = 0;
            }
        }
    }
}
