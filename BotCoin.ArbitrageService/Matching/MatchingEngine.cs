using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Exchange;
using BotCoin.Instruments;
using BotCoin.Logger;
using BotCoin.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotCoin.ArbitrageService
{
    internal class MatchingEngine : StrategyBase
    {
        readonly Dictionary<ExchangeName, IExchange> _exchanges;
        readonly Dictionary<ExchangeName, double> _profitRatios;
        readonly Dictionary<ExchangeName, BaseExchange> _wsExchanges;
        readonly Dictionary<CurrencyName, Instrument> _wsInstruments;
        readonly Dictionary<CurrencyName, Instrument> _instruments;
        readonly CancellationPattern _singleInstrument;
        readonly CancellationPattern _manyInstruments;
        readonly CancellationPattern _cancelMonitoring;
        readonly MessageHandling _msgHandling;
        readonly DbRepositoryService _dbRepo;
        readonly SyncBalanceService _syncService;
        readonly ServiceEventLogger _log;
        readonly AutoResetEvent _matchingEvent;
        readonly AutoResetEvent _stopEvent1;
        readonly AutoResetEvent _stopEvent2;
        readonly OrderPlacement _orderPlacement;
        readonly BalanceManagement _balance;
        readonly BittrexArbitrage _bittrex;
        readonly OrderBook _orderBook;
        readonly object _obj;

        internal event EventHandler<MatchingEventArgs> BTC;
        internal event EventHandler<MatchingEventArgs> XRP;
        internal event EventHandler<MatchingEventArgs> DASH;
        internal event EventHandler<MatchingEventArgs> IOTA;

        SpreadMono _spreadMono;
        SpreadIOTA _spreadIota;
        InstrumentSpreadBTC _spread;

        public MatchingEngine(ServiceEventLogger log, MessageHandling msgHandling)
        {
            _singleInstrument = new CancellationPattern();
            _manyInstruments = new CancellationPattern();
            _orderPlacement = new OrderPlacement();
            _cancelMonitoring = new CancellationPattern();
            _wsExchanges = new Dictionary<ExchangeName, BaseExchange>();
            _exchanges = new Dictionary<ExchangeName, IExchange>();
            _profitRatios = new Dictionary<ExchangeName, double>();
            _instruments = new Dictionary<CurrencyName, Instrument>();
            _wsInstruments = new Dictionary<CurrencyName, Instrument>();
            _balance = new BalanceManagement();
            _matchingEvent = new AutoResetEvent(false);
            _stopEvent1 = new AutoResetEvent(false);
            _stopEvent2 = new AutoResetEvent(false);
            _bittrex = new BittrexArbitrage(log);
            _orderBook = new OrderBook();
            _obj = new object();
            _dbRepo = (DbRepositoryService)log.DbRepository;
            _syncService = new SyncBalanceService(log);
            _msgHandling = msgHandling;
            _log = log;

            Instrument.InitWebsocketInstruments(_wsInstruments);

            _instruments.Add(CurrencyName.BTC, new InstrumentBTC());
            _instruments.Add(CurrencyName.BCH, new InstrumentBCH());
            _instruments.Add(CurrencyName.LTC, new InstrumentLTC());
            _instruments.Add(CurrencyName.ETH, new InstrumentETH());
            _instruments.Add(CurrencyName.XRP, new InstrumentXRP());
            _instruments.Add(CurrencyName.DSH, new InstrumentDASH());
            _instruments.Add(CurrencyName.IOTA, new InstrumentIOTA());

            _spreadMono = new SpreadMono(this, _dbRepo, ExchangeName.OkEx);
            _spreadIota = new SpreadIOTA(this, _dbRepo);
            _spread = new InstrumentSpreadBTC(this, _dbRepo);
        }

        public ServiceEventLogger Log
        {
            get { return _log; }
        }

        private void CreateExchanges(Action<List<IExchange>> syncBalance)
        {
            var settings = _msgHandling.GetExchangeSettings();
            var exchanges = new List<IExchange>();

            Action<KeyValuePair<string, Type>, bool> initExchange = (pair, isWebsocket) =>
             {
                 var setting = settings.Where(s => s.Exchange.ToString() == pair.Key).Single();
                 var ex = (BaseExchange)Activator.CreateInstance(pair.Value, setting);
                 var exName = ex.GetExchangeName();
                 _orderBook.InitOrder(exName);
                 _exchanges[exName] = ex;
                 exchanges.Add(ex);
                 if (isWebsocket)
                     _wsExchanges[ex.GetExchangeName()] = ex;
             };

            #region Rest
            var restExchanges = _msgHandling.GetRestEnabledExchanges();
            var dict = new Dictionary<string, Type>();

            Utils.MapExchangeAttributes((attrName, isWebsocket, type) =>
            {
                if (restExchanges.ContainsKey(attrName))
                    if (!isWebsocket)
                        dict[attrName] = type;
            });

            foreach (var pair in dict)
                initExchange(pair, false);
            #endregion

            #region WebSocket
            var wsExchanges = _msgHandling.GetWebSocketEnabledExchanges();
            dict.Clear();

            Utils.MapExchangeAttributes((attrName, isWebsocket, type) =>
            {
                if (wsExchanges.ContainsKey(attrName))
                    if (isWebsocket)
                        dict[attrName] = type;
            });

            foreach (var pair in dict)
                initExchange(pair, true);
            #endregion

            syncBalance(exchanges);

            foreach (var ex1 in exchanges)
            {
                ex1.Exchanges = new List<IExchange>();
                foreach (var ex2 in exchanges)
                {
                    if (ex1.GetExchangeName() != ex2.GetExchangeName())
                        ex1.Exchanges.Add(ex2);
                }
                ((BaseExchange)ex1).Log = _log;
                ex1.DbGateway = _dbRepo;
                ex1.InitExchange();
            }
        }

        private void OrderBookWebSocketHandler(ExchangeName exName, OrderBookEventArgs args)
        {
            var instrument = _wsInstruments[args.Instrument1];
            var exchange = _wsExchanges[exName];

            if (!exchange.ValidOrderPrices(args, instrument))
                return;

            var priceArgs = new ExchangePricesEventArgs
            {
                CreatedAt = DateTime.UtcNow,
                Timestamp = args.Timestamp,
                Instrument1 = args.Instrument1,
                Instrument2 = args.Instrument2,
                OrderId = args.OrderId,
                Exchange = exchange.GetExchangeName(),
                IsOrderDeleted = args.IsOrderDeleted
            };

            Task.Run(() => _dbRepo.SaveOrderBook(priceArgs, args.OrderBook));

            instrument.InitExchangePrice(exchange, priceArgs);
            OnExchangePrices(null, priceArgs);
        }

        public void Start()
        {
            CreateExchanges(exchanges => _syncService.Start(exchanges));

            foreach (var client in _wsExchanges.Values)
            {
                client.Logon();

                foreach (var ins in _wsInstruments.Values)
                    ins.SubscribeOrderBook(client, (n, o) => OrderBookWebSocketHandler(n, o));
            }

            if (_bittrex.Enable)
            {
                _bittrex.CreateClient(_instruments, () => _msgHandling.GetExchangeSettings().Where(s => s.Exchange == ExchangeName.Bittrex).Single());
                ManyInstrumentsArbitrage();
            }
            else
                SingleInstrumentArbitrage();
        }

        public void Stop()
        {
            foreach (var client in _wsExchanges.Values)
            {
                client.Logout();
                foreach (var i in _wsInstruments.Values)
                    i.UnsubscribeOrderBook(client);
            }

            _bittrex.Stop();
            _syncService.Stop();
            StopMatching();
        }

        public void OnExchangePrices(object sender, ExchangePricesEventArgs args)
        {
            var instrument = _instruments[args.Instrument1];

            _orderBook.HandlingOrder(ex => instrument.CreateOrder(ex, instrument, args),
                                     _exchanges[args.Exchange],
                                     _matchingEvent,
                                     instrument,
                                     args
                                     );
        }

        public string GetState()
        {
            return _singleInstrument.IsCancelled ? "Cancelled" : "Active";
        }

        public void StopMatching()
        {
            if (!_singleInstrument.IsCancelled)
            {
                _singleInstrument.Cancel();
                _matchingEvent.Set();
                _stopEvent1.WaitOne();
            }
            if (!_manyInstruments.IsCancelled)
            {
                _manyInstruments.Cancel();
                _matchingEvent.Set();

                if (_bittrex.Enable)
                    _stopEvent2.WaitOne();
            }
        }

        public void ManyInstrumentsArbitrage()
        {
            _manyInstruments.Run();

            Task.Run(() =>
            {
                _manyInstruments.ThreadActivity(_log.Service,
                    () =>
                    {
                        _bittrex.DoArbitrage(_exchanges, data => _dbRepo.SaveBittrexArbitrageAsync(data));
                        _matchingEvent.WaitOne();
                    },
                    data =>
                    {
                        _manyInstruments.Cancel();
                        _log.WriteError("Terminated due to error: " + data.Message);
                    },
                    () =>
                    {
                        _matchingEvent.Set();
                        _stopEvent2.Set();
                    });
            },
            _manyInstruments.Token);
        }

        private void InitTradingStates()
        {
            foreach (var instrument in _instruments.Values)
            {
                foreach (var ex in _exchanges.Values)
                {
                    _balance.SetTradingState(ex, instrument);
                }
            }
        }

        public void SingleInstrumentArbitrage()
        {
            InitTradingStates();
            _singleInstrument.Run();

            Task.Run(() =>
            {
                _singleInstrument.ThreadActivity(_log.Service,
                    () =>
                    {
                        try
                        {
                            ArbitrageImpl();
                            ScalpingBitstamp();
                        }
                        catch (Exception ex)
                        {
                            _log.WriteError(ex.StackTrace);
                            throw ex;
                        }
                        _matchingEvent.WaitOne();
                    },
                    data =>
                    {
                        _singleInstrument.Cancel();
                        _log.WriteError("Terminated due to error: " + data.Message);
                    },
                    () =>
                    {
                        _matchingEvent.Set();
                        _stopEvent1.Set();
                    });
            },
            _singleInstrument.Token);
        }

        private void ScalpingBitstamp()
        {
            ForeachInstrument((ex, inst) =>
            {
            },
            ExchangeName.Bitstamp);
        }

        private void ArbitrageImpl()
        {
            ForeachInstrument((ex, inst) =>
            {
                var instrument = inst.GetInstrument();
                var args = new MatchingEventArgs(ex.GetExchangeName(), inst.Instrument2, inst.GetBidPrice(ex), inst.GetAskPrice(ex), ex.TradingFee);

                switch (instrument)
                {
                case CurrencyName.BTC: BTC(null, args); break;
                case CurrencyName.XRP: XRP(null, args); break;
                case CurrencyName.DSH: DASH(null, args); break;
                case CurrencyName.IOTA: IOTA(null, args); break;
                }
            });
        }

        private bool BackTradingImpl(IExchange ex1, IExchange ex2, Instrument instrument, bool checkBalance = false)
        {
            double balance1 = instrument.GetCryptoBalance(ex1);
            double balance2 = instrument.GetCryptoBalance(ex2);
            double avgAmount = instrument.GetAvgAmount();
            double amount = instrument.GetBalanceStep();

            if (checkBalance)
            {
                if (Math.Abs(balance1 - balance2) < 1.5 * amount)
                    return false;
            }

            var data = InitTransData(amount, instrument);
            data.TransactionState = _balance.BackTrading(ex1, ex2, instrument, data);

            MatchingImpl(ex1, ex2, instrument, data);
            return data.TransactionState == TradingState.Back;
        }

        private void ForeachInstrument(Action<IExchange, Instrument> action, ExchangeName exchange = ExchangeName.Undefined)
        {
            foreach (var instrument in _instruments.Values)
            {
                foreach (var ex in _exchanges.Values)
                {
                    if (exchange != ExchangeName.Undefined && ex.GetExchangeName() != exchange) continue;
                    lock (ex)
                    {
                        if (!instrument.ContainsOrders(ex)) continue;
                        action(ex, instrument);
                    }
                }
            }
        }

        private void ForeachExchanges(Instrument instrument, Func<IExchange, IExchange, bool> action)
        {
            foreach (var ex1 in _exchanges.Values)
            {
                lock (ex1)
                {
                    if (!instrument.ContainsOrders(ex1))
                        continue;

                    foreach (var ex2 in ex1.Exchanges)
                    {
                        if (!instrument.ContainsOrders(ex2))
                            continue;

                        lock (ex2)
                        {
                            if (action(ex1, ex2))
                                break;
                        }
                    }
                }
            }
        }

        private bool TryToExchange(IExchange ex1, Instrument ins, out IExchange exBuy, out IExchange exSell)
        {
            var data = InitTransData(ins.GetBalanceStep(), ins);
            exBuy = null; exSell = null;

            foreach (var ex2 in ex1.Exchanges)
            {
                if (!ins.ContainsOrders(ex2)) continue;
                lock (ex2)
                {
                    if (ex1.TradingState == TradingState.NoCrypt)
                    {
                        if (ex2.TradingState == TradingState.NoUsd || ex2.TradingState == TradingState.Ok)
                        {
                            _profitRatios[ex2.GetExchangeName()] = _balance.GetProfitRatio(ex1, ex2, ins, data);
                        }
                    }
                    data.AskPrice1 = data.BidPrice2 = 0;
                }
            }
            if (_profitRatios.Count == 0)
                return false;

            var min = _profitRatios.OrderBy(kvp => kvp.Value).First();
            var max = _profitRatios.OrderBy(kvp => kvp.Value).Last();

            _profitRatios.Clear();
            return true;
        }

        private TradingState BearBuy(IExchange ex1, IExchange ex2, Instrument ins)
        {
            var data = InitTransData(ins.GetBalanceStep(), ins);

            data.TransactionState = _balance.Trading(ex1, ex2, ins, data, );
            MatchingImpl(ex1, ex2, ins, data);

            return data.TransactionState;
        }

        private void BackTrading(Instrument instrument)
        {
            ForeachExchanges(instrument, (ex1, ex2) =>
            {
                if (ex1.TradingState == TradingState.Ok)
                {
                    if (ex2.TradingState == TradingState.NoCrypt)
                    {
                        if (BackTradingImpl(ex2, ex1, instrument, true))
                            return true;
                    }
                    if (ex2.TradingState == TradingState.NoUsd)
                    {
                        if (BackTradingImpl(ex1, ex2, instrument, true))
                            return true;
                    }
                }
                if (ex1.TradingState == TradingState.NoCrypt)
                {
                    if (ex2.TradingState == TradingState.NoUsd)
                    {
                        if (BackTradingImpl(ex1, ex2, instrument))
                            return true;
                    }
                    if (ex2.TradingState == TradingState.Ok)
                    {
                        if (BackTradingImpl(ex1, ex2, instrument, true))
                            return true;
                    }
                }
                if (ex1.TradingState == TradingState.NoUsd)
                {
                    if (ex2.TradingState == TradingState.NoCrypt)
                    {
                        if (BackTradingImpl(ex2, ex1, instrument))
                            return true;
                    }
                    if (ex2.TradingState == TradingState.Ok)
                    {
                        if (BackTradingImpl(ex2, ex1, instrument, true))
                            return true;
                    }
                }
                return false;
            });
        }

        private void Trading(Instrument instrument)
        {
            double maxBid = 0, minAsk = Double.MaxValue;
            IExchange ex1 = null, ex2 = null;

            if (!GetBestPrices(_exchanges, instrument, instrument, out maxBid, out minAsk, out ex1, out ex2))
                return;

            lock (ex1)
            {
                lock (ex2)
                {
                    if (ex1.TradingState == TradingState.Ok && ex2.TradingState == TradingState.Ok)
                    {
                        var amount = instrument.GetAskAmount(ex1);
                        var data = InitTransData(amount, instrument);

                        if (amount == 0)
                        {
                            Log.WriteError("Amount zero");
                            return;
                        }
                        data.TransactionState = _balance.Trading(ex1, ex2, instrument, data, maxBid, minAsk);
                        MatchingImpl(ex1, ex2, instrument, data);
                    }
                }
            }
        }

        private async void MatchingImpl(IExchange ex1, IExchange ex2, Instrument ins, MatchingData data)
        {
            if (data.TransactionState == TradingState.Ok ||
                data.TransactionState == TradingState.Back)
            {
                if (!_orderBook.TryExecuteOrders(ex1, ex2))
                    return;

                _orderPlacement.ExecuteOrdersAsync(ex1, ex2, data);
            }
            if (data.TransactionState == TradingState.NoPrice ||
                data.TransactionState == TransactionState.Negative ||
                data.TransactionState == TradingState.Reject)
            {
                return;
            }
            if (data.HasRejectedState || data.TransactionState == TradingState.Negative ||
                data.TransactionState == TradingState.Fail12)
            {
                await _dbRepo.SaveMatchingAsync(data);
            }
            else
                _dbRepo.SaveMatchingAsync(data, ex1, ex2).Wait();
        }

        private MatchingData InitTransData(double amount, Instrument instrument)
        {
            var data = new MatchingData { CreatedAt = DateTime.UtcNow };

            data.TransactionState = TradingState.Reject;
            data.Instrument = instrument.GetInstrument();
            data.Amount = amount;

            return data;
        }
    }
}
