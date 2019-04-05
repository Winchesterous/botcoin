using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Timers;
using WebSocketDotNet;

namespace BotCoin.Exchange
{
    [Exchange(Name = "OkEx", IsWebsocket = true)]
    public class OkExExchange : BaseWebSocketExchange, IExchangeEvents, IExchangeAltcoinEvents, IAnalyticsEvents
    {
        EventHandler<OrderBookEventArgs> _btcOrderBookHandler;
        EventHandler<OrderBookEventArgs> _bchOrderBookHandler;
        EventHandler<OrderBookEventArgs> _ltcOrderBookHandler;
        EventHandler<OrderBookEventArgs> _ethOrderBookHandler;
        EventHandler<OrderBookEventArgs> _iotaOrderBookHandler;
        EventHandler<TradeEventArgs> _btcTradeHandler;
        EventHandler<TradeEventArgs> _bchTradeHandler;
        EventHandler<TradeEventArgs> _ltcTradeHandler;
        EventHandler<TradeEventArgs> _ethTradeHandler;
        EventHandler<TradeEventArgs> _xrpTradeHandler;

        readonly string OrderBookChannel;
        readonly string TradeChannel;
        readonly string PingRequest;
        readonly Timer _timer;

        public event EventHandler<ExchangePricesEventArgs> InstrumentReceived;
        public event EventHandler<TradeEventArgs> TradeReceived;

        public OkExExchange(ExchangeSettingsData setting) : base(setting)
        {
            _restExchange = new OkExRestExchange(setting);
            this.InstrumentReceived += (s, e) => { };
            this.TradeReceived += (s, e) => { };

            OrderBookChannel = "'channel':'ok_sub_spot_{0}_{1}_depth_20'";
            TradeChannel = "'channel':'ok_sub_spot_{0}_{1}_deals'";
            PingRequest = "{'event':'ping'}";
            _timer = new Timer(29000);
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            SendMessage(c => c.Send(PingRequest));
        }
        
        public void SubscribeOrderBook(CurrencyName instrument1, EventHandler<OrderBookEventArgs> handler = null, CurrencyName instrument2 = CurrencyName.USDT)
        {
            string channelName = null;

            if (handler != null)
            {
                if (instrument1 == CurrencyName.BTC)
                    _btcOrderBookHandler += handler;
                else if (instrument1 == CurrencyName.BCH)
                    _bchOrderBookHandler += handler;
                else if (instrument1 == CurrencyName.LTC)
                    _ltcOrderBookHandler += handler;
                else if (instrument1 == CurrencyName.ETH)
                    _ethOrderBookHandler += handler;
                else if (instrument1 == CurrencyName.IOTA)
                    _iotaOrderBookHandler += handler;
            }

            channelName = String.Format(OrderBookChannel, instrument1.ToString(), instrument2.ToString()).ToLower();
            SendMessage(c => c.Send("{'event':'addChannel'," + channelName + "}"));
        }

        public void SubscribeTrade(CurrencyName instrument1, EventHandler<TradeEventArgs> handler = null, CurrencyName instrument2 = CurrencyName.USDT)
        {
            if (handler != null)
            {
                if (instrument1 == CurrencyName.BTC)
                    _btcTradeHandler += handler;
                if (instrument1 == CurrencyName.BCH)
                    _bchTradeHandler += handler;
                if (instrument1 == CurrencyName.LTC)
                    _ltcTradeHandler += handler;
                if (instrument1 == CurrencyName.ETH)
                    _ethTradeHandler += handler;
                if (instrument1 == CurrencyName.XRP)
                    _xrpTradeHandler += handler;
            }

            var channelName = String.Format(TradeChannel, instrument1.ToString().ToLower(), instrument2.ToString().ToLower());
            SendMessage(c => c.Send("{'event':'addChannel'," + channelName + "}"));
        }

        public void UnsubscribeOrderBook(CurrencyName instrument1, EventHandler<OrderBookEventArgs> handler, CurrencyName instrument2 = CurrencyName.USDT)
        {
            if (instrument1 == CurrencyName.BTC) _btcOrderBookHandler -= handler;
            if (instrument1 == CurrencyName.BCH) _bchOrderBookHandler -= handler;
            if (instrument1 == CurrencyName.LTC) _ltcOrderBookHandler -= handler;
            if (instrument1 == CurrencyName.ETH) _ethOrderBookHandler -= handler;
            if (instrument1 == CurrencyName.IOTA) _iotaOrderBookHandler -= handler;

            var channelName = String.Format(OrderBookChannel, instrument1.ToString().ToLower(), instrument2.ToString().ToLower());
            SendMessage(c => c.Send("{'event':'removeChannel'," + channelName + "}"));
        }

        private void UnsubscribeTrade(CurrencyName instrument1, EventHandler<TradeEventArgs> handler, CurrencyName instrument2 = CurrencyName.USDT)
        {
            if (instrument1 == CurrencyName.BTC) _btcTradeHandler -= handler;
            if (instrument1 == CurrencyName.BCH) _bchTradeHandler -= handler;
            if (instrument1 == CurrencyName.LTC) _ltcTradeHandler -= handler;
            if (instrument1 == CurrencyName.ETH) _ethTradeHandler -= handler;
            if (instrument1 == CurrencyName.XRP) _xrpTradeHandler -= handler;

            var channelName = String.Format(TradeChannel, instrument1.ToString().ToLower(), instrument2.ToString().ToLower());
            SendMessage(c => c.Send("{'event':'removeChannel'," + channelName + "}"));
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.OkEx;
        }

        public override UserAccount GetBalances()
        {
            return _restExchange.GetBalances();
        }

