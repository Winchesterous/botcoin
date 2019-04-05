using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Security.Authentication;
using WebSocketDotNet;

namespace BotCoin.Exchange
{
    [Exchange(Name="Cex", IsWebsocket = true)]
    public class CexExchange : BaseWebSocketExchange, IExchangeEvents
    {
        EventHandler<OrderBookEventArgs> _btcOrderBookHandler;
        EventHandler<OrderBookEventArgs> _bchOrderBookHandler;
        EventHandler<OrderBookEventArgs> _ethOrderBookHandler;
        EventHandler<OrderBookEventArgs> _xrpOrderBookHandler;
        EventHandler<OrderBookEventArgs> _dashOrderBookHandler;
        EventHandler<TradeEventArgs> _btcTradeHandler;
        EventHandler<TradeEventArgs> _bchTradeHandler;
        EventHandler<TradeEventArgs> _ethTradeHandler;

        public CexExchange(ExchangeSettingsData setting) : base(setting, 10)
        {
            _restExchange = new CexRestExchange(setting);
        }

        protected override void SetSecurityProtocol()
        {
            _client.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
        }

        public override void Logon()
        {
            base.Logon();
            Authentication();
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Cex;
        }

        protected override void OnMessage(object sender, MessageEventArgs e)
        {
            var response = JsonConvert.DeserializeObject<CexResponse>(e.Data);

            switch (response.e)
            {
            case "md_update":
                OnMarketDataUpdate(e.Data);
                break;
            case "order-book-subscribe":
                OnMarketDataUpdate(e.Data);
                break;
            case "auth":
                OnAuthorizationResponse(e.Data);
                break;
            case "ping":
                SendPong();
                break;
            case "ticker":
                //...
                break;
            case "disconnecting":
                Log.WriteInfo("CEX disconnecting. " + response.reason);
                break;
            }
        }

        protected override void OnClose(object sender, CloseEventArgs e)
        {
            var obj = new CexOrderBookResponse { data = new CexOrderBookResponse.CexOrderBookData() };

            if (_btcOrderBookHandler != null) _btcOrderBookHandler(this, new OrderBookEventArgs(obj));
            if (_bchOrderBookHandler != null) _bchOrderBookHandler(this, new OrderBookEventArgs(obj));
            if (_ethOrderBookHandler != null) _ethOrderBookHandler(this, new OrderBookEventArgs(obj));
            if (_xrpOrderBookHandler != null) _xrpOrderBookHandler(this, new OrderBookEventArgs(obj));
            if (_dashOrderBookHandler != null) _dashOrderBookHandler(this, new OrderBookEventArgs(obj));

            base.OnClose(sender, e);
        }

        protected override void SendPong()
        {
            SendMessage(c => c.Send("{\"e\":\"pong\"}"));
        }

        private void Authentication()
        {
            var client = ((CexRestExchange)_restExchange).Client;
            var nonce  = client.GetTimestampInSeconds();
            var hash   = String.Format(CultureInfo.InvariantCulture, "{0}{1}", nonce, client.PublicKey);

            SendMessage(c => c.Send(JsonConvert.SerializeObject(new CexRequest
            {
                e = "auth",
                auth = new CexAuth
                {
                    signature = client.CreateSignature256(hash),
                    timestamp = nonce,
                    key = client.PublicKey
                }
            })));
        }

        protected override void Reconnect()
        {
            CloseForReconnect();
            Connect();
            Authentication();

            if (_btcOrderBookHandler != null) SubscribeOrderBook(CurrencyName.BTC);
            if (_bchOrderBookHandler != null) SubscribeOrderBook(CurrencyName.BCH);
            if (_ethOrderBookHandler != null) SubscribeOrderBook(CurrencyName.ETH);
            if (_xrpOrderBookHandler != null) SubscribeOrderBook(CurrencyName.XRP);
            if (_dashOrderBookHandler != null) SubscribeOrderBook(CurrencyName.DSH);
        }
        
        private void OnAuthorizationResponse(string json)
        {
            var response = JsonConvert.DeserializeObject<CexAuthResponse>(json);

            if (response.ok == "error")
                Log.WriteError("CEX error. " + response.data.error);
            else
                Log.WriteInfo("CEX authorized");

            if (String.Compare(response.ok, "error", true) == 0)
            {
                Logout();
            }
        }

