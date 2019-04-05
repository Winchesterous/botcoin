using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using WebSocketDotNet;

namespace BotCoin.Exchange
{
    [Exchange(Name = "HitBtc", IsWebsocket = true)]
    public class HitBtcExchange : BaseWebSocketExchange, IExchangeEvents
    {
        EventHandler<OrderBookEventArgs> _btcOrderBookHandler;
        EventHandler<OrderBookEventArgs> _ethOrderBookHandler;
        EventHandler<OrderBookEventArgs> _bchOrderBookHandler;
        EventHandler<OrderBookEventArgs> _ltcOrderBookHandler;
        EventHandler<OrderBookEventArgs> _dashOrderBookHandler;
        EventHandler<OrderBookEventArgs> _xrpOrderBookHandler;
        EventHandler<TradeEventArgs> _btcTradeHandler;

        public HitBtcExchange(ExchangeSettingsData setting) : base(setting)
        {
            _restExchange = new HitBtcRestExchange(setting);
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.HitBtc;
        }

        public override UserAccount GetBalances()
        {
            return _restExchange.GetBalances();
        }

        public override bool CancelOrder(string orderId, string symbol = null)
        {
            return _restExchange.CancelOrder(orderId, symbol);
        }

        protected override void OnClose(object sender, CloseEventArgs e)
        {
            var obj = new HitBtcOrderBook();

            if (_btcOrderBookHandler != null) _btcOrderBookHandler(this, new OrderBookEventArgs(obj));
            if (_ethOrderBookHandler != null) _ethOrderBookHandler(this, new OrderBookEventArgs(obj));
            if (_bchOrderBookHandler != null) _bchOrderBookHandler(this, new OrderBookEventArgs(obj));
            if (_ltcOrderBookHandler != null) _ltcOrderBookHandler(this, new OrderBookEventArgs(obj));
            if (_dashOrderBookHandler != null) _dashOrderBookHandler(this, new OrderBookEventArgs(obj));
            if (_xrpOrderBookHandler != null) _xrpOrderBookHandler(this, new OrderBookEventArgs(obj));
        }

        protected override void OnMessage(object sender, MessageEventArgs e)
        {
            var time = DateTime.UtcNow;
            var strData = e.Data.Substring(0, 100);
            
            if (strData.Contains("snapshotOrderbook") || strData.Contains("updateOrderbook"))
            {
                var response = JsonConvert.DeserializeObject<HitBtcOrderBook>(e.Data);
                var data = new OrderBookEventArgs(response, time);

                if (response.Params.Symbol == "BTCUSD" && _btcOrderBookHandler != null)
                    _btcOrderBookHandler(this, data);
                else if (response.Params.Symbol == "ETHUSD" && _ethOrderBookHandler != null)
                    _ethOrderBookHandler(this, data);
                else if (response.Params.Symbol == "BCHUSD" && _bchOrderBookHandler != null)
                    _bchOrderBookHandler(this, data);
                else if (response.Params.Symbol == "LTCUSD" && _ltcOrderBookHandler != null)
                    _ltcOrderBookHandler(this, data);
                else if (response.Params.Symbol == "DASHUSD" && _dashOrderBookHandler != null)
                    _dashOrderBookHandler(this, data);
                else if (response.Params.Symbol == "XRPUSD" && _xrpOrderBookHandler != null)
                    _xrpOrderBookHandler(this, data);
            }
            else if (strData.Contains("snapshotTrades") || strData.Contains("updateTrades"))
            {
            }
        }

        public void SubscribeOrderBook(bool subscribe, string symbol)
        {
            var json = JsonConvert.SerializeObject(new HitBtcRequest
            {
                Method = subscribe ? "subscribeOrderbook" : "unsubscribeOrderbook",
                Params = new HitBtcRequest.HitBtcRequestParams { Symbol = symbol },
                Id = Guid.NewGuid().ToString().ToLower().Split('-')[0]
            });
            SendMessage(c => c.Send(json.Replace("Params", "params")));
        }

        public void SubscribeTrade(bool subscribe, string symbol)
        {
            var json = JsonConvert.SerializeObject(new HitBtcRequest
            {
                Method = subscribe ? "subscribeTrades" : "unsubscribeTrades",
                Params = new HitBtcRequest.HitBtcRequestParams { Symbol = symbol },
                Id = Guid.NewGuid().ToString().ToLower().Split('-')[0]
            });
            SendMessage(c => c.Send(json.Replace("Params", "params")));
        }

        public event EventHandler<OrderBookEventArgs> OnBtcOrderBook
        {
            add
            {
                _btcOrderBookHandler += value;
                SubscribeOrderBook(true, "BTCUSD");
            }
            remove
            {
                _btcOrderBookHandler -= value;
                SubscribeOrderBook(false, "BTCUSD");
            }
        }

        public event EventHandler<OrderBookEventArgs> OnEthOrderBook
        {
            add
            {
                _ethOrderBookHandler += value;
                SubscribeOrderBook(true, "ETHUSD");
            }
            remove
            {
                _ethOrderBookHandler -= value;
                SubscribeOrderBook(false, "ETHUSD");
            }
        }

        public event EventHandler<OrderBookEventArgs> OnBchOrderBook
        {
            add
            {
                _bchOrderBookHandler += value;
                SubscribeOrderBook(true, "BCHUSD");
            }
            remove
            {
                _bchOrderBookHandler -= value;
                SubscribeOrderBook(false, "BCHUSD");
            }
        }
        
        public event EventHandler<OrderBookEventArgs> OnLtcOrderBook
        {
            add
            {
                _ltcOrderBookHandler += value;
                SubscribeOrderBook(true, "LTCUSD");
            }
            remove
            {
                _ltcOrderBookHandler -= value;
                SubscribeOrderBook(false, "LTCUSD");
            }
        }

        public event EventHandler<OrderBookEventArgs> OnDashOrderBook
        {
            add
            {
                _dashOrderBookHandler += value;
                SubscribeOrderBook(true, "DASHUSD");
            }
            remove
            {
                _dashOrderBookHandler -= value;
                SubscribeOrderBook(false, "DASHUSD");
            }
        }

        public event EventHandler<OrderBookEventArgs> OnXrpOrderBook
        {
            add
            {
                _xrpOrderBookHandler += value;
                SubscribeOrderBook(true, "XRPUSD");
            }
            remove
            {
                _xrpOrderBookHandler -= value;
                SubscribeOrderBook(false, "XRPUSD");
            }
        }

        public event EventHandler<TradeEventArgs> OnBtcTrade
        {
            add
            {
                _btcTradeHandler += value;
                SubscribeTrade(true, "BTCUSD");
            }
            remove
            {
                _btcTradeHandler -= value;
                SubscribeTrade(false, "BTCUSD");
            }
        }
    }
}
