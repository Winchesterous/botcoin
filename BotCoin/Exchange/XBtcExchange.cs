using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;
using WebSocketDotNet;

namespace BotCoin.Exchange
{
    [Exchange(Name = "XBtce", IsWebsocket = true)]
    public class XBtcExchange : BaseWebSocketExchange, IExchangeEvents
    {
        EventHandler<OrderBookEventArgs> _btcOrderBookHandler;
        EventHandler<OrderBookEventArgs> _ltcOrderBookHandler;
        EventHandler<OrderBookEventArgs> _bchOrderBookHandler;
        EventHandler<OrderBookEventArgs> _ethOrderBookHandler;
        EventHandler<OrderBookEventArgs> _dashOrderBookHandler;
        readonly string PublicKey;
        readonly string ClientId;
        readonly byte[] PrivateKey;

        public XBtcExchange(ExchangeSettingsData setting) : base(setting)
        {
            ClientId   = setting.ClientId;
            PublicKey  = setting.PublicKey;
            PrivateKey = Encoding.ASCII.GetBytes(setting.SecretKey);

            //_restExchange = new CexRestExchange(setting);
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.XBtce;
        }

        public override UserAccount GetBalances()
        {
            return _restExchange.GetBalances();
        }

        protected override void OnOpen(object sender, EventArgs e)
        {
            var request = new XBtcLoginRequest
            {
                Id = Guid.NewGuid().ToString(),
                Request = "Login",
                Params = new XBtcLoginRequestArgs
                {
                    AuthType = "HMAC",
                    DeviceId = "WebBrowser",
                    WebApiId = ClientId,
                    WebApiKey = PublicKey,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    AppSessionId = DateTime.Now.Ticks.ToString()
                }
            };
            using (var hmac = new HMACSHA256(PrivateKey))
            {
                var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(request.Params.Timestamp + request.Params.WebApiId + request.Params.WebApiKey));
                request.Params.Signature = Convert.ToBase64String(hash);
            }
            SendMessage(c => c.Send(JsonConvert.SerializeObject(request)));
        }

        protected override void OnMessage(object sender, MessageEventArgs e)
        {
            var obj = JsonConvert.DeserializeObject<XBtcResponse>(e.Data);
            if (String.Compare(obj.Response, "Login") == 0)
            {
                if (String.Compare(obj.Result.Info, "ok", true) != 0)
                {
                    Log.WriteError(obj.Result.Info, (int)GetExchangeName());
                }
            }
            else if (String.Compare(obj.Response, "FeedSubscribe") == 0)
            {
                //...
            }
            else if (String.Compare(obj.Response, "FeedTick") == 0)
            {
                var args = new OrderBookEventArgs(obj.Result);

                if (obj.Result.Symbol == "BTCUSD" && _btcOrderBookHandler != null)
                {
                    _btcOrderBookHandler(this, args);
                }
                else if (obj.Result.Symbol == "LTCUSD" && _ltcOrderBookHandler != null)
                {
                    _ltcOrderBookHandler(this, args);
                }
                else if (obj.Result.Symbol == "BCHUSD" && _bchOrderBookHandler != null)
                {
                    _bchOrderBookHandler(this, args);
                }
                else if (obj.Result.Symbol == "ETHUSD" && _ethOrderBookHandler != null)
                {
                    _ethOrderBookHandler(this, args);
                }
                else if (obj.Result.Symbol == "DSHUSD" && _dashOrderBookHandler != null)
                {
                    _dashOrderBookHandler(this, args);
                }
            }
        }

        protected override void OnClose(object sender, CloseEventArgs e)
        {
            var obj = new XBtcResponseResult();

            if (_btcOrderBookHandler != null)
                _btcOrderBookHandler(this, new OrderBookEventArgs(obj));
            if (_ltcOrderBookHandler != null)
                _ltcOrderBookHandler(this, new OrderBookEventArgs(obj));
            if (_bchOrderBookHandler != null)
                _bchOrderBookHandler(this, new OrderBookEventArgs(obj));
            if (_ethOrderBookHandler != null)
                _ethOrderBookHandler(this, new OrderBookEventArgs(obj));
            if (_dashOrderBookHandler != null)
                _dashOrderBookHandler(this, new OrderBookEventArgs(obj));
        }

        private void SubscribeOrderBook(CurrencyName currency)
        {
            SendMessage(c => c.Send(JsonConvert.SerializeObject(new XBtcSubscribeRequest
            {
                Id = DateTime.Now.Ticks.ToString(),
                Request = "FeedSubscribe",
                Params = new XBtcSubscribeRequestArgs
                {
                    Subscribe = new XBtcRequestItem[1]
                    {
                        new XBtcRequestItem { Symbol = currency.ToString() + "USD", BookDepth = 10 }
                    }
                }
            })));
        }

        private void UnsubscribeOrderBook(CurrencyName currency)
        {
            SendMessage(c => c.Send(JsonConvert.SerializeObject(new XBtcUnsubscribeRequest
            {
                Id = DateTime.Now.Ticks.ToString(),
                Request = "FeedSubscribe",
                Params = new XBtcUnsubscribeRequestArgs { Unsubscribe = new string[1] { currency.ToString() + "USD" } }
            })));
        }

        public event EventHandler<OrderBookEventArgs> OnBtcOrderBook
        {
            add
            {
                _btcOrderBookHandler += value;
                SubscribeOrderBook(CurrencyName.BTC);
            }
            remove
            {
                _btcOrderBookHandler -= value;
                UnsubscribeOrderBook(CurrencyName.BTC);
            }
        }

        public event EventHandler<OrderBookEventArgs> OnBchOrderBook
        {
            add
            {
                _bchOrderBookHandler += value;
                SubscribeOrderBook(CurrencyName.BCH);
            }
            remove
            {
                _bchOrderBookHandler -= value;
                UnsubscribeOrderBook(CurrencyName.BCH);
            }
        }

        public event EventHandler<OrderBookEventArgs> OnEthOrderBook
        {
            add
            {
                _ethOrderBookHandler += value;
                SubscribeOrderBook(CurrencyName.ETH);
            }
            remove
            {
                _ethOrderBookHandler -= value;
                UnsubscribeOrderBook(CurrencyName.ETH);
            }
        }

        public event EventHandler<OrderBookEventArgs> OnXrpOrderBook
        {
            add { }
            remove { }
        }

        public event EventHandler<OrderBookEventArgs> OnLtcOrderBook
        {
            add
            {
                _ltcOrderBookHandler += value;
                SubscribeOrderBook(CurrencyName.LTC);
            }
            remove
            {
                _ltcOrderBookHandler -= value;
                UnsubscribeOrderBook(CurrencyName.LTC);
            }
        }

        public event EventHandler<OrderBookEventArgs> OnDashOrderBook
        {
            add
            {
                _dashOrderBookHandler += value;
                SubscribeOrderBook(CurrencyName.DSH);
            }
            remove
            {
                _dashOrderBookHandler -= value;
                UnsubscribeOrderBook(CurrencyName.DSH);
            }
        }

        public event EventHandler<TradeEventArgs> OnEthTrade
        {
            add { }
            remove { }
        }

        public event EventHandler<TradeEventArgs> OnLtcTrade
        {
            add { }
            remove { }
        }

        public event EventHandler<TradeEventArgs> OnBchTrade
        {
            add { }
            remove { }
        }

        public event EventHandler<TradeEventArgs> OnBtcTrade
        {
            add { }
            remove { }
        }
    }
}
