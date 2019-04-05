using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Exchange;
using BotCoin.Logger;
using BotCoin.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;

namespace BotCoin.TradeDataBotService
{
    internal class SelfHost
    {
        readonly List<IExchange> _exchanges;
        readonly ServiceEventLogger _log;
        BinanceVwapIndicator _binanceVwap;
        BitmexVwapIndicator _bitmexVwap;
        Bitmex _bitmex;

        public SelfHost()
        {
            _log = new ServiceEventLogger(ServiceName.TradeDataBot, new DbRepositoryService());
            _exchanges = new List<IExchange>();
        }

        private string FormatMessage(string msg)
        {
            return String.Format("{0} [{1}]", msg, Dns.GetHostName());
        }

        public void Start()
        {
            var dbRepo = (DbRepositoryService)_log.DbRepository;
            var settings = dbRepo.GetExchangeSettings();
            var instruments = new List<CurrencyName>();

            foreach (CurrencyName ins in Enum.GetValues(typeof(CurrencyName)))
            {
                if ((int)ins > 1 && (int)ins < 100)
                {
                    instruments.Add(ins);
                }
            }
            var btcInstruments = instruments.ToArray();
            var usdInstruments = new CurrencyName[]
            {
                CurrencyName.BTC, CurrencyName.LTC, CurrencyName.ETH, CurrencyName.NEO,
                CurrencyName.BCHABC, CurrencyName.XRP, CurrencyName.ADA, CurrencyName.QTUM,
                CurrencyName.BCHSV, CurrencyName.BTT, CurrencyName.EOS, CurrencyName.TRX,
                CurrencyName.ONT, CurrencyName.XLM, CurrencyName.BNB, CurrencyName.FET,
                CurrencyName.ICX, CurrencyName.VET, CurrencyName.NULS, CurrencyName.IOTA,
                CurrencyName.BAT
            };

            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            var wsExchanges = new Dictionary<string, Type>();
            var exchanges = config.GetWebSocketEnabledExchanges();

            Utils.MapExchangeAttributes((attrName, isWebsocket, type) =>
            {
                if (exchanges.ContainsKey(attrName))
                    if (isWebsocket)
                        wsExchanges[attrName] = type;
            });
            foreach (var exchange in wsExchanges)
            {
                var setting = settings.Where(s => s.Exchange.ToString() == exchange.Key).Single();
                var ex = (BaseExchange)Activator.CreateInstance(exchange.Value, setting);
                ex.Log = _log;

                _exchanges.Add(ex);
                var loadVwap = Int32.Parse(ConfigurationManager.AppSettings["LoadVwap"]);

                try
                {
                    switch (ex.GetExchangeName())
                    {
                    case ExchangeName.Binance:
                        {
                            var exch = (BinanceExchange)ex;
                            _binanceVwap = new BinanceVwapIndicator(_log);

                            if (loadVwap == 1)
                            {
                                _binanceVwap.SaveVwapHistory(_binanceVwap.Date(2019, 4, 1), _binanceVwap.Date(2019, 4, 2));
                            }
                            else
                            {
                                Binance.SubscribeInstruments(exch, btcInstruments, usdInstruments, OnInstrumentReceived);
                                Binance.SubscribeTrades(exch, btcInstruments, usdInstruments, OnTradeReceived);
                                Binance.SubscribeTicker(exch, btcInstruments, usdInstruments, OnTickerReceived);
                                ex.Logon();
                                //_binanceVwap.TimerAction(true);
                            }
                            break;
                        }
                    case ExchangeName.Bitmex:
                        {
                            var btx = (BitmexExchange)ex;
                            _bitmex = new Bitmex(btx);
                            _bitmexVwap = new BitmexVwapIndicator(btx, _bitmex.Contracts, _log);

                            if (loadVwap == 1)
                            {
                                _bitmexVwap.LoadTrades();
                                //_bitmexVwap.SaveVwapHistory(_bitmexVwap.Date(2019, 4, 1), _bitmexVwap.Date(2019, 4, 2));
                            }
                            else
                            {
                                btx.Logon();
                                _bitmex.SubscribeLiquidation(btx, OnLiquidationReceived);
                                _bitmex.SubscribeTicker(btx, OnTickerReceived);
                                _bitmexVwap.TimerAction(true);
                            }
                            break;
                        }
                    case ExchangeName.OkEx:
                        {
                            // mock
                            btcInstruments = new CurrencyName[] { CurrencyName.LTC, CurrencyName.BCH, CurrencyName.ETH, CurrencyName.XRP };
                            usdInstruments = new CurrencyName[] { CurrencyName.BTC, CurrencyName.LTC, CurrencyName.BCH, CurrencyName.ETH, CurrencyName.XRP };

                            ex.Logon();
                            Okex.SubscribeInstruments((OkExExchange)ex, btcInstruments, usdInstruments, OnInstrumentReceived);
                            Okex.SubscribeTrades((OkExExchange)ex, btcInstruments, usdInstruments, OnTradeReceived);
                            break;
                        }
                    case ExchangeName.Bitstamp:
                        {
                            Bitstamp.SubscribeInstruments((BitstampExchange)ex, btcInstruments, usdInstruments, OnInstrumentReceived);
                            Bitstamp.SubscribeTrades((BitstampExchange)ex, btcInstruments, usdInstruments, OnTradeReceived);
                            Bitstamp.SubscribeTicker((BitstampExchange)ex, _log);
                            ex.Logon();
                            break;
                        }
                    case ExchangeName.Gdax:
                        {
                            ex.Logon();
                            Gdax.SubscribeInstruments((GdaxExchange)ex, OnInstrumentReceived);
                            Gdax.SubscribeTicker((GdaxExchange)ex, OnTickerReceived);
                            break;
                        }
                    }
                    _log.WriteInfo(FormatMessage("Service started"));
                }
                catch (Exception e)
                {
                    var msg = String.Format("[{0}] {1} {2}", e.GetType(), e.Message, e.StackTrace);

                    Console.WriteLine(msg);
                    _log.WriteError(msg);
                }
            }            
        }

