using BotCoin.ApiClient;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using BotCoin.Exchange;
using BotCoin.Instruments;
using BotCoin.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Timers;

namespace BotCoin.ArbitrageService
{
    internal class BittrexArbitrage : StrategyBase
    {
        readonly ServiceEventLogger _log;
        readonly List<BittrexData> _pairs;        
        readonly Timer _timer;
        BittrexClient _client;
        double MinProfitRatio;

        public BittrexArbitrage(ServiceEventLogger log)
        {
            _pairs = new List<BittrexData>();
            _timer = new Timer(40000);
            _log   = log;

            _timer.Elapsed += OnTimerElapsed;
        }

        public bool Enable
        {
            get
            {
                var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
                return config.Settings.Instruments.Enable == 1;
            }
        }

        public void CreateClient(Dictionary<CurrencyName, Instrument> instruments, Func<ExchangeSettingsData> setting)
        {
            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            foreach (InstrumentElement pair in config.Settings.Instruments)
            {
                _pairs.Add(new BittrexData
                {
                    Instrument1 = instruments[pair.Instrument1],
                    Instrument2 = instruments[pair.Instrument2]
                });
            }

            MinProfitRatio = config.Settings.Instruments.MinProfitRatio;
            _client = new BittrexClient(setting());

            OnTimerElapsed(null, null);
            _timer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var pair in _pairs)
            {                
                pair.BaseRatio = Double.MaxValue;
                double sum = 0;
                try
                {
                    foreach (var order in _client.GetOrderBook(pair.Instrument1.GetInstrument(), pair.Instrument2.GetInstrument()))
                    {
                        sum += order.Amount;
                        if (sum >= 2 * pair.Instrument2.GetBalanceStep())
                        {
                            lock (pair) pair.BaseRatio = order.Price;
                            break;
                        }
                    }
                    System.Threading.Thread.Sleep(3000);
                }
                catch (Exception ex)
                {
                    _log.WriteError("Bittrex error: " + ex.Message);
                    pair.BaseRatio = Double.MaxValue;
                    break;
                }
            }
        }

        public void DoArbitrage(Dictionary<ExchangeName, IExchange> exchanges, Action<BittrexArbitrageData> saveArbitrageData)
        {
            foreach (var pair in _pairs)
            {
                double maxBid, minAsk;
                IExchange ex1, ex2;

                var data = new BittrexArbitrageData
                {
                    Instrument1 = pair.Instrument1.GetInstrument(),
                    Instrument2 = pair.Instrument2.GetInstrument()
                };

                if (!GetBestPrices(exchanges, pair.Instrument1, pair.Instrument2, out maxBid, out minAsk, out ex1, out ex2))
                    continue;

                data.Exchange1 = ex1.GetExchangeName();
                data.Exchange2 = ex2.GetExchangeName();
                data.BuyPrice  = maxBid;
                data.SellPrice = minAsk;

                lock (pair)
                {
                    data.Ratio        = pair.Instrument1.GetInstrument() == CurrencyName.BTC ? minAsk / maxBid : maxBid / minAsk;
                    data.ProfitRatio  = pair.BaseRatio == Double.MaxValue ? 0 : Math.Round(100 - (100 * data.Ratio / pair.BaseRatio), 3);
                    data.BittrexRatio = pair.BaseRatio;
                }
                if (data.Profit > MinProfitRatio)
                {
                    double sellUsd   = maxBid * pair.Instrument1.GetBalanceStep();
                    double buyAmount = sellUsd / minAsk;

                    data.Fees         = ex1.TradingFee * sellUsd + ex2.TradingFee * sellUsd;                    
                    data.ProfitRatio -= Math.Round(data.Fees, ex1.CountryCurrencyDecimal);
                
                    saveArbitrageData(data);
                }
            }
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}
