using BotCoin.BitmexScalper.Controllers;
using BotCoin.BitmexScalper.Domain;
using BotCoin.BitmexScalper.Models;
using BotCoin.DataType;
using BotCoin.DataType.Database;
using BotCoin.DataType.Exchange;
using BotCoin.Instruments;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BotCoin.BitmexScalper
{
    public partial class PositionControl : UserControl
    {
        readonly PositionController _posController;
        readonly object _obj;
        PositionWatcher _posWatcher;
        InstrumentBaseType _instrumentObj;
        string _trailingStopId, _instrument;
        double _priceMultiplier;
        double? _stopPx;

        static PositionControl()
        {
            PriceMultiplierProperty = DependencyProperty.Register("PriceMultiplier", typeof(double), typeof(PositionControl));
            InstrumentProperty      = DependencyProperty.Register("Instrument", typeof(string), typeof(PositionControl));
        }

        public event EventHandler<PosWatcherEventArgs> PositionSizeChanged;

        public PositionControl()
        {
            InitializeComponent();

            _posController = new PositionController();
            _obj = new object();

            gbPosition.Visibility = Visibility.Hidden;
            rbOrdTypeAuto.IsChecked = true;

            var takeProfitPcntItems = new List<string>
            {
                "","100","90","80","70","60","50","40","30","20","10"
            };
            cbProfitPcnt1.ItemsSource =
            cbProfitPcnt2.ItemsSource =
            cbProfitPcnt3.ItemsSource = new ObservableCollection<string>(takeProfitPcntItems);
            takeProfitPcntItems.RemoveAt(0);

            _posController.Changed += (s, e) => ChangeControl(() => ShowPosition(e));            
        }

        public void InitPositionWatcher(BitmexUser account, Dictionary<string, ICryptoIntrument> instruments)
        {
            _priceMultiplier = PriceMultiplier;
            _instrument      = Instrument;
            _instrumentObj   = (InstrumentBaseType)instruments[_instrument];
            _posWatcher      = new PositionWatcher(_instrument, MainWnd.Controller, MainWindow.Error);

            var ex = MainWnd.Controller.Exchange;

            ex.PositionChanged += _posWatcher.OnPositionChanged;
            ex.OrderChanged    += _posWatcher.OnOrderChanged;
            
            this.PositionSizeChanged += (s, e) =>
            {
                MainWindow.HandleException(() => _posWatcher.OnPositionSizeChanged(e.PositionSize));
            };
            _posWatcher.WatcherChanged += (s, e) =>
            {
                ChangeControl(() => LogWatcherEvent(e.Message));
            };
            _posWatcher.StateChanged += (s, e) => ChangeControl(() =>
            {
                lblState.Content = e.Message;
                var state = e.Message == "BuyLimit" || e.Message == "SellLimit";
                btnCancelWatching.Visibility = state ? Visibility.Visible : Visibility.Hidden;
            });
            MainWnd.PriceChangedSubscription(true, _instrument);
            _posWatcher.Init(account);
        }

        private void UpdateModel(string symbol, Action<PositionModel> action)
        {
            if (_posController.PositionOpened(symbol))
            {
                var model = _posController.GetPosition(symbol);
                action(model);
                _posController.Refresh(model);
            }
        }

        public void CreateBitmexTrade(DbPosition pos)
        {
            var stopLoss = GetAlignedStopLoss(Convert.ToDouble(lblBidPrice.Content));

            if (stopLoss.HasValue)
                MainWnd.Controller.CreateBitmexTrade(pos, stopLoss.Value, Convert.ToDouble(tbRiskPcnt.Text), 0, 0);//!! _posWatcher.StopPrice.Value, _posWatcher.StartWatchingPrice.Value);
        }

        #region Helpers
        private long CalcPositionSize(double pcnt)
        {
            return (long)(ActualPositionSize * pcnt / 100 + 0.5);
        }

        void LogWatcherEvent(string msg)
        {
            MainWnd.InsertLogEvent(new PosWatcherEventModel { Message = msg });
        }

        void Warning(string msg)
        {
            MainWindow.Warning(msg);
        }

        void Error(string msg)
        {
            MainWindow.Error(msg);
        }

        void ChangeControl(Action action)
        {
            try
            {
                Action callback = () => action();
                Dispatcher.Invoke(callback);
            }
            catch (TaskCanceledException)
            { }
        }

        private void LogPositionEvent(string msg)
        {
            MainWnd.InsertLogEvent(new PositionEventModel { Message = msg });
        }

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

        private void SetPriceLabelWidth(string instrument)
        {
            if (instrument.StartsWith("XBT") || 
                instrument.StartsWith("ETH"))      gridPrices.ColumnDefinitions[0].Width = gridPrices.ColumnDefinitions[1].Width = new GridLength(70);
            else if (instrument.StartsWith("LTC")) gridPrices.ColumnDefinitions[0].Width = gridPrices.ColumnDefinitions[1].Width = new GridLength(80);
            else if (instrument.StartsWith("EOS")) gridPrices.ColumnDefinitions[0].Width = gridPrices.ColumnDefinitions[1].Width = new GridLength(95);
            else if (instrument.StartsWith("XRP")) gridPrices.ColumnDefinitions[0].Width = gridPrices.ColumnDefinitions[1].Width = new GridLength(105);
            else if (instrument.StartsWith("ADA")) gridPrices.ColumnDefinitions[0].Width = gridPrices.ColumnDefinitions[1].Width = new GridLength(105);
            else if (instrument.StartsWith("TRX")) gridPrices.ColumnDefinitions[0].Width = gridPrices.ColumnDefinitions[1].Width = new GridLength(105);
            else if (instrument.StartsWith("BCH")) gridPrices.ColumnDefinitions[0].Width = gridPrices.ColumnDefinitions[1].Width = new GridLength(80);
        }
        #endregion

        #region Dependency properties
        public static readonly DependencyProperty InstrumentProperty;
        public static readonly DependencyProperty PriceMultiplierProperty;

        public string Instrument
        {
            set { SetValue(InstrumentProperty, value); SetPriceLabelWidth(value); }
            get { return (string)GetValue(InstrumentProperty); }
        }

        public double PriceMultiplier
        {
            set { SetValue(PriceMultiplierProperty, value); }
            get { return (double)GetValue(PriceMultiplierProperty); }
        }
        #endregion

        #region Properties
        internal MainWindow MainWnd
        {
            set; get;
        }

        internal PositionWatcher PosWatcher
        {
            get { return _posWatcher; }
        }

        double ConvertValueFromString(string text)
        {
            double value = 0;
            if (text.Length == 0) return 0;
            if (!Double.TryParse(text, out value))
            {
                Error("Invalid digit format");
                return 0;
            }
            return value;
        }

        double StopSlip
        {
            get { return ConvertValueFromString(tbStopSlip.Text) * _instrumentObj.TickSize; }
        }

        public double? StopLossPercent
        {
            get
            {
                if (tbStopLoss1.Text.Length == 0 && tbStopLoss2.Text.Length == 0)
                {
                    Error("Empty stop-loss percent");
                    return null;
                }
                var text = String.Format("{0}.{1}", tbStopLoss1.Text, tbStopLoss2.Text);
                return ConvertValueFromString(text);
            }
        }

        int PositionSize
        {
            set
            {
                lblPositionSize.Content = value.ToString();
                _positionSize = value;
            }
            get
            {
                return _positionSize;
            }
        }
        int _positionSize;

        long ActualPositionSize
        {
            set { _actPosSize = Math.Abs(value); }
            get { return _actPosSize; }
        }
        long _actPosSize;

        string PositionSide
        {
            set; get;
        }

        public double? OpenPositionFee
        {
            set; get;
        }
        #endregion

        public void OnOrderBookChanged(ExchangePricesEventArgs args, Action<double, double> action)
        {
            ChangeControl(() =>
            {
                Lock(() =>
                {
                    var ctrl = MainWnd.Controller;
                    var bid  = ctrl.SetBidPrice(args);
                    var ask  = ctrl.SetAskPrice(args);

                    UpdateModel(args.Symbol, model =>
                    {
                        model.OpenPosFee = OpenPositionFee;
                        model.SetProfit(MainWnd.Controller, bid, ask);
                    });

                    action(bid, ask);
                });
            });
        }

        private void HandleEntryPriceActions(Action<double> buyAction, Action<double> sellAction)
        {
            var model = _posController.GetPosition(_instrument);
            var entryPrice = model.AvgEntryPrice;

            if (model.Side == "Buy")
                buyAction(entryPrice);
            else
                sellAction(entryPrice);
        }

        private double? GetAlignedStopLoss(double price)
        {
            var stopLossPcnt = StopLossPercent;
            if (!stopLossPcnt.HasValue) return null;

            var value = price * stopLossPcnt.Value / 100;
            return _instrumentObj.RoundPrice(value);
        }

        private void SetPositionSize(double bidPrice, double askPrice)
        {
            double balance = 0, risk = 0;
            if (tbBalance.Text.Length > 0)
            {
                if (!Double.TryParse(tbBalance.Text, out balance)) return;
            }
            if (tbRiskPcnt.Text.Length > 0)
            {
                if (!Double.TryParse(tbRiskPcnt.Text, out risk)) return;
            }
            if (balance > 0 && risk > 0)
            {
                var stopLoss = GetAlignedStopLoss(bidPrice);
                if (!stopLoss.HasValue) return;

                var stopValue = StopSlip + stopLoss.Value;
                PositionSize  = _posController.CalculatePosition(bidPrice, stopValue, risk, balance, MainWnd.Controller, _instrument);

                lblBidPrice.ToolTip = MainWnd.Controller.ToStringPrice(bidPrice - stopValue, _instrument);
                lblAskPrice.ToolTip = MainWnd.Controller.ToStringPrice(askPrice + stopValue, _instrument);
            }
            else
                PositionSize = 0;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (MainWnd != null)
            {
                var ctrl = MainWnd.Controller;
                SetPositionSize(ctrl.GetBidPrice(_instrument), ctrl.GetAskPrice(_instrument));
            }
        }

        private void CreateMarketOrder(Button btn, string side)
        {
            if (!_posController.PositionOpened(_instrument)) { Error("This action shouldn't create a position."); return; }
            if (String.IsNullOrEmpty(tbPositionSize.Text)) { Error("Empty position size."); return; }

            var    size = Convert.ToInt32(tbPositionSize.Text);
            string text = null;

            if (!_posController.PositionOpened(_instrument))
                text = "OPN";

            MainWnd.HandleActionWithMetrics(btn, () => MainWnd.Controller.CreateMarketOrder(_instrument, side, size, text), "NEW MARKET");
        }

        private double ToPrice(string price)
        {
            return Convert.ToDouble(price) / _priceMultiplier;
        }

        private void CreateLimitOrder(Button btn, string side)
        {
            if (String.IsNullOrEmpty(tbPositionSize.Text)) { Warning("Empty position size."); return; }
            if (String.IsNullOrEmpty(tbLimitPrice.Text)) { Warning("Empty limit price."); return; }

            var    limitPrice = ToPrice(tbLimitPrice.Text);
            var    size       = Convert.ToInt32(tbPositionSize.Text);
            string text       = null;

            if (!_posController.PositionOpened(_instrument))
                text = "OPN";

            MainWnd.HandleActionWithMetrics(btn, () => MainWnd.Controller.CreateLimitOrder(_instrument, side, size, limitPrice, false, text), "NEW LIMIT");
        }

        private void CreateStopLimitOrder(Button btn, string side)
        {
            double stopPx = 0;

            if (String.IsNullOrEmpty(tbPositionSize.Text)) { Warning("Empty position size."); return; }
            if (String.IsNullOrEmpty(tbStopLimitPrice.Text)) { Warning("Empty stop limit price."); return; }
            if (String.IsNullOrEmpty(tbStopPriceDelta.Text)) { Warning("Empty trigger value"); return; }

            var    triggerDelta = ToPrice(tbStopPriceDelta.Text);
            var    price        = ToPrice(tbStopLimitPrice.Text);
            var    size         = Convert.ToInt32(tbPositionSize.Text);
            var    ctrl         = MainWnd.Controller;
            string text         = null;

            if (side == "Buy")
            {
                if (ctrl.GetBidPrice(_instrument) > price) { Warning("Stop price is less than market for the buy."); return; }
                stopPx = price - triggerDelta;
            }
            else
            {
                if (price > ctrl.GetAskPrice(_instrument)) { Warning("Stop price is more than market for the sell."); return; }
                stopPx = price + triggerDelta;
            }
            if (!_posController.PositionOpened(_instrument))
            {
                text = "OPN";
            }
            MainWnd.HandleActionWithMetrics(btn, () => ctrl.CreateStopLimitOrder(_instrument, side, size, price, stopPx, false, text), "NEW STOP");
        }

        private void CreateWatchingOrder(Button btn, string side)
        {
            if (PositionSize == 0) { Warning("Empty position size"); return; }

            var stopLoss  = GetAlignedStopLoss(Convert.ToDouble(lblBidPrice.Content));// stopLossPcnt.Value * Convert.ToDouble(lblBidPrice.Content) / 100;
            if (!stopLoss.HasValue) return;

            var priceSlip = ConvertValueFromString(tbPriceSlip.Text) * _instrumentObj.TickSize;
            var stopSlip  = StopSlip;

            MainWnd.HandleActionWithMetrics(btn, () => _posWatcher.BeginOpenPosition(_instrument, side, PositionSize, stopLoss.Value, priceSlip, stopSlip), "NEW ORD");
        }

        public void SetStopPrice(BitmexOrderData order)
        {
            if (String.Compare(order.Text, "STOPLS") == 0)
            {
                _stopPx = order.StopPx.Value;
            }
            else if (!String.IsNullOrEmpty(order.OrderId))
            {
                if (order.StopPx.HasValue)
                    _stopPx = order.StopPx;
            }
        }

        internal void ShowPosition(PositionControllerEventArgs e)
        {
            gbPosition.DataContext = null;
            gbPosition.DataContext = e.Model;

            if (_posController.PositionOpened(_instrument))
            {
                gbPosition.Visibility = Visibility.Visible;
                lblProfitXbt.ToolTip = e.Model.TradeProfitXbt;

                var color = e.Model.Side == "Sell" ? Brushes.LightCoral : (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF56B372"));
                lblPosSize.Foreground = lblPosSide.Foreground = lblEntryPrice.Foreground = color;
            }
            else
                gbPosition.Visibility = Visibility.Hidden;
        }

        private void OnPriceChangedWhenPositionOpened(object sender, ExchangePricesEventArgs args)
        {
            if (!_stopPx.HasValue)
                return;

            double delta1 = 0, delta2 = 0;
            var ctrl = MainWnd.Controller;

            HandleEntryPriceActions(entryPrice1 =>
            {
                var bid = ctrl.GetBidPrice(args.Symbol);
                if (bid > 0)
                {
                    delta1 = entryPrice1 - _stopPx.Value;
                    delta2 = bid - entryPrice1;
                }
            },
            entryPrice2 =>
            {
                var ask = ctrl.GetAskPrice(args.Symbol);
                if (ask > 0)
                {
                    delta1 = _stopPx.Value - entryPrice2;
                    delta2 = entryPrice2 - ask;
                }
            });
            ChangeControl(() =>
            {
                var ratio = delta2 / delta1;

                lblPriceStop.Content = ratio.ToString("0.00");
                btnZeroStop.IsEnabled = ratio > 1;   // more than 1 stop
                lblPriceStop.Foreground = ratio > 1 ? Brushes.Black : Brushes.LightCoral;
            });
        }

        #region Event handlers
        private void OnBuyOrderClick(object sender, RoutedEventArgs e)
        {
            if (rbOrdTypeLimit.IsChecked.Value)
            {
                CreateLimitOrder(btnBuy, "Buy");
            }
            else if (rbOrdTypeMarket.IsChecked.Value)
            {
                CreateMarketOrder(btnBuy, "Buy");
            }
            else if (rbOrdTypeStopLimit.IsChecked.Value)
            {
                CreateStopLimitOrder(btnBuy, "Buy");
            }
            else
                CreateWatchingOrder(sender as Button, "Buy");
        }

        private void OnCancelOrderWatchingClick(object sender, RoutedEventArgs e)
        {
            if (btnCancelWatching.Visibility == Visibility.Visible)
            {
                _posWatcher.StopWatching();
            }
        }

        private void OnSellOrderClick(object sender, RoutedEventArgs e)
        {
            if (rbOrdTypeLimit.IsChecked.Value)
            {
                CreateLimitOrder(btnBuy, "Sell");
            }
            else if (rbOrdTypeMarket.IsChecked.Value)
            {
                CreateMarketOrder(btnBuy, "Sell");
            }
            else if (rbOrdTypeStopLimit.IsChecked.Value)
            {
                CreateStopLimitOrder(btnBuy, "Sell");
            }
            else
                CreateWatchingOrder(sender as Button, "Sell");
        }

        internal void OnPositionChanged(BitmexPositionData pos)
        {
            if (pos.Leverage.HasValue)
                ChangeControl(() => lblLeverage.Content = (int)pos.Leverage.Value + "x");

            if (pos.IsOpen.HasValue)
            {
                ChangeControl(() => gridClosePosition.IsEnabled = pos.IsOpen.Value);

                var posCtrl = _posController;
                var ctrl    = MainWnd.Controller;

                if (pos.IsOpen.Value)
                {
                    ctrl.Exchange.XbtPriceChanged += OnPriceChangedWhenPositionOpened;
                    ctrl.Exchange.EthPriceChanged += OnPriceChangedWhenPositionOpened;
                    ctrl.Exchange.LtcPriceChanged += OnPriceChangedWhenPositionOpened;
                    ctrl.Exchange.EosPriceChanged += OnPriceChangedWhenPositionOpened;
                    ctrl.Exchange.XrpPriceChanged += OnPriceChangedWhenPositionOpened;
                    ctrl.Exchange.AdaPriceChanged += OnPriceChangedWhenPositionOpened;
                    ctrl.Exchange.TrxPriceChanged += OnPriceChangedWhenPositionOpened;
                    ctrl.Exchange.BchPriceChanged += OnPriceChangedWhenPositionOpened;

                    ActualPositionSize = pos.PositionSize.Value;
                    PositionSide       = pos.PositionSide;

                    posCtrl.Add(pos, pos.Symbol, ctrl);

                    ChangeControl(() =>
                    {
                        LogPositionEvent("OPENED " + pos.Symbol);
                        tabPosition.SelectedIndex = 1;
                        cbProfitPcnt1.SelectedIndex = 1;
                    });
                }
                else
                {
                    ctrl.Exchange.XbtPriceChanged -= OnPriceChangedWhenPositionOpened;
                    ctrl.Exchange.EthPriceChanged -= OnPriceChangedWhenPositionOpened;
                    ctrl.Exchange.LtcPriceChanged -= OnPriceChangedWhenPositionOpened;
                    ctrl.Exchange.EosPriceChanged -= OnPriceChangedWhenPositionOpened;
                    ctrl.Exchange.XrpPriceChanged -= OnPriceChangedWhenPositionOpened;
                    ctrl.Exchange.AdaPriceChanged -= OnPriceChangedWhenPositionOpened;
                    ctrl.Exchange.TrxPriceChanged -= OnPriceChangedWhenPositionOpened;
                    ctrl.Exchange.BchPriceChanged -= OnPriceChangedWhenPositionOpened;

                    ChangeControl(() =>
                    {
                        lblPriceStop.Content = "0.00";
                        tbTrailValue.Text    = tbProfit1.Text =
                        tbProfit2.Text       = tbProfit3.Text =
                        tbPositionSize.Text  = String.Empty;
                        cbProfitPcnt1.SelectedIndex =
                        cbProfitPcnt2.SelectedIndex =
                        cbProfitPcnt3.SelectedIndex = 0;
                        tabPosition.SelectedIndex   = 0;
                        rbOrdTypeAuto.IsChecked     = true;

                        OpenPositionFee = null;

                        if (_instrument != null && posCtrl.PositionOpened(_instrument))
                        {
                            posCtrl.Remove(_instrument);
                            LogPositionEvent("CLOSED " + pos.Symbol);
                        }
                    });
                }
            }
            else
            {
                long? newSize = null;

                UpdateModel(pos.Symbol, model =>
                {
                    var posModel = PositionModel.ToModel(pos, MainWnd.Controller);
                    newSize = model.Update(posModel, msg =>
                    {
                        ChangeControl(() =>
                        {
                            pos.Message = "Update " + msg;
                            LogPositionEvent(pos.Message);
                        });
                    },
                    () => { });
                });

                if (newSize.HasValue)
                {
                    ActualPositionSize = newSize.Value;
                    PositionSizeChanged(this, new PosWatcherEventArgs(ActualPositionSize));
                }
            }
        }

        private void OnZeroStopClick(object sender, RoutedEventArgs e)
        {
            double? priceDelta = null, entryPrice = null;

            HandleEntryPriceActions(e1 => entryPrice = e1, e2 => entryPrice = e2);

            priceDelta = entryPrice - _stopPx.Value;
            priceDelta += MainWnd.Controller.GetZeroProfitPriceStep(ActualPositionSize, entryPrice.Value, _instrument);

            _posWatcher.PositionPriceChanged(priceDelta.Value);
        }

        private void OnCreateTakeProfitClick(object sender, RoutedEventArgs e)
        {
            double takePcnt1 = 0, takePrice1 = 0, takePcnt2 = 0, takePrice2 = 0, takePcnt3 = 0, takePrice3 = 0;
            long orderQty1 = 0, orderQty2 = 0, orderQty3 = 0;
            string ordSide = lblPosSide.Content.ToString();

            var p1 = cbProfitPcnt1.SelectedValue;
            var p2 = cbProfitPcnt2.SelectedValue;
            var p3 = cbProfitPcnt3.SelectedValue;

            if (String.IsNullOrEmpty(tbProfit1.Text)) { Warning("Empty take profit1 price"); return; }
            takePrice1 = Convert.ToDouble(tbProfit1.Text);

            if (p1 == null || String.IsNullOrEmpty(p1.ToString())) { Warning("Empty take profit1 %"); return; }
            takePcnt1 = Convert.ToDouble(p1.ToString());

            if (!String.IsNullOrEmpty(tbProfit2.Text))
            {
                takePrice2 = Convert.ToDouble(tbProfit2.Text);
                if (p2 != null && !String.IsNullOrEmpty(p2.ToString()))
                    takePcnt2 = Convert.ToDouble(p2.ToString());
            }
            if (!String.IsNullOrEmpty(tbProfit3.Text))
            {
                takePrice3 = Convert.ToDouble(tbProfit3.Text);
                if (p3 != null && !String.IsNullOrEmpty(p3.ToString()))
                    takePcnt3 = Convert.ToDouble(p3.ToString());
            }
            if (takePcnt1 + takePcnt2 + takePcnt3 > 100)
            {
                Error("Invalid percent summary");
                return;
            }
            orderQty1 = (long)(ActualPositionSize * takePcnt1 / 100 + 0.5);

            if (takePrice1 != 0 && takePrice2 == 0 && takePrice3 == 0)
            {
                MainWnd.Controller.CreateTakeProfit(_instrument, ordSide, new Tuple<double, long>(takePrice1, orderQty1));
            }
            else if (takePrice1 != 0 && takePrice2 != 0 && takePrice3 == 0)
            {
                if (takePcnt2 == 0)
                    orderQty2 = ActualPositionSize - orderQty1;
                else
                    orderQty2 = CalcPositionSize(takePcnt2);

                if (orderQty2 == 0)
                {
                    Error("Invalid percent for profit2");
                    return;
                }
                MainWnd.Controller.CreateTakeProfit(_instrument,
                    ordSide,
                    new Tuple<double, long>(takePrice1, orderQty1),
                    new Tuple<double, long>(takePrice2, orderQty2)
                    );
            }
            else if (takePrice1 != 0 && takePrice2 != 0 && takePrice3 != 0)
            {
                if (takePcnt2 == 0 && takePcnt3 == 0)
                {
                    double pcnt2 = (long)((100 - takePcnt1) / 2 + 0.5);
                    double pcnt3 = 100 - takePcnt1 - pcnt2;
                    orderQty2 = CalcPositionSize(pcnt2);
                    orderQty3 = CalcPositionSize(pcnt3);
                }
                else if (takePcnt2 != 0 && takePcnt3 == 0)
                {
                    orderQty2 = CalcPositionSize(takePcnt2);
                    orderQty3 = ActualPositionSize - orderQty1 - orderQty2;

                    if (orderQty3 == 0) { Error("Invalid percent for profit3"); return; }
                }
                else if (takePcnt2 != 0 && takePcnt3 != 0)
                {
                    if (takePcnt1 + takePcnt2 + takePcnt3 != 100)
                    {
                        Error("Percent summary is not 100%");
                        return;
                    }
                    else
                    {
                        orderQty2 = CalcPositionSize(takePcnt2);
                        orderQty3 = CalcPositionSize(takePcnt3);
                    }
                }
                else
                {
                    Error("Empty take profit2 %");
                    return;
                }
                MainWnd.Controller.CreateTakeProfit(_instrument,
                            ordSide,
                            new Tuple<double, long>(takePrice1, orderQty1),
                            new Tuple<double, long>(takePrice2, orderQty2),
                            new Tuple<double, long>(takePrice3, orderQty3)
                            );
            }
            else
                Error("Invalid use case");
        }

        private void OnCloseLimitPositionClick(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(tbCloseSlip.Text)) { Warning("Slippage value is undefinfed."); return; }

            var pos       = _posController.GetPosition(_instrument);
            var side      = pos.Side == "Buy" ? "Sell" : "Buy";
            var qty       = Convert.ToInt64(pos.Size);
            var closeSlip = Convert.ToDouble(tbCloseSlip.Text) * _instrumentObj.TickSize;

            tbCloseSlip.Background = Brushes.White;
            
            MainWnd.HandleActionWithMetrics(btnCloseLimit, () => _posWatcher.BeginClosePosition(_instrument, side, qty, closeSlip), "NEW ORD");
        }

        private void OnCloseMarketPositionClick(object sender, RoutedEventArgs e)
        {
            var pos  = _posController.GetPosition(_instrument);
            var qty  = Convert.ToInt64(pos.Size);
            var side = pos.Side == "Buy" ? "Sell" : "Buy";

            tbCloseSlip.Background = Brushes.White;
            
            MainWnd.HandleActionWithMetrics(btnCloseMarket, () => MainWnd.Controller.CreateCloseMarketOrder(_instrument, side, qty), "NEW ORD");
        }

        private void OnOrderTypeChecked(object sender, RoutedEventArgs e)
        {
            Action<bool> stopLimitPrice = state => tbStopLimitPrice.IsEnabled = tbStopPriceDelta.IsEnabled = tbPositionSize.IsEnabled = state;
            Action<bool> limitPrice     = state => tbLimitPrice.IsEnabled = tbPositionSize.IsEnabled = state;

            tbLimitPrice.Text = tbStopLimitPrice.Text = tbStopPriceDelta.Text = tbPositionSize.Text = String.Empty;

            if (rbOrdTypeAuto.IsChecked.Value)
            {
                stopLimitPrice(false);
                limitPrice(false);
            }
            else if (rbOrdTypeLimit.IsChecked.Value)
            {
                stopLimitPrice(false);
                limitPrice(true);
            }
            else if (rbOrdTypeStopLimit.IsChecked.Value)
            {
                limitPrice(false);
                stopLimitPrice(true);
            }
            else if (rbOrdTypeMarket.IsChecked.Value)
            {
                stopLimitPrice(false);
                limitPrice(true);
                tbLimitPrice.IsEnabled = false;
            }
        }

        private void OnCreateTrailClick(object sender, RoutedEventArgs e)
        {
            string side = "Buy";
            if (String.IsNullOrEmpty(tbTrailValue.Text)) { Warning("Empty trailing stop value"); return; }

            var trailValue = Convert.ToDouble(tbTrailValue.Text);
            var exchTrail  = cbExchTrail.IsChecked.Value;
            var qty        = CalcPositionSize(100);

            MainWnd.HandleActionWithMetrics((Button)sender, () =>
            {
                if (exchTrail)
                {
                    if (PositionSide == "Buy")
                    {
                        trailValue *= -1;
                        side = "Sell";
                    }
                    var ordData = MainWnd.Controller.Client.CreateTrailingStopOrder(_instrument, side, qty, trailValue);
                    _trailingStopId = ordData.OrderId;
                }
                else
                    _posWatcher.PositionPriceChanged(trailValue);
            },
            "TRAIL");

            if (exchTrail)
                btnCancelTrail.IsEnabled = true;
        }

        private void OnCancelTrailClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(_trailingStopId));

            MainWnd.CancelOrder(_trailingStopId);
            _trailingStopId = null;
        }

        public void OnOrderBookChanged(object sender, ExchangePricesEventArgs args, Action<double, double> action)
        {
            OnOrderBookChanged(args, (bid, ask) =>
            {
                var ctrl = MainWnd.Controller;

                lblBidPrice.Content = ctrl.ToStringPrice(bid, args.Symbol);
                lblAskPrice.Content = ctrl.ToStringPrice(ask, args.Symbol);

                action(bid, ask);
            });
        }

        private void OnStopLossTextChanged(object sender, TextChangedEventArgs e)
        {
            if (rbOrdTypeAuto != null)
                rbOrdTypeAuto.IsChecked = true;
            OnTextChanged(sender, e);
        }

        internal void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
