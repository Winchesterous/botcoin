using BotCoin.BitmexScalper.Controllers;
using BotCoin.BitmexScalper.Models.State;
using BotCoin.BitmexScalper.Properties;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Database;
using BotCoin.DataType.Exchange;
using BotCoin.Instruments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BotCoin.BitmexScalper.Domain
{
    internal class PositionWatcher
    {
        static InitState InitState;
        public static string RejectEventName = "REJECT";

        internal readonly MainWindowController Controller;
        readonly Action<Exception> showError;
        readonly LtcInstrument _ltc;
        readonly EosInstrument _eos;
        readonly XrpInstrument _xrp;
        readonly XbtInstrument _xbt;
        readonly AdaInstrument _ada;
        readonly TrxInstrument _trx;
        readonly BchInstrument _bch;
        readonly EthInstrument _eth;
        readonly List<string> _orders;
        readonly string _instrument;
        readonly object _obj;
                
        System.Timers.Timer _timer;
        DbPositionState _stopObj;
        IPositionState _state;        
        long _positionSize;

        public event EventHandler<PosWatcherEventArgs> WatcherChanged;
        public event EventHandler<PosWatcherEventArgs> StateChanged;
                
        public PositionWatcher(string instrument, MainWindowController controller, Action<Exception> showError)
        {
            this.WatcherChanged += (s, e) => { };
            this.showError      = showError;
            _instrument         = instrument;

            if (instrument == Settings.Default.LtcContract)      _ltc = new LtcInstrument(instrument);
            else if (instrument == Settings.Default.EosContract) _eos = new EosInstrument(instrument);
            else if (instrument == Settings.Default.XrpContract) _xrp = new XrpInstrument(instrument);
            else if (instrument == Settings.Default.AdaContract) _ada = new AdaInstrument(instrument);
            else if (instrument == Settings.Default.TrxContract) _trx = new TrxInstrument(instrument);
            else if (instrument == Settings.Default.BchContract) _bch = new BchInstrument(instrument);
            else if (instrument == Settings.Default.XbtSwap)     _xbt = new XbtInstrument(instrument);
            else if (instrument == Settings.Default.EthSwap)     _eth = new EthInstrument(instrument);
            else throw new InvalidOperationException();

            InitState  = new InitState(this, instrument);            
            _orders    = new List<string>();
            _obj       = new object();
            Controller = controller;
        }

        internal double? StopPrice { private set; get; }
        internal double? StartWatchingPrice { private set; get; }

        internal void AddOrder(string ordId)
        {
            _orders.Add(ordId);
        }

        internal void StartTimer()
        {
            if (_timer == null)
            {
                _timer = new System.Timers.Timer(300);
                _timer.Elapsed += OnTimerElapsed;
                _timer.Start();
            }
        }

        //private void StopTimer()
        //{
        //    _timer.Stop();
        //    _timer = null;
        //}

        public DbPositionState GetState()
        {
            var state = PositionState;
            if (state == null) return null;

            var args = PositionState.GetArgs();
            args.StateName = state.GetType().ToString();
            args.Instrument = _instrument;

            state.Copy(args, _stopObj);
                        
            return args;
        }

        internal void SendWatcherEvent(Func<string> msg)
        {
            Task.Run(() => WatcherChanged(this, new PosWatcherEventArgs(msg())));
        }

        internal void Init(BitmexUser account)
        {
            var obj = Controller.GetPositionState(account, _instrument);
            if (obj != null)
            {
                if (obj.Opened == 1)
                {
                    var msg = String.Format("The application has been launched on the '{0}'", obj.HostName);
                    throw new InvalidOperationException(msg);
                }
                else
                {
                    var type = Assembly.GetExecutingAssembly().GetType(obj.StateName);
                    PositionState = (IPositionState)Activator.CreateInstance(type, this, obj.Instrument);
                    PositionState.Init(obj);
                    _stopObj = obj;

                    StartWatchingPrice = obj.StartWatchPrice;
                    StopPrice = obj.StopPrice;
                }
            }
            else
                ResetState();
        }

        internal IPositionState PositionState
        {
            set
            {
                Task.Run(() => StateChanged(this, new PosWatcherEventArgs(value.GetStateName())));
                _state = value;
            }
            get
            {
                return _state;
            }
        }

        private void Lock(Action action, Action<string> exAction = null)
        {
            try
            {
                Monitor.Enter(_obj);
                action();
            }
            catch (Exception ex)
            {
                if (exAction != null) exAction(ex.Message);
                Task.Run(() => showError(ex));
            }
            finally
            {
                Monitor.Exit(_obj);
            }
        }

        public void StopWatching()
        {
            Lock(() => ResetState());
        }

        private void ResetState()
        {
            var s = PositionState;

            if (s != null) s.Dispose();
            PositionState = InitState;
        }

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Lock(() =>
            {
                if (PositionState.GetStateName() == "Init")
                {
                    _orders.Clear();
                }
                else if (_orders.Count > 1)
                {
                    var ids = _orders.Take(_orders.Count - 1).ToArray();
                    _orders.RemoveRange(0, _orders.Count - 1);

                    Task.Run(() =>
                    {
                        var res = Controller.CancelOrders(ids);
                        if (res != null)
                            SendWatcherEvent(() => res);
                    });
                }
            });
        }

        internal void OnLtcPriceChanged(object sender, ExchangePricesEventArgs e)
        {
            Lock(() =>
            {
                var bid = _ltc.SetBidPrice(e);
                var ask = _ltc.SetAskPrice(e);
                SetPricesOrReject(bid, ask);
            });
        }

        internal void OnEosPriceChanged(object sender, ExchangePricesEventArgs e)
        {
            Lock(() =>
            {
                var bid = _eos.SetBidPrice(e);
                var ask = _eos.SetAskPrice(e);
                SetPricesOrReject(bid, ask);
            });
        }

        internal void OnXrpPriceChanged(object sender, ExchangePricesEventArgs e)
        {
            Lock(() =>
            {
                var bid = _xrp.SetBidPrice(e);
                var ask = _xrp.SetAskPrice(e);
                SetPricesOrReject(bid, ask);
            });
        }

        internal void OnAdaPriceChanged(object sender, ExchangePricesEventArgs e)
        {
            Lock(() =>
            {
                var bid = _ada.SetBidPrice(e);
                var ask = _ada.SetAskPrice(e);
                SetPricesOrReject(bid, ask);
            });
        }

        internal void OnTrxPriceChanged(object sender, ExchangePricesEventArgs e)
        {
            Lock(() =>
            {
                var bid = _trx.SetBidPrice(e);
                var ask = _trx.SetAskPrice(e);
                SetPricesOrReject(bid, ask);
            });
        }

        internal void OnBchPriceChanged(object sender, ExchangePricesEventArgs e)
        {
            Lock(() =>
            {
                var bid = _bch.SetBidPrice(e);
                var ask = _bch.SetAskPrice(e);
                SetPricesOrReject(bid, ask);
            });
        }

        internal void OnXbtSwapPriceChanged(object sender, ExchangePricesEventArgs e)
        {
            Lock(() =>
            {
                var bid = _xbt.SetBidPrice(e);
                var ask = _xbt.SetAskPrice(e);
                SetPricesOrReject(bid, ask);
            });
        }

        internal void OnEthSwapPriceChanged(object sender, ExchangePricesEventArgs e)
        {
            Lock(() =>
            {
                var bid = _eth.SetBidPrice(e);
                var ask = _eth.SetAskPrice(e);
                SetPricesOrReject(bid, ask);
            });
        }

        private void SetPricesOrReject(double bidPrice, double askPrice)
        {
            var s = PositionState;
            if (s.RejectWatching(bidPrice, askPrice))
            {
                //StopTimer();
                ResetState();
                SendWatcherEvent(() => RejectEventName);
            }
            else
                s.SetPrices(bidPrice, askPrice);
        }

        internal void OnOrderChanged(object sender, ExchangePricesEventArgs e)
        {
            Lock(() =>
            {
                foreach (var ord in e.BtxOrders)
                {
                    if (PositionState.OrderFilled(ord))
                    {
                        ResetState();
                        break;
                    }
                }
            });
        }

        private BitmexPositionData FindPosition(BitmexPositionData[] positions)
        {
            foreach (var pos in positions)
            {
                if (pos.Symbol == _instrument)
                    return pos;
            }
            return null;
        }

        internal void OnPositionChanged(object sender, ExchangePricesEventArgs e)
        {
            var pos = FindPosition(e.BtxPositions);
            if (pos == null) return; 

            if (pos.IsOpen.HasValue)
            {
                if (pos.IsOpen.Value)
                    OnPositionOpened(pos);
                else
                    OnPositionClosed(pos);
            }
        }
                
        private void OnPositionOpened(BitmexPositionData pos)
        {
            if (PositionState.GetStateName() != "Position")
                ChangeStopLoss(pos.Symbol, pos.PositionSide, pos.PositionSize.Value, pos.AvgEntryPrice.Value);
        }
        
        public void OnPositionSizeChanged(long qty)
        {
            if (_positionSize != qty)
            {
                PositionState.ChangeState(_stopObj.StopOrderId, qty, null);
                _positionSize = qty;
            }
        }

        public void PositionPriceChanged(double priceStep)
        {
            PositionState.ChangeState(_stopObj.StopOrderId, _positionSize, priceStep);
        }

        private void OnPositionClosed(BitmexPositionData pos)
        {
            if (!pos.Leverage.HasValue)
            {
                ResetState();
                Controller.CancelAllOrders(pos.Symbol);
            }
            _stopObj = null;
        }

        private void CreateStopOrder(string symbol, string ordSide, double stopPrice)
        {
            var stopState = new StopOrderState(this, symbol, ordSide);
            stopState.CreateOrder(_positionSize, stopPrice);
            _stopObj.StopOrderId = stopState.OrderId;
        }

        private void ChangeStopLoss(string symbol, string ordSide, long qty, double entryPrice)
        {
            _positionSize = Math.Abs(qty);

            var isLongPosition = ordSide == "Buy";
            var stopValue      = _stopObj.StopLoss.Value + _stopObj.StopSlip.Value;

            if (isLongPosition)
            {
                StopPrice = entryPrice - stopValue;
                StartWatchingPrice = StopPrice + _stopObj.StopSlip.Value;
            }
            else
            {
                StopPrice = entryPrice + stopValue;
                StartWatchingPrice = StopPrice - _stopObj.StopSlip.Value;
            }

            CreateStopOrder(symbol, ordSide, StopPrice.Value);
            Lock(() => PositionState = new PositionWatchingState(this, symbol, isLongPosition, StartWatchingPrice.Value, _positionSize, StopPrice.Value));
        }

        public void BeginClosePosition(string symbol, string side, long qty, double closeSlip)
        {
            Lock(() => CreateLimitState(symbol, side, qty, closeSlip, true));
        }

        public void BeginOpenPosition(string symbol, string side, int qty, double stopLoss, double priceSlip, double stopSlip)
        {
            Lock(() =>
            {
                _stopObj = new DbPositionState { StopLoss = stopLoss, StopSlip = stopSlip };

                CreateLimitState(symbol, side, qty, priceSlip);
                StartTimer();
            });
        }

        private ICryptoIntrument GetInstrument(string symbol)
        {
            ICryptoIntrument instrument = null;

            if (symbol == "XBTUSD")             instrument = _xbt;
            else if (symbol == "ETHUSD")        instrument = _eth;
            else if (symbol.StartsWith("LTC"))  instrument = _ltc;
            else throw new InvalidOperationException();

            return instrument;
        }

        private void CreateLimitState(string symbol, string side, long qty, double priceSlip, bool closeOrder = false)
        {
            var instrument = GetInstrument(symbol);
            double? price = null;

            if (side == "Buy")
            {
                price = instrument.GetBidPrice();
                if (priceSlip == 0)
                    PositionState = new BuyMarketState(this, symbol);
                else
                    PositionState = (LimitState)new BuyLimitState(this, symbol, price.Value + priceSlip, closeOrder);

                PositionState.CreateOrder(qty, price.Value);
            }
            else
            {
                price = instrument.GetAskPrice();
                if (priceSlip == 0)
                    PositionState = new SellMarketState(this, symbol);
                else
                    PositionState = (LimitState)new SellLimitState(this, symbol, price.Value - priceSlip, closeOrder);
                
                PositionState.CreateOrder(qty, price.Value);
            }
        }                
    }
}
