using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using PusherClient;
using System;
using System.Timers;

namespace BotCoin.Exchange
{
    [Exchange(Name = "Bitstamp", IsWebsocket = true)]
    public class BitstampExchange : BaseExchange, IExchangeEvents
    {
        EventHandler<OrderBookEventArgs> _btcOrderBookHandler;
        EventHandler<OrderBookEventArgs> _bchOrderBookHandler;
        EventHandler<OrderBookEventArgs> _bchBtcOrderBookHandler;
        EventHandler<OrderBookEventArgs> _ltcOrderBookHandler;
        EventHandler<OrderBookEventArgs> _ltcBtcOrderBookHandler;
        EventHandler<OrderBookEventArgs> _ethOrderBookHandler;
        EventHandler<OrderBookEventArgs> _ethBtcOrderBookHandler;
        EventHandler<OrderBookEventArgs> _xrpOrderBookHandler;
        EventHandler<OrderBookEventArgs> _xrpBtcOrderBookHandler;

        Channel _btcOrdersChannel;
        Channel _bchOrdersChannel;
        Channel _bchBtcOrdersChannel;
        Channel _ltcOrdersChannel;
        Channel _ltcBtcOrdersChannel;
        Channel _ethOrdersChannel;
        Channel _ethBtcOrdersChannel;
        Channel _xrpOrdersChannel;
        Channel _xrpBtcOrdersChannel;
        Channel _btcTradesChannel;
        Channel _bchTradesChannel;
        Channel _bchBtcTradesChannel;
        Channel _ethTradesChannel;
        Channel _ethBtcTradesChannel;
        Channel _ltcTradesChannel;
        Channel _ltcBtcTradesChannel;
        Channel _xrpTradesChannel;
        Channel _xrpBtcTradesChannel;        
        Pusher _pusher;
        string _restUrl;

        public BitstampExchange(ExchangeSettingsData setting) : base(setting)
        {
            InitPusher(setting.PusherKey);

            _restExchange = new BitstampRestExchange(setting);
            TickerTimer = new Timer(60000);
            _restUrl = setting.RestUrl;
        }

        public BitstampRestExchange Exchange
        {
            get { return (BitstampRestExchange)_restExchange; }
        }

        public Timer TickerTimer
        {
            private set;
            get;
        }

        private void InitPusher(string appKey)
        {
            _pusher = new Pusher(appKey);
            _pusher.ConnectionStateChanged += ConnectionStateChanged;
        }

        private void ConnectionStateChanged(object sender, ConnectionState state)
        {
            Log.WriteInfo("[Bitstamp] " + state.ToString().ToLower(), (int)ExchangeName.Bitstamp);

            if (state == ConnectionState.Disconnected)
            {
                var obj = new BitstampOrderBook();
                if (_btcOrderBookHandler != null)
                    _btcOrderBookHandler(this, new OrderBookEventArgs(obj, OrderState.Created));
                if (_bchOrderBookHandler != null)
                    _bchOrderBookHandler(this, new OrderBookEventArgs(obj, OrderState.Created));
                if (_ltcOrderBookHandler != null)
                    _ltcOrderBookHandler(this, new OrderBookEventArgs(obj, OrderState.Created));
                if (_ethOrderBookHandler != null)
                    _ethOrderBookHandler(this, new OrderBookEventArgs(obj, OrderState.Created));
            }
        }

        public override void InitExchange()
        {
            base.InitExchange();
            base.InitBitstampConfiguration(((BitstampRestExchange)_restExchange).Client);
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Bitstamp;
        }

        public override void Logon()
        {
            _pusher.Connect();
            TickerTimer.Start();
        }

        public override void Logout()
        {
            TickerTimer.Stop();
            _pusher.Disconnect();
        }

