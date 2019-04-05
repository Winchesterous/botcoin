using BotCoin.BitmexScalper.Controllers;
using BotCoin.BitmexScalper.Helpers;
using BotCoin.BitmexScalper.Models;
using BotCoin.BitmexScalper.Properties;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BotCoin.DataType.Database;

namespace BotCoin.BitmexScalper
{
    public partial class MainWindow : Window
    {
        readonly Dictionary<string, PositionControl> _posControls;
        readonly Dictionary<string, bool> _markAsRemovedOrders;
        readonly object _obj;

        MainWindowController _controller;
        PriceMonitorWindow _priceWnd;
        ExchangeName _vwapExchange;

        public MainWindow()
        {
            InitializeComponent();

            _markAsRemovedOrders = new Dictionary<string, bool>();
            _posControls = new Dictionary<string, PositionControl>();
            _controller = new MainWindowController(this);
            _obj = new object();

            limitOrderCtrl.MainWnd = stopOrderCtrl.MainWnd = this;
            _vwapExchange = ExchangeName.Bitmex;

            var config = Config;
            var symbolCode = config.BitmexScalper.Contracts.BitmexCode;

            foreach (BitmexContractElement c in config.BitmexScalper.Contracts)
            {
                if (c.Name == "XbtSwap") InitXBT(c, config.BitmexScalper);
                else if (c.Name == "EthSwap") InitETH(c, config.BitmexScalper);
                else if (c.Name == "Ltc") InitLTC(c, config.BitmexScalper, symbolCode);
                else if (c.Name == "Eos") InitEOS(c, config.BitmexScalper, symbolCode);
                else if (c.Name == "Xrp") InitXRP(c, config.BitmexScalper, symbolCode);
                else if (c.Name == "Ada") InitADA(c, config.BitmexScalper, symbolCode);
                else if (c.Name == "Trx") InitTRX(c, config.BitmexScalper, symbolCode);
                else if (c.Name == "Bch") InitBCH(c, config.BitmexScalper, symbolCode);
            }
        }