        private void OnMarketDataUpdate(string json)
        {
            var response = JsonConvert.DeserializeObject<CexOrderBookResponse>(json);

            if (response.ok == "error")
            {
                Log.WriteError(response.data.error, (int)GetExchangeName());
                return;
            }
            if (String.IsNullOrEmpty(response.data.pair))
            {
                return;
            }
            if (response.data.pair.StartsWith("BTC"))
            {
                if (_btcOrderBookHandler != null)
                    _btcOrderBookHandler(this, new OrderBookEventArgs(response));
            }
            else if (response.data.pair.StartsWith("BCH"))
            {
                if (_bchOrderBookHandler != null)
                    _bchOrderBookHandler(this, new OrderBookEventArgs(response));
            }
            else if (response.data.pair.StartsWith("ETH"))
            {
                if (_ethOrderBookHandler != null)
                    _ethOrderBookHandler(this, new OrderBookEventArgs(response));
            }
            else if (response.data.pair.StartsWith("XRP"))
            {
                if (_xrpOrderBookHandler != null)
                    _xrpOrderBookHandler(this, new OrderBookEventArgs(response));
            }
            else if (response.data.pair.StartsWith("DASH"))
            {
                if (_dashOrderBookHandler != null)
                    _dashOrderBookHandler(this, new OrderBookEventArgs(response));
            }
        }

        private void SubscribeOrderBook(CurrencyName currency)
        {
            var name = currency.ToString();
            if (currency == CurrencyName.DSH)
                name = "DASH";

            SendMessage(c => c.Send(JsonConvert.SerializeObject(new CexOrderBookRequest
            {
                e = "order-book-subscribe",
                data = new CexOrderBookRequest.CexOrderBookData
                {
                    pair = new string[] { name, "USD" },
                    subscribe = true,
                    depth = 20
                },
                oid = "1435927928274_3_order-book-subscribe"
            })));
        }

        private void SubscribeTrade(CurrencyName currency)
        {
            //...
        }

        private void UnsubscribeOrderBook(CurrencyName currency)
        {
            var name = currency.ToString();
            if (currency == CurrencyName.DSH)
                name = "DASH";

            SendMessage(c => c.Send(JsonConvert.SerializeObject(new CexOrderBookRequest
            {
                e = "order-book-unsubscribe",
                data = new CexOrderBookRequest.CexOrderBookData
                {
                    pair = new string[] { name, "USD" }
                },
                oid = "1435927928274_4_order-book-unsubscribe"
            })));
        }

        private void UnsubscribeTrade(CurrencyName currency)
        {
            //...
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

        public event EventHandler<TradeEventArgs> OnBtcTrade
        {
            add
            {
                _btcTradeHandler += value;
                SubscribeTrade(CurrencyName.BTC);
            }
            remove
            {
                _btcTradeHandler -= value;
                UnsubscribeTrade(CurrencyName.BTC);
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

        public event EventHandler<TradeEventArgs> OnBchTrade
        {
            add
            {
                _bchTradeHandler += value;
                SubscribeTrade(CurrencyName.BCH);
            }
            remove
            {
                _bchTradeHandler -= value;
                UnsubscribeTrade(CurrencyName.BCH);
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
            add
            {
                _xrpOrderBookHandler += value;
                SubscribeOrderBook(CurrencyName.XRP);
            }
            remove
            {
                _xrpOrderBookHandler -= value;
                UnsubscribeOrderBook(CurrencyName.XRP);
            }
        }

        public event EventHandler<TradeEventArgs> OnEthTrade
        {
            add
            {
                _ethTradeHandler += value;
                SubscribeTrade(CurrencyName.ETH);
            }
            remove
            {
                _ethTradeHandler -= value;
                UnsubscribeTrade(CurrencyName.ETH);
            }
        }

        public event EventHandler<OrderBookEventArgs> OnLtcOrderBook
        {
            add { }
            remove { }
        }

        public event EventHandler<TradeEventArgs> OnLtcTrade
        {
            add { }
            remove { }
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
    }
}