        public void Stop()
        {
            try
            {
                foreach (var ex in _exchanges)
                {
                    if (ex.GetExchangeName() == ExchangeName.Gdax)
                    {
                        Gdax.Subscribe((GdaxExchange)ex, false);
                    }
                    if (ex.GetExchangeName() == ExchangeName.Binance)
                    {
                        if (_binanceVwap != null)
                            _binanceVwap.TimerAction(false);
                    }
                    if (ex.GetExchangeName() == ExchangeName.Bitmex)
                    {
                        if (_bitmexVwap != null)
                            _bitmexVwap.TimerAction(false);
                    }
                    ex.Logout();
                }
                _log.WriteInfo(FormatMessage("Service stopped"));
            }
            catch (Exception ex)
            {
                _log.WriteError(String.Format("[{0}] {1} {2}", ex.GetType(), ex.Message, ex.StackTrace));
            }
        }

        private void OnInstrumentReceived(object sender, ExchangePricesEventArgs args)
        {
            try
            {
                DbRepositoryService dbRepo = null;
                var s = sender as BaseWebSocketExchange;
                if (s != null)
                {
                    dbRepo = (DbRepositoryService)s.Log.DbRepository;
                }
                else
                {
                    if (sender is BitstampExchange)
                        dbRepo = (DbRepositoryService)((BitstampExchange)sender).Log.DbRepository;
                    else
                        dbRepo = (DbRepositoryService)((BitmexExchange)sender).Log.DbRepository;
                }
                dbRepo.SaveOrderBook(args, args.OrderBook);
            }
            catch (Exception ex)
            {
                _log.WriteError(String.Format("[{0}] {1} {2}", ex.GetType(), ex.Message, ex.StackTrace));
            }
        }

        private void OnTradeReceived(object sender, TradeEventArgs args)
        {
            try
            {
                DbRepositoryService dbRepo = null;
                var s = sender as BaseWebSocketExchange;
                if (s != null)
                {
                    dbRepo = (DbRepositoryService)s.Log.DbRepository;
                }
                else
                {
                    if (sender is BitstampExchange)
                        dbRepo = (DbRepositoryService)((BitstampExchange)sender).Log.DbRepository;
                    else
                        dbRepo = (DbRepositoryService)((BitmexExchange)sender).Log.DbRepository;
                }
                dbRepo.SaveTrade(args);
            }
            catch (Exception ex)
            {
                _log.WriteError(String.Format("[{0}] {1} {2}", ex.GetType(), ex.Message, ex.StackTrace));
            }
        }

        private void OnTickerReceived(object sender, TickerEventArgs args)
        {
            try
            {
                DbRepositoryService dbRepo = null;
                var s = sender as BaseWebSocketExchange;
                if (s != null)
                    dbRepo = (DbRepositoryService)s.Log.DbRepository;
                else
                    dbRepo = (DbRepositoryService)((BitmexExchange)sender).Log.DbRepository;
                dbRepo.SaveTicker(args);
            }
            catch (Exception ex)
            {
                _log.WriteError(String.Format("[{0}] {1} {2}", ex.GetType(), ex.Message, ex.StackTrace));
            }
        }

        private void OnLiquidationReceived(object sender, LiquidationEventArgs args)
        {
            try
            {
                var dbRepo = (DbRepositoryService)((BitmexExchange)sender).Log.DbRepository;
                dbRepo.SaveLiquidation(args);
            }
            catch (Exception ex)
            {
                _log.WriteError(String.Format("[{0}] {1} {2}", ex.GetType(), ex.Message, ex.StackTrace));
            }
        }        
    }
}