        internal static BotcoinConfigSection Config
        {
            get { return (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin"); }
        }

        private void InitPositionControl(PositionControl ctrl, BitmexContractElement c, BitmexScalperElement s, string symbolCode)
        {
            ctrl.Instrument       = c.Symbol + symbolCode;
            ctrl.tbPriceSlip.Text = c.PriceSlip.ToString();
            ctrl.tbStopSlip.Text  = c.StopSlip.ToString();
            ctrl.tbRiskPcnt.Text  = s.RiskPercent.ToString("0.0");
            ctrl.tbBalance.Text   = s.XbtBalance.ToString("0.000");

            ctrl.MainWnd = this;
        }

        private void InitXBT(BitmexContractElement c, BitmexScalperElement s)
        {
            _posControls[c.Symbol] = xbtPositionCtrl;
            tabItemXbt.Header = "BTC";

            InitPositionControl(xbtPositionCtrl, c, s, String.Empty);
        }

        private void InitETH(BitmexContractElement c, BitmexScalperElement s)
        {
            _posControls[c.Symbol] = ethPositionCtrl;
            tabItemEth.Header = "ETH";

            InitPositionControl(ethPositionCtrl, c, s, String.Empty);
        }

        private void InitLTC(BitmexContractElement c, BitmexScalperElement s, string symbolCode)
        {
            _posControls[c.Symbol + symbolCode] = ltcPositionCtrl;
            ltcPositionCtrl.PriceMultiplier = 100000;
            tabItemLtc.Header = "LTC";

            InitPositionControl(ltcPositionCtrl, c, s, symbolCode);
        }

        private void InitBCH(BitmexContractElement c, BitmexScalperElement s, string symbolCode)
        {
            _posControls[c.Symbol + symbolCode] = bchPositionCtrl;
            bchPositionCtrl.PriceMultiplier = 10000;
            tabItemBch.Header = "BCH";

            InitPositionControl(bchPositionCtrl, c, s, symbolCode);
        }

        private void InitEOS(BitmexContractElement c, BitmexScalperElement s, string symbolCode)
        {
            _posControls[c.Symbol + symbolCode] = eosPositionCtrl;
            eosPositionCtrl.PriceMultiplier = 10000000;
            tabItemEos.Header = "EOS";

            InitPositionControl(eosPositionCtrl, c, s, symbolCode);
        }

        private void InitXRP(BitmexContractElement c, BitmexScalperElement s, string symbolCode)
        {
            _posControls[c.Symbol + symbolCode] = xrpPositionCtrl;
            xrpPositionCtrl.PriceMultiplier = 100000000;
            tabItemXrp.Header = "XRP";

            InitPositionControl(xrpPositionCtrl, c, s, symbolCode);
        }

        private void InitADA(BitmexContractElement c, BitmexScalperElement s, string symbolCode)
        {
            _posControls[c.Symbol + symbolCode] = adaPositionCtrl;
            adaPositionCtrl.PriceMultiplier = 100000000;
            tabItemAda.Header = "ADA";

            InitPositionControl(adaPositionCtrl, c, s, symbolCode);
        }

        private void InitTRX(BitmexContractElement c, BitmexScalperElement s, string symbolCode)
        {
            _posControls[c.Symbol + symbolCode] = trxPositionCtrl;
            trxPositionCtrl.PriceMultiplier = 1000000000;
            tabItemTrx.Header = "TRX";

            InitPositionControl(trxPositionCtrl, c, s, symbolCode);
        }

        private void LoadVwaps()
        {
            if (!datePicker1.SelectedDate.HasValue) return;

            tradeHistoryCtrl.ClearVwaps();

            var vwaps = _controller.GetVwapGains(datePicker1.SelectedDate.Value, _vwapExchange);
            foreach (var vwap in vwaps.GroupBy(i => i.Timestamp))
            {
                var obj = tradeHistoryCtrl.CreateVwapGain(vwap.Key, vwaps);
                tradeHistoryCtrl.InsertVwapGain(obj);
            }
            lblVwapExchange.Content = _vwapExchange.ToString();
        }

        #region Properties
        BitmexUser BtxAccount
        {
            set; get;
        }

        double BtcBalance
        {
            set
            {
                lblBalance.Content = value.ToString("0.00000") + " XBT";
                _balance = value;
            }
            get
            {
                return _balance;
            }
        }
        double _balance;

        double BtcMargin
        {
            set
            {
                lblAvailMargin.Content = value.ToString("0.00000") + " XBT";
                _margin = value;
            }
            get
            {
                return _margin;
            }
        }
        double _margin;

        internal MainWindowController Controller
        {
            get { return _controller; }
        }
        #endregion

        #region Helpers
        private void Lock(Action action)
        {
            try
            {
                Monitor.Enter(_obj);
                action();
            }
            finally
            {
                Monitor.Exit(_obj);
            }
        }

        internal void ChangeControl(Action action)
        {
            try
            {
                Action callback = () => action();
                Dispatcher.Invoke(callback);
            }
            catch (TaskCanceledException)
            { }
        }

        internal static void Warning(string msg)
        {
            MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        internal static void Error(string msg)
        {
            MessageBox.Show(msg, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        internal static void Error(Exception ex)
        {
            Error(ex.Message);
        }

        internal static bool HandleException(Action action, Action finallyAction = null, Action exAction = null)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (exAction != null) exAction();
                Error(ex);
                return false;
            }
            finally
            {
                if (finallyAction != null)
                    finallyAction();
            }
            return true;
        }

        private void LogMarginEvent(string msg)
        {
            InsertLogEvent(new MarginEventModel { Message = msg });
        }

        internal void LogOrderEvent(string msg)
        {
            InsertLogEvent(new OrderEventModel { Message = msg });
        }

        internal void LogServiceEvent(string msg)
        {
            InsertLogEvent(new ServiceEventModel { Message = msg });
        }

        internal void InsertLogEvent(EventModel model)
        {
            ChangeControl(() => lstEvents.Items.Insert(0, model));
            _controller.LogEvent(model);
        }

        internal void CancelOrder(string orderId)
        {
            long elapsedTime = 0;

            Task.Run(() => HandleException(
                () => elapsedTime = StopWatch.Get(() => _controller.CancelOrders(orderId)),
                () => ChangeControl(() => tbWatchingEvent.Text = String.Format("DEL ORD ({0} ms)", elapsedTime))
                ));
        }

        internal void HandleActionWithMetrics(UIElement element, Action action, string eventName = null)
        {
            if (element != null)
                element.IsEnabled = false;

            Task.Run(() =>
            {
                long elapsedTime = 0;

                HandleException(
                    () => elapsedTime = StopWatch.Get(() => action()),
                    () => ChangeControl(() =>
                    {
                        if (eventName != null)
                            tbWatchingEvent.Text = String.Format("{0} ({1} ms)", eventName, elapsedTime);
                        if (element != null)
                            element.IsEnabled = true;
                    }));
            });
        }
        #endregion

        internal void UpdateOrder<T>(DataGrid grid, BitmexOrderData order, T model, int idx, ListBox listbox = null) where T : OrderModel
        {
            RemoveOrder<T>(grid, order, listbox, null, false);
            InsertOrder<T>(grid, model, idx, listbox, false);
        }

        internal void RemoveOrder<T>(DataGrid grid, BitmexOrderData order, ListBox listBox, Action action = null, bool useMarking = false) where T : OrderModel
        {
            if (useMarking && !_markAsRemovedOrders.ContainsKey(order.OrderId))
                _markAsRemovedOrders.Add(order.OrderId, true);

            if (listBox != null)
                listBox.Items.RemoveFromCollection<T>(order.OrderId);

            if (grid.Items.RemoveFromCollection<T>(order.OrderId))
            {
                if (action != null)
                    action();
            }
        }

        internal void InsertOrder<T>(DataGrid grid, T model, int? idx = null, ListBox listbox = null, bool useMarking = true) where T : OrderModel
        {
            if (useMarking && _markAsRemovedOrders.ContainsKey(model.OrderId))
            {
                _markAsRemovedOrders.Remove(model.OrderId);
                return;
            }
            if (idx.HasValue)
            {
                if (listbox != null)
                    listbox.Items.Insert(idx.Value, model);

                grid.Items.Insert(idx.Value, model);
                grid.Items.Refresh();
            }
            else
            {
                if (listbox != null)
                    listbox.Items.Add(model);

                grid.Items.Add(model);
            }
        }

        internal int ContainsOrder<T>(DataGrid grid, BitmexOrderData order) where T : OrderModel
        {
            return grid.Items.ContainsOrderItem<T>(order.OrderId);
        }

        private void InitHandlers()
        {
            _controller.BotClient.PositionReceived += (s, e) =>
            {
                ChangeControl(() =>
                {
                    var pos = e.Positions[0];
                    tradeHistoryCtrl.InsertPosition(pos);

                    if (pos.IsOpen)
                    {
                        var ctrl = _posControls[pos.Instrument];
                        ctrl.OpenPositionFee = pos.FeeRate;
                        ctrl.CreateBitmexTrade(pos);
                    }
                });
            };
            _controller.BotClient.TradeReceived += (s, e) =>
            {
                ChangeControl(() =>
                {
                    foreach (var item in e.Trades)
                        tradeHistoryCtrl.InsertTrade(item);
                });
            };
            _controller.BotClient.VwapGainReceived += (s, e) =>
            {
                ChangeControl(() =>
                {
                    if (datePicker1.SelectedDate.HasValue && e.Timestamp.Date == datePicker1.SelectedDate.Value)
                    {
                        var obj = tradeHistoryCtrl.CreateVwapGain(e.Timestamp, e.VwapGains);
                        tradeHistoryCtrl.InsertVwapGain(obj);
                    }
                });
            };
            _controller.Exchange.PositionChanged += (s, e) =>
            {
                Parallel.ForEach(e.BtxPositions, pos => _posControls[pos.Symbol].OnPositionChanged(pos));
            };
        }

        private void OnOrderChanged(object sender, ExchangePricesEventArgs args)
        {
            ChangeControl(() =>
            {
                Lock(() =>
                {
                    var bidPrice = _controller.GetBidPrice(args.Symbol);
                    var askPrice = _controller.GetAskPrice(args.Symbol);

                    foreach (var order in args.BtxOrders)
                    {
                        if (order.IsCreated)
                        {
                            if (order.IsLimitOrder)
                            {
                                var msg = String.Format("Limit Px={0} {1}", _controller.ToStringPrice(order.Price.Value, order.Symbol), order.Symbol);
                                limitOrderCtrl.CreateOrder(order, msg);
                            }
                            else if (order.IsMarketOrder)
                            {
                                limitOrderCtrl.CreateOrder(order, "Market");
                            }
                            else if (order.IsMarketStopOrder || order.IsLimitStopOrder)
                            {
                                _posControls[order.Symbol].SetStopPrice(order);
                                stopOrderCtrl.CreateStopOrder(order, bidPrice, askPrice);
                            }
                            else if (order.IsTakeProfitLimitOrder || order.IsTakeProfitMarketOrder)
                            {
                                stopOrderCtrl.CreateStopOrder(order, bidPrice, askPrice);
                            }
                            else
                                throw new InvalidOperationException("Invalid order state " + order.OrdStatus);
                        }
                        else if (order.IsCanceled || order.IsRejected || order.HasTriggered)
                        {
                            limitOrderCtrl.CancelLimit(order);
                            stopOrderCtrl.CancelStopLimit(order);
                        }
                        else if (order.IsPartiallyFilled || order.IsFilled)
                        {
                            limitOrderCtrl.UpdateFilled(order);
                        }
                        else
                        {
                            _posControls[order.Symbol].SetStopPrice(order);
                            stopOrderCtrl.UpdateStopLimit(order, bidPrice, askPrice);
                            limitOrderCtrl.UpdateLimit(order);
                        }
                    }
                });
            });
        }

        private void OnMarginChanged(object sender, ExchangePricesEventArgs e)
        {
            var x = e.BtxMargin;
            ChangeControl(() =>
            {
                HandleException(() =>
                {
                    if (x.WalletBalance.HasValue)
                    {
                        BtcBalance = BitmexMargin.ToBtc(x.WalletBalance.Value, 5);
                        LogMarginEvent("Balance " + lblBalance.Content);
                    }
                    if (x.MarginBalance.HasValue)
                    {
                        BtcMargin = BitmexMargin.ToBtc(x.MarginBalance.Value, 5);
                    }
                    if (x.AvailableMargin.HasValue)
                    {
                        BtcMargin = BitmexMargin.ToBtc(x.AvailableMargin.Value, 5);
                        if (x.MarginUsedPcnt.HasValue)
                        {
                            if (x.MarginUsedPcnt.Value != 0)
                            {
                                var pcnt = Math.Round(100 - x.MarginUsedPcnt.Value * 100, 1);
                                lblAvailMargin.Content = String.Format("{0} ({1}%)", lblAvailMargin.Content.ToString(), pcnt.ToString("0.0"));
                            }
                        }
                    }
                });
            });
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs args)
        {
            var result = HandleException(() =>
            {
                ApplicationState.Load(this);
                _controller.Logon();

                var ex = _controller.Exchange;
                ex.AuthPassed          += OnAuthPassed;
                ex.OrderChanged        += OnOrderChanged;
                ex.MarginChanged       += OnMarginChanged;
                ex.XbtOrderBookChanged += OnXbtOrderBookChanged;
                ex.EthOrderBookChanged += OnEthOrderBookChanged;
                ex.LtcOrderBookChanged += OnLtcOrderBookChanged;
                ex.EosOrderBookChanged += OnEosOrderBookChanged;
                ex.XrpOrderBookChanged += OnXrpOrderBookChanged;
                ex.AdaOrderBookChanged += OnAdaOrderBookChanged;
                ex.BchOrderBookChanged += OnBchOrderBookChanged;
                ex.TrxOrderBookChanged += OnTrxOrderBookChanged;

                _controller.Exchange.SubscriptionAuth(null);
                LoadLeverage();
            },
            null,
            () => this.Close());

            if (result)
                InitHandlers();
        }

        internal void PriceChangedSubscription(bool action, string symbol)
        {
            var ex = _controller.Exchange;
            if (action)
            {
                if (symbol == Settings.Default.XbtSwap)          ex.XbtPriceChanged += _posControls[symbol].PosWatcher.OnXbtSwapPriceChanged;
                else if (symbol == Settings.Default.EthSwap)     ex.EthPriceChanged += _posControls[symbol].PosWatcher.OnEthSwapPriceChanged;
                else if (symbol == Settings.Default.LtcContract) ex.LtcPriceChanged += _posControls[symbol].PosWatcher.OnLtcPriceChanged;
                else if (symbol == Settings.Default.EosContract) ex.EosPriceChanged += _posControls[symbol].PosWatcher.OnEosPriceChanged;
                else if (symbol == Settings.Default.XrpContract) ex.XrpPriceChanged += _posControls[symbol].PosWatcher.OnXrpPriceChanged;
                else if (symbol == Settings.Default.AdaContract) ex.AdaPriceChanged += _posControls[symbol].PosWatcher.OnAdaPriceChanged;
                else if (symbol == Settings.Default.TrxContract) ex.TrxPriceChanged += _posControls[symbol].PosWatcher.OnTrxPriceChanged;
                else if (symbol == Settings.Default.BchContract) ex.BchPriceChanged += _posControls[symbol].PosWatcher.OnBchPriceChanged;
            }
            else
            {
                if (symbol == Settings.Default.XbtSwap)          ex.XbtPriceChanged -= _posControls[symbol].PosWatcher.OnXbtSwapPriceChanged;
                else if (symbol == Settings.Default.EthSwap)     ex.EthPriceChanged -= _posControls[symbol].PosWatcher.OnEthSwapPriceChanged;
                else if (symbol == Settings.Default.LtcContract) ex.LtcPriceChanged -= _posControls[symbol].PosWatcher.OnLtcPriceChanged;
                else if (symbol == Settings.Default.EosContract) ex.EosPriceChanged -= _posControls[symbol].PosWatcher.OnEosPriceChanged;
                else if (symbol == Settings.Default.XrpContract) ex.XrpPriceChanged -= _posControls[symbol].PosWatcher.OnXrpPriceChanged;
                else if (symbol == Settings.Default.AdaContract) ex.AdaPriceChanged -= _posControls[symbol].PosWatcher.OnAdaPriceChanged;
                else if (symbol == Settings.Default.TrxContract) ex.TrxPriceChanged -= _posControls[symbol].PosWatcher.OnTrxPriceChanged;
                else if (symbol == Settings.Default.BchContract) ex.BchPriceChanged -= _posControls[symbol].PosWatcher.OnBchPriceChanged;
            }
        }

        private void OnAuthPassed(object s, EventArgs e)
        {
            if (s == null)
            {
                Error("Authorization failed");
                return;
            }            
            HandleException(() =>
            {
                var elapsedMs = StopWatch.Get(() => BtxAccount = _controller.GetAccountName());
                _controller.SetBitmexSettings(BtxAccount.Id);
            
                ChangeControl(() =>
                {
                    HandleException(() =>
                    {
                        foreach (var ctrl in _posControls)
                        {
                            ctrl.Value.InitPositionWatcher(BtxAccount, _controller.Instruments);
                        }
                        lblBtxAccount.Content = BtxAccount.UserName;
                        tbWatchingEvent.Text = String.Format("ACCOUNT ({0} ms)", elapsedMs);
                        datePicker1.SelectedDate = DateTime.Now.Date;

                        LoadDailyTrades();
                        _controller.SubscribeTopics();
                    });
                });
            });            
        }

        private void LoadDailyTrades()
        {
            Action<string> action = instrument =>
            {
                var dbMsg = _controller.GetDbTrades(BtxAccount, instrument);
                _posControls[instrument].OpenPositionFee = dbMsg.OpenPosFee;

                foreach (var p in dbMsg.Positions.OrderBy(p => p.TransactTime))
                    tradeHistoryCtrl.InsertPosition(p);

                foreach (var t in dbMsg.Trades.OrderBy(p => p.EndTime))
                    tradeHistoryCtrl.InsertTrade(t);
            };

            foreach (var instrument in _posControls.Keys)
                action(instrument);
        }

        private void OnLtcOrderBookChanged(object sender, ExchangePricesEventArgs args)
        {
            ltcPositionCtrl.OnOrderBookChanged(sender, args, (bid, ask) =>
            {
                stopOrderCtrl.UpdateTriggerPrice(args.Symbol, bid, ask);
            });
        }

        private void OnEosOrderBookChanged(object sender, ExchangePricesEventArgs args)
        {
            eosPositionCtrl.OnOrderBookChanged(sender, args, (bid, ask) =>
            {
                stopOrderCtrl.UpdateTriggerPrice(args.Symbol, bid, ask);
            });
        }

        private void OnXrpOrderBookChanged(object sender, ExchangePricesEventArgs args)
        {
            xrpPositionCtrl.OnOrderBookChanged(sender, args, (bid, ask) =>
            {
                stopOrderCtrl.UpdateTriggerPrice(args.Symbol, bid, ask);
            });
        }

        private void OnAdaOrderBookChanged(object sender, ExchangePricesEventArgs args)
        {
            adaPositionCtrl.OnOrderBookChanged(sender, args, (bid, ask) =>
            {
                stopOrderCtrl.UpdateTriggerPrice(args.Symbol, bid, ask);
            });
        }

        private void OnTrxOrderBookChanged(object sender, ExchangePricesEventArgs args)
        {
            trxPositionCtrl.OnOrderBookChanged(sender, args, (bid, ask) =>
            {
                stopOrderCtrl.UpdateTriggerPrice(args.Symbol, bid, ask);
            });
        }

        private void OnBchOrderBookChanged(object sender, ExchangePricesEventArgs args)
        {
            bchPositionCtrl.OnOrderBookChanged(sender, args, (bid, ask) =>
            {
                stopOrderCtrl.UpdateTriggerPrice(args.Symbol, bid, ask);
            });
        }

        private void OnXbtOrderBookChanged(object sender, ExchangePricesEventArgs args)
        {
            xbtPositionCtrl.OnOrderBookChanged(sender, args, (bid, ask) =>
            {
                stopOrderCtrl.UpdateTriggerPrice(args.Symbol, bid, ask);
            });
        }

        private void OnEthOrderBookChanged(object sender, ExchangePricesEventArgs args)
        {
            ethPositionCtrl.OnOrderBookChanged(sender, args, (bid, ask) =>
            {
                stopOrderCtrl.UpdateTriggerPrice(args.Symbol, bid, ask);
            });
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            var ex = _controller.Exchange;
            if (ex != null)
            {
                ex.XbtOrderBookChanged -= OnXbtOrderBookChanged;
                ex.EthOrderBookChanged -= OnEthOrderBookChanged;
                ex.LtcOrderBookChanged -= OnLtcOrderBookChanged;
                ex.EosOrderBookChanged -= OnEosOrderBookChanged;
                ex.XrpOrderBookChanged -= OnXrpOrderBookChanged;
                ex.AdaOrderBookChanged -= OnAdaOrderBookChanged;
                ex.BchOrderBookChanged -= OnBchOrderBookChanged;
                ex.TrxOrderBookChanged -= OnTrxOrderBookChanged;
            }
            ApplicationState.Save(this);
            _controller.Dispose();

            if (_priceWnd != null)
                _priceWnd.Close();

            var states = new List<DbPositionState>();
            foreach (var ctrl in _posControls.Values)
            {
                if (ctrl.PosWatcher != null)
                {
                    var state = ctrl.PosWatcher.GetState();
                    if (state != null)
                        states.Add(state);
                }
            }
            if (states.Count > 0)
                _controller.SavePositionState(BtxAccount, states);
        }

        internal void OnKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;

            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                return;
            if (e.Key >= Key.D0 && e.Key <= Key.D9)
                return;
            if (e.Key == Key.Tab || e.Key == Key.OemPeriod)
                return;

            e.Handled = true;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var header = ((TabItem)tabInstruments.SelectedItem).Header.ToString();

            if (header == "BTC")
                xbtPositionCtrl.OnPreviewKeyDown(sender, e);
            else if (header == "ETH")
                ethPositionCtrl.OnPreviewKeyDown(sender, e);
            else if (header == "LTC")
                ltcPositionCtrl.OnPreviewKeyDown(sender, e);
            else if (header == "EOS")
                eosPositionCtrl.OnPreviewKeyDown(sender, e);
            else if (header == "XRP")
                xrpPositionCtrl.OnPreviewKeyDown(sender, e);
            else if (header == "ADA")
                adaPositionCtrl.OnPreviewKeyDown(sender, e);
            else if (header == "TRX")
                trxPositionCtrl.OnPreviewKeyDown(sender, e);
        }

        private void OnOpenChartsClick(object sender, RoutedEventArgs e)
        {
            _priceWnd = new PriceMonitorWindow(this);
            HandleException(() => _priceWnd.Show());
        }

        private void OnEventsMenuItemClick(object sender, RoutedEventArgs e)
        {
            lstEvents.Items.Clear();
        }

        private void OnDatePickerSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadVwaps();            
        }
                
        private void OnVwapBitmexMenuClick(object sender, RoutedEventArgs e)
        {
            _vwapExchange = ExchangeName.Bitmex;
            LoadVwaps();
        }

        private void OnVwapBinanceMenuClick(object sender, RoutedEventArgs e)
        {
            _vwapExchange = ExchangeName.Binance;
            LoadVwaps();
        }

        private void OnVwapBitstampMenuClick(object sender, RoutedEventArgs e)
        {
            _vwapExchange = ExchangeName.Bitstamp;
            LoadVwaps();
        }
    }
}