        private void SubscribeOrder(Channel channel, string eventName, EventHandler<OrderBookEventArgs> handler, OrderState state)
        {
            channel.Bind(eventName, json =>
            {
                var time = DateTime.UtcNow;
                var order = JsonConvert.DeserializeObject<BitstampOrderBook>(json);
                var orderBook = new OrderBookEventArgs(order, state);
                orderBook.CreatedAt = time;

                handler(this, orderBook);
            });
        }

        private void SubscribeTrade(Channel channel, string eventName, EventHandler<TradeEventArgs> handler)
        {
            channel.Bind(eventName, json =>
            {
                var trade = JsonConvert.DeserializeObject<BitstampTrade>(json);
                handler(this, new TradeEventArgs(trade, channel.Name));
            });
        }

        public override UserAccount GetBalances()
        {
            return _restExchange.GetBalances();
        }

        public override OrderResponse[] GetActiveOrders(CurrencyName currency = CurrencyName.Undefined)
        {
            return _restExchange.GetActiveOrders(currency);
        }

        public override bool CancelOrder(string orderId, string symbol = null)
        {
            return _restExchange.CancelOrder(orderId, symbol);
        }

        public override OrderResponse PlaceOrder(double price, double amount, CurrencyName currency, OrderSide side, TradeOrderType type = TradeOrderType.Limit)
        {
            return _restExchange.PlaceOrder(price, amount, currency, side, type);
        }

        #region Events
        public event EventHandler<OrderBookEventArgs> OnBtcOrderBook
        {
            add
            {
                _btcOrdersChannel = _pusher.Subscribe("live_orders");
                _btcOrderBookHandler += value;
                SubscribeOrder(_btcOrdersChannel, "order_created", value, OrderState.Created);
                SubscribeOrder(_btcOrdersChannel, "order_changed", value, OrderState.Created);
                SubscribeOrder(_btcOrdersChannel, "order_deleted", value, OrderState.Deleted);
            }
            remove
            {
                _btcOrderBookHandler -= value;
                _btcOrdersChannel.Unbind("order_created");
                _btcOrdersChannel.Unbind("order_changed");
                _btcOrdersChannel.Unbind("order_deleted");
            }
        }

        public event EventHandler<OrderBookEventArgs> OnBchBtcOrderBook
        {
            add
            {
                _bchBtcOrdersChannel = _pusher.Subscribe("live_orders_bchbtc");
                _bchBtcOrderBookHandler += value;
                SubscribeOrder(_bchBtcOrdersChannel, "order_created", value, OrderState.Created);
                SubscribeOrder(_bchBtcOrdersChannel, "order_changed", value, OrderState.Created);
                SubscribeOrder(_bchBtcOrdersChannel, "order_deleted", value, OrderState.Deleted);
            }
            remove
            {
                _bchBtcOrderBookHandler -= value;
                _bchBtcOrdersChannel.Unbind("order_created");
                _bchBtcOrdersChannel.Unbind("order_changed");
                _bchBtcOrdersChannel.Unbind("order_deleted");
            }
        }

        public event EventHandler<OrderBookEventArgs> OnBchOrderBook
        {
            add
            {
                _bchOrdersChannel = _pusher.Subscribe("live_orders_bchusd");
                _bchOrderBookHandler += value;
                SubscribeOrder(_bchOrdersChannel, "order_created", value, OrderState.Created);
                SubscribeOrder(_bchOrdersChannel, "order_changed", value, OrderState.Created);
                SubscribeOrder(_bchOrdersChannel, "order_deleted", value, OrderState.Deleted);
            }
            remove
            {
                _bchOrderBookHandler -= value;
                _bchOrdersChannel.Unbind("order_created");
                _bchOrdersChannel.Unbind("order_changed");
                _bchOrdersChannel.Unbind("order_deleted");
            }
        }

