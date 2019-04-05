using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using ProExchange.Common.Security;
using ProExchange.JSON.API;
using ProExchange.JSON.API.Notifications;
using ProExchange.JSON.API.Requests;
using ProExchange.JSON.API.Responses;
using System;
using WebSocketDotNet;

namespace BotCoin.Exchange
{
    [Exchange(Name = "Btcc", IsWebsocket = true)]
    public class BtccExchange : BaseWebSocketExchange, IExchangeEvents
    {
        EventHandler<OrderBookEventArgs> _btcOrderBookHandler;
        readonly JsonDeserializer<JsonMessageOut> _deserializer;
        readonly string PublicKey;
        readonly string PrivateKey;

        public BtccExchange(ExchangeSettingsData setting) : base(setting, 60)
        {
            _deserializer = new JsonDeserializer<JsonMessageOut>();
            PublicKey = setting.PublicKey;
            PrivateKey = setting.SecretKey;

            _deserializer.On<Ticker>(ticker => 
            {
            });
            _deserializer.On<ProExchange.JSON.API.Notifications.OrderBook>(obj =>
            {
            });
            _deserializer.On<QuoteResponse>(response =>
            {
                //response.OrderBook
            });
            _deserializer.On<ErrorResponse>(error =>
            {
            });
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Btcc;
        }

        public override UserAccount GetBalances()
        {
            return _restExchange.GetBalances();
        }

        protected override void OnClose(object sender, CloseEventArgs e)
        {
            //if (_btcOrderBookHandler != null)
            //    _btcOrderBookHandler(this, new OrderBookEventArgs(new BinanceOrderBookData()));
        }

        protected override void OnOpen(object sender, EventArgs e)
        {
            var request = new BtccLoginRequest
            {
                ClientRequestId = Guid.NewGuid().ToString("N"),
                Date            = DateTime.UtcNow.ToString("yyyyMMdd"),
                Account         = PublicKey
            };
            request.Signature = SignatureEngine.ComputeSignature(SignatureEngine.ComputeHash(PrivateKey), SignatureEngine.PrepareMessage(request, SignatureEngine.MessageSignatureOrdering.Alphabetical));
            SendMessage(c => c.Send(JsonConvert.SerializeObject(request)));
        }

        protected override void OnMessage(object sender, MessageEventArgs e)
        {
            //var response = JsonConvert.DeserializeObject<QuoteResponse>(e.Data);
        }

        public event EventHandler<OrderBookEventArgs> OnBtcOrderBook
        {
            add
            {
                _btcOrderBookHandler += value;
                var json = JsonConvert.SerializeObject(new QuoteRequest { Symbol = "BTCUSD", QuoteType = 2 });
                SendMessage(c => c.Send(json));
            }
            remove
            {
                _btcOrderBookHandler -= value;
            }
        }

        public event EventHandler<OrderBookEventArgs> OnBchOrderBook
        {
            add { }
            remove { }
        }

        public event EventHandler<OrderBookEventArgs> OnEthOrderBook
        {
            add { }
            remove { }
        }

        public event EventHandler<OrderBookEventArgs> OnLtcOrderBook
        {
            add { }
            remove { }
        }

        public event EventHandler<OrderBookEventArgs> OnDashOrderBook
        {
            add { }
            remove { }
        }

        public event EventHandler<OrderBookEventArgs> OnXrpOrderBook
        {
            add { }
            remove { }
        }
    }
}
