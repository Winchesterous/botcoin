using BotCoin.ApiClient;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using BotCoin.Instruments;
using BotCoin.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace BotCoin.Service
{
    public class TickerService
    {
        readonly Dictionary<CurrencyName, Instrument> _restInstruments;
        //readonly Dictionary<CurrencyName, Instrument> _wsInstruments;
        readonly ServiceEventLogger _log;
        readonly BitfinexClient _bitfinex;
        readonly Timer _timer;

        public TickerService(Dictionary<CurrencyName, Instrument> restInstruments,
                             //Dictionary<CurrencyName, Instrument> wsInstruments,
                             ExchangeSettingsData[] settings,
                             ServiceEventLogger log)
        {
            _bitfinex = new BitfinexClient(settings.Where(s => s.Exchange == ExchangeName.Bitfinex).Single().RestUrl);
            _timer    = new Timer(60 * 60000);
            _log      = log;

            _restInstruments = restInstruments;
            //_wsInstruments = wsInstruments;

            _timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Update();
        }

        private void SetAveragePrice(CurrencyName instrument, string pairName, Action wait = null)
        {
            var exist1 = _restInstruments.ContainsKey(instrument);
            //var exist2 = _wsInstruments.ContainsKey(instrument);

            if (!exist1 /*&& !exist2*/)
                return;

            if (wait != null)
                wait();

            var vwap = _bitfinex.GetTicker(pairName).Mid;

            if (_restInstruments.ContainsKey(instrument))
                _restInstruments[instrument].SetAveragePrice(vwap);

            //if (_wsInstruments.ContainsKey(instrument))
            //    _wsInstruments[instrument].SetAveragePrice(vwap);
        }

        public void Update()
        {
            Action wait = () => System.Threading.Thread.Sleep(1000);
            try
            {
                SetAveragePrice(CurrencyName.BTC, "btcusd");
                SetAveragePrice(CurrencyName.BCH, "bchusd", () => wait());
                SetAveragePrice(CurrencyName.ETH, "ethusd", () => wait());
                SetAveragePrice(CurrencyName.LTC, "ltcusd", () => wait());
                SetAveragePrice(CurrencyName.XRP, "xrpusd", () => wait());
                SetAveragePrice(CurrencyName.DSH, "dshusd", () => wait());
            }
            catch (Exception ex)
            {
                _log.WriteError(ex.Message);
            }
            _log.WriteInfo("Ticker updated");
        }

        public void Start()
        {
            _timer.Start();
        }
                
        public void Stop()
        {
            _timer.Stop();
        }
    }
}