        public event EventHandler<OrderBookEventArgs> OnLtcOrderBook
        {
            add
            {
                _ltcOrdersChannel = _pusher.Subscribe("live_orders_ltcusd");
                _ltcOrderBookHandler += value;
                SubscribeOrder(_ltcOrdersChannel, "order_created", value, OrderState.Created);
                SubscribeOrder(_ltcOrdersChannel, "order_changed", value, OrderState.Created);
                SubscribeOrder(_ltcOrdersChannel, "order_deleted", value, OrderState.Deleted);
            }
            remove
            {
                _ltcOrderBookHandler -= value;
                _ltcOrdersChannel.Unbind("order_created");
                _ltcOrdersChannel.Unbind("order_changed");
                _ltcOrdersChannel.Unbind("order_deleted");
            }
        }

        public event EventHandler<OrderBookEventArgs> OnLtcBtcOrderBook
        {
            add
            {
                _ltcBtcOrdersChannel = _pusher.Subscribe("live_orders_ltcbtc");
                _ltcBtcOrderBookHandler += value;
                SubscribeOrder(_ltcBtcOrdersChannel, "order_created", value, OrderState.Created);
                SubscribeOrder(_ltcBtcOrdersChannel, "order_changed", value, OrderState.Created);
                SubscribeOrder(_ltcBtcOrdersChannel, "order_deleted", value, OrderState.Deleted);
            }
            remove
            {
                _ltcBtcOrderBookHandler -= value;
                _ltcBtcOrdersChannel.Unbind("order_created");
                _ltcBtcOrdersChannel.Unbind("order_changed");
                _ltcBtcOrdersChannel.Unbind("order_deleted");
            }
        }

        public event EventHandler<OrderBookEventArgs> OnEthOrderBook
        {
            add
            {
                _ethOrdersChannel = _pusher.Subscribe("live_orders_ethusd");
                _ethOrderBookHandler += value;
                SubscribeOrder(_ethOrdersChannel, "order_created", value, OrderState.Created);
                SubscribeOrder(_ethOrdersChannel, "order_changed", value, OrderState.Created);
                SubscribeOrder(_ethOrdersChannel, "order_deleted", value, OrderState.Deleted);
            }
            remove
            {
                _ethOrderBookHandler -= value;
                _ethOrdersChannel.Unbind("order_created");
                _ethOrdersChannel.Unbind("order_changed");
                _ethOrdersChannel.Unbind("order_deleted");
            }
        }

        public event EventHandler<OrderBookEventArgs> OnEthBtcOrderBook
        {
            add
            {
                _ethBtcOrdersChannel = _pusher.Subscribe("live_orders_ethbtc");
                _ethBtcOrderBookHandler += value;
                SubscribeOrder(_ethBtcOrdersChannel, "order_created", value, OrderState.Created);
                SubscribeOrder(_ethBtcOrdersChannel, "order_changed", value, OrderState.Created);
                SubscribeOrder(_ethBtcOrdersChannel, "order_deleted", value, OrderState.Deleted);
            }
            remove
            {
                _ethBtcOrderBookHandler -= value;
                _ethBtcOrdersChannel.Unbind("order_created");
                _ethBtcOrdersChannel.Unbind("order_changed");
                _ethBtcOrdersChannel.Unbind("order_deleted");
            }
        }

        public event EventHandler<OrderBookEventArgs> OnXrpOrderBook
        {
            add
            {
                _xrpOrdersChannel = _pusher.Subscribe("live_orders_xrpusd");
                _xrpOrderBookHandler += value;
                SubscribeOrder(_xrpOrdersChannel, "order_created", value, OrderState.Created);
                SubscribeOrder(_xrpOrdersChannel, "order_changed", value, OrderState.Created);
                SubscribeOrder(_xrpOrdersChannel, "order_deleted", value, OrderState.Deleted);
            }
            remove
            {
                _xrpOrderBookHandler -= value;
                _xrpOrdersChannel.Unbind("order_created");
                _xrpOrdersChannel.Unbind("order_changed");
                _xrpOrdersChannel.Unbind("order_deleted");
            }
        }