#if FULLSCREENMODE
            if (e.Key == Key.F11)
            {
                if (WindowStyle == WindowStyle.None)
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Normal;
                    ResizeMode = ResizeMode.CanResize;
                }
                else
                {
                    WindowStyle = WindowStyle.None;
                    ResizeMode = ResizeMode.NoResize;
                    WindowState = WindowState.Maximized;
                }
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                return;
            }
#endif
            switch (tabPosition.SelectedIndex)
            {
            case 0:
                {
                    switch (e.Key)
                    {
                    case Key.F1: OnBuyOrderClick(null, null); break;
                    case Key.F4: OnSellOrderClick(null, null); break;
                    case Key.Escape:
                        {
                            if (btnCancelWatching.Visibility == Visibility.Visible)
                                OnCancelOrderWatchingClick(null, null);
                            break;
                        }
                    }
                }
                break;
            case 1:
                {
#if ALTKEYMODE
                    if (_altKeyPressed)
                    {
                        _altKeyPressed = false;
                        if (e.SystemKey == Key.Q)
                        {
                            OnCloseMarketPositionClick(null, null);
                        }
                        else if (e.SystemKey == Key.X)
                        {
                            OnCloseLimitPositionClick(null, null);
                        }
                    }
                    if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
                    {
                        _altKeyPressed = true;
                    }
#else
                    if (gridClosePosition.IsEnabled)
                    {
                        switch (e.Key)
                        {
                        case Key.F1: OnCreateTakeProfitClick(null, null); break;
                        case Key.F12: OnCloseMarketPositionClick(null, null); break;
                        }
                    }
#endif
                }
                break;
            }
        }
#if ALTKEYMODE
        bool _altKeyPressed;
#endif
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            MainWnd.OnKeyDown(sender, e);
        }
        #endregion
    }
}
