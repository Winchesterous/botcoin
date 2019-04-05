using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using WebSocketDotNet;

namespace BotCoin.Exchange
{
    [Exchange(Name = "Binance", IsWebsocket = true)]
    public class BinanceExchange : BaseWebSocketExchange, IExchangeEvents, IExchangeAltcoinEvents, IAnalyticsEvents
    {
        EventHandler<OrderBookEventArgs> _btcOrderBookHandler;
        EventHandler<OrderBookEventArgs> _bchOrderBookHandler;
        EventHandler<OrderBookEventArgs> _ethOrderBookHandler;
        EventHandler<OrderBookEventArgs> _ltcOrderBookHandler;
        EventHandler<OrderBookEventArgs> _xrpOrderBookHandler;
        EventHandler<OrderBookEventArgs> _dashOrderBookHandler;
        EventHandler<OrderBookEventArgs> _iotaOrderBookHandler;
        readonly object _obj;

        public event EventHandler<ExchangePricesEventArgs> InstrumentReceived;
        public event EventHandler<TradeEventArgs> TradeReceived;
        public event EventHandler<TickerEventArgs> TickerReceived;

        public BinanceExchange(ExchangeSettingsData setting) : base(setting)
        {
            _restExchange = new BinanceRestExchange(setting);
            _obj = new object();

            this.InstrumentReceived += (s, e) => { };
            this.TradeReceived      += (s, e) => { };
            this.TickerReceived     += (s, e) => { };
        }

        public BinanceRestExchange RestExchange
        {
            get { return (BinanceRestExchange)_restExchange; }
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Binance;
        }

        public override UserAccount GetBalances()
        {
            return _restExchange.GetBalances();
        }

        public override OrderResponse PlaceOrder(double price, double amount, CurrencyName currency, OrderSide side, TradeOrderType type = TradeOrderType.Limit)
        {
            return _restExchange.PlaceOrder(price, amount, currency, side, type);
        }

        public override bool CancelOrder(string orderId, string symbol = null)
        {
            return _restExchange.CancelOrder(orderId, symbol);
        }

        protected override void OnClose(object sender, CloseEventArgs e)
        {
            lock (_obj)
            {
                if (_btcOrderBookHandler != null)
                    _btcOrderBookHandler(this, new OrderBookEventArgs(new BinanceOrderBook()));
                if (_ltcOrderBookHandler != null)
                    _ltcOrderBookHandler(this, new OrderBookEventArgs(new BinanceOrderBook()));
                if (_ethOrderBookHandler != null)
                    _ethOrderBookHandler(this, new OrderBookEventArgs(new BinanceOrderBook()));
                if (_bchOrderBookHandler != null)
                    _bchOrderBookHandler(this, new OrderBookEventArgs(new BinanceOrderBook()));
                if (_dashOrderBookHandler != null)
                    _dashOrderBookHandler(this, new OrderBookEventArgs(new BinanceOrderBook()));
                if (_xrpOrderBookHandler != null)
                    _xrpOrderBookHandler(this, new OrderBookEventArgs(new BinanceOrderBook()));
                if (_iotaOrderBookHandler != null)
                    _iotaOrderBookHandler(this, new OrderBookEventArgs(new BinanceOrderBook()));
            }
        }
        
        protected override void OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Data.Contains("@ticker"))
            {
                var response = JsonConvert.DeserializeObject<BinanceTicker>(e.Data);
                TickerReceived(this, new TickerEventArgs(response));
            }
            else if (e.Data.Contains("@trade"))
            {
                var response = JsonConvert.DeserializeObject<BinanceOrderBook>(e.Data);
                TradeReceived(this, new TradeEventArgs(response));
            }
            else if (e.Data.Contains("@depth"))
            {
                var response = JsonConvert.DeserializeObject<BinanceOrderBook>(e.Data);
                InstrumentReceived(this, new ExchangePricesEventArgs(response));
            }
            //if (stream[0].StartsWith("btc"))
            //else if (response.Stream.StartsWith("bcc"))
            //...
        }

        public event EventHandler<OrderBookEventArgs> OnBtcOrderBook
        {
            add { lock (_obj) _btcOrderBookHandler += value; }
            remove { lock (_obj) _btcOrderBookHandler -= value; }
        }

        public event EventHandler<OrderBookEventArgs> OnBchOrderBook
        {
            add { lock (_obj) _bchOrderBookHandler += value; }
            remove { lock (_obj) _bchOrderBookHandler -= value; }
        }

        public event EventHandler<OrderBookEventArgs> OnEthOrderBook
        {
            add { lock (_obj) _ethOrderBookHandler += value; }
            remove { lock (_obj) _ethOrderBookHandler -= value; }
        }

        public event EventHandler<OrderBookEventArgs> OnLtcOrderBook
        {
            add { lock (_obj) _ltcOrderBookHandler += value; }
            remove { lock (_obj) _ltcOrderBookHandler -= value; }
        }

        public event EventHandler<OrderBookEventArgs> OnDashOrderBook
        {
            add { lock (_obj) _dashOrderBookHandler += value; }
            remove { lock (_obj) _dashOrderBookHandler -= value; }
        }

        public event EventHandler<OrderBookEventArgs> OnXrpOrderBook
        {
            add { lock (_obj) _xrpOrderBookHandler += value; }
            remove { lock (_obj) _xrpOrderBookHandler -= value; }
        }

        public event EventHandler<OrderBookEventArgs> OnIotaOrderBook
        {
            add { lock (_obj) _iotaOrderBookHandler += value; }
            remove { lock (_obj) _iotaOrderBookHandler -= value; }
        }
    }
}