        public override bool CancelOrder(string orderId, string symbol = null)
        {
            return _restExchange.CancelOrder(orderId);
        }

        public override OrderResponse PlaceOrder(double price, double amount, CurrencyName currency, OrderSide side, TradeOrderType type = TradeOrderType.Limit)
        {
            return _restExchange.PlaceOrder(price, amount, currency, side, type);
        }

        public override void Logon()
        {
            base.Logon();

            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }

        public override void Logout()
        {
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Stop();

            base.Logout();
        }

        protected override void OnMessage(object sender, MessageEventArgs e)
        {
            if (!e.IsText)
            {
                var s = System.Text.Encoding.ASCII.GetString(e.RawData);
                return;
            }
            if (e.Data.Contains("pong"))
                return;

            var json = e.Data.Substring(1, e.Data.Length - 2);            
            if (json.Contains("_deals"))
            {
                if (json.Contains("addChannel"))
                    return;

                var trade = JsonConvert.DeserializeObject<OkExTrade>(json);
                trade.ParseData();

                foreach (var item in trade.Items)
                    TradeReceived(this, new TradeEventArgs(item));

                return;
            }

            if (!json.Contains("_depth_20"))
                return;

            var response = JsonConvert.DeserializeObject<OkExOrderBook>(json);
            if (response.Data.ErrorCode != 0)
            {
                var msg = String.Format("[{0}] error. {1} '{2}'", GetExchangeName(), response.Data.ErrorCode, response.Data.ErrorMessage);
                Log.WriteError(msg);
                return;
            }

            response.SortAsks();
            if (response.Channel.Contains("_depth_"))
            {
                InstrumentReceived(this, new ExchangePricesEventArgs(response, response.Channel));
            }
            if (response.Channel.StartsWith("ok_sub_spot_btc") && _btcOrderBookHandler != null)
            {
                _btcOrderBookHandler(this, new OrderBookEventArgs(response));
                return;
            }
            if (response.Channel.StartsWith("ok_sub_spot_bch") && _bchOrderBookHandler != null)
            {
                _bchOrderBookHandler(this, new OrderBookEventArgs(response));
                return;
            }
            if (response.Channel.StartsWith("ok_sub_spot_ltc") && _ltcOrderBookHandler != null)
            {
                _ltcOrderBookHandler(this, new OrderBookEventArgs(response));
                return;
            }
            if (response.Channel.StartsWith("ok_sub_spot_eth") && _ethOrderBookHandler != null)
            {
                _ethOrderBookHandler(this, new OrderBookEventArgs(response));
                return;
            }
            if (response.Channel.StartsWith("ok_sub_spot_iota") && _iotaOrderBookHandler != null)
            {
                var instrument = CurrencyName.Undefined;
                if (response.Channel.Contains("_usdt_"))
                {
                    instrument = CurrencyName.USDT;
                }
                else if (response.Channel.Contains("_btc_"))
                {
                    instrument = CurrencyName.BTC;
                }
                else
                    throw new InvalidOperationException("[OkEx] IOTA response.");

                _iotaOrderBookHandler(this, new OrderBookEventArgs(response, instrument));
            }
        }

        protected override void OnOpen(object sender, EventArgs e)
        {
            base.OnOpen(sender, e);
            OnTimerElapsed(sender, null);
        }

        public event EventHandler<OrderBookEventArgs> OnBtcOrderBook
        {
            add { SubscribeOrderBook(CurrencyName.BTC, value); }
            remove { UnsubscribeOrderBook(CurrencyName.BTC, value); }
        }

        public event EventHandler<OrderBookEventArgs> OnBchOrderBook
        {
            add { SubscribeOrderBook(CurrencyName.BCH, value); }
            remove { UnsubscribeOrderBook(CurrencyName.BCH, value); }
        }

        public event EventHandler<OrderBookEventArgs> OnLtcOrderBook
        {
            add { SubscribeOrderBook(CurrencyName.LTC, value); }
            remove { UnsubscribeOrderBook(CurrencyName.LTC, value); }
        }

        public event EventHandler<OrderBookEventArgs> OnEthOrderBook
        {
            add { SubscribeOrderBook(CurrencyName.ETH, value); }
            remove { UnsubscribeOrderBook(CurrencyName.ETH, value); }
        }

        public event EventHandler<OrderBookEventArgs> OnIotaOrderBook
        {
            add { SubscribeOrderBook(CurrencyName.IOTA, value); }
            remove { UnsubscribeOrderBook(CurrencyName.IOTA, value); }
        }

        public event EventHandler<TradeEventArgs> OnBtcTrade
        {
            add { SubscribeTrade(CurrencyName.BTC, value); }
            remove { UnsubscribeTrade(CurrencyName.BTC, value); }
        }

        public event EventHandler<TradeEventArgs> OnBchTrade
        {
            add { SubscribeTrade(CurrencyName.BCH, value); }
            remove { UnsubscribeTrade(CurrencyName.BCH, value); }
        }

        public event EventHandler<TradeEventArgs> OnLtcTrade
        {
            add { SubscribeTrade(CurrencyName.LTC, value); }
            remove { UnsubscribeTrade(CurrencyName.LTC, value); }
        }

        public event EventHandler<TradeEventArgs> OnEthTrade
        {
            add { SubscribeTrade(CurrencyName.ETH, value); }
            remove { UnsubscribeTrade(CurrencyName.ETH, value); }
        }
                
        public event EventHandler<OrderBookEventArgs> OnXrpOrderBook
        {
            add { }
            remove { }
        }

        public event EventHandler<OrderBookEventArgs> OnDashOrderBook
        {
            add { }
            remove { }
        }
    }
}