        public event EventHandler<OrderBookEventArgs> OnXrpBtcOrderBook
        {
            add
            {
                _xrpBtcOrdersChannel = _pusher.Subscribe("live_orders_xrpbtc");
                _xrpBtcOrderBookHandler += value;
                SubscribeOrder(_xrpBtcOrdersChannel, "order_created", value, OrderState.Created);
                SubscribeOrder(_xrpBtcOrdersChannel, "order_changed", value, OrderState.Created);
                SubscribeOrder(_xrpBtcOrdersChannel, "order_deleted", value, OrderState.Deleted);
            }
            remove
            {
                _xrpBtcOrderBookHandler -= value;
                _xrpBtcOrdersChannel.Unbind("order_created");
                _xrpBtcOrdersChannel.Unbind("order_changed");
                _xrpBtcOrdersChannel.Unbind("order_deleted");
            }
        }

        public event EventHandler<OrderBookEventArgs> OnDashOrderBook
        {
            add { }
            remove { }
        }

        public event EventHandler<TradeEventArgs> OnBtcTrade
        {
            add
            {
                _btcTradesChannel = _pusher.Subscribe("live_trades");
                SubscribeTrade(_btcTradesChannel, "trade", value);
            }
            remove
            {
                _btcTradesChannel.Unbind("trade");
            }
        }

        public event EventHandler<TradeEventArgs> OnBchTrade
        {
            add
            {
                _bchTradesChannel = _pusher.Subscribe("live_trades_bchusd");
                SubscribeTrade(_bchTradesChannel, "trade", value);
            }
            remove
            {
                _bchTradesChannel.Unbind("trade");
            }
        }

        public event EventHandler<TradeEventArgs> OnBchBtcTrade
        {
            add
            {
                _bchBtcTradesChannel = _pusher.Subscribe("live_trades_bchbtc");
                SubscribeTrade(_bchBtcTradesChannel, "trade", value);
            }
            remove
            {
                _bchBtcTradesChannel.Unbind("trade");
            }
        }

        public event EventHandler<TradeEventArgs> OnEthTrade
        {
            add
            {
                _ethTradesChannel = _pusher.Subscribe("live_trades_ethusd");
                SubscribeTrade(_ethTradesChannel, "trade", value);
            }
            remove
            {
                _ethTradesChannel.Unbind("trade");
            }
        }

        public event EventHandler<TradeEventArgs> OnEthBtcTrade
        {
            add
            {
                _ethBtcTradesChannel = _pusher.Subscribe("live_trades_ethbtc");
                SubscribeTrade(_ethBtcTradesChannel, "trade", value);
            }
            remove
            {
                _ethBtcTradesChannel.Unbind("trade");
            }
        }

        public event EventHandler<TradeEventArgs> OnLtcTrade
        {
            add
            {
                _ltcTradesChannel = _pusher.Subscribe("live_trades_ltcusd");
                SubscribeTrade(_ltcTradesChannel, "trade", value);
            }
            remove
            {
                _ltcTradesChannel.Unbind("trade");
            }
        }

        public event EventHandler<TradeEventArgs> OnLtcBtcTrade
        {
            add
            {
                _ltcBtcTradesChannel = _pusher.Subscribe("live_trades_ltcbtc");
                SubscribeTrade(_ltcBtcTradesChannel, "trade", value);
            }
            remove
            {
                _ltcBtcTradesChannel.Unbind("trade");
            }
        }

        public event EventHandler<TradeEventArgs> OnXrpTrade
        {
            add
            {
                _xrpTradesChannel = _pusher.Subscribe("live_trades_xrpusd");
                SubscribeTrade(_xrpTradesChannel, "trade", value);
            }
            remove
            {
                _xrpTradesChannel.Unbind("trade");
            }
        }

        public event EventHandler<TradeEventArgs> OnXrpBtcTrade
        {
            add
            {
                _xrpBtcTradesChannel = _pusher.Subscribe("live_trades_xrpbtc");
                SubscribeTrade(_xrpBtcTradesChannel, "trade", value);
            }
            remove
            {
                _xrpBtcTradesChannel.Unbind("trade");
            }
        }
        #endregion
    }
}
