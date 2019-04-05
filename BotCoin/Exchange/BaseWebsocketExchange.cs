using BotCoin.DataType.Exchange;
using System;
using System.Net;
using System.Threading;
using WebSocketDotNet;

namespace BotCoin.Exchange
{
    public class BaseWebSocketExchange : BaseExchange
    {
        protected int _tryCount = 10;

        readonly System.Timers.Timer _timer;
        protected WebSocket _client;        
        protected DateTime _tickTime;

        public event EventHandler BeforeReconnect;
        public event EventHandler AfterReconnect;

        protected BaseWebSocketExchange(ExchangeSettingsData setting, int? timeoutMinutes = null) : base(setting)
        {
            WsUrl = setting.WebsocketUrl;
            this.BeforeReconnect += (s, e) => { };
            this.AfterReconnect += (s, e) => { };

            if (timeoutMinutes.HasValue)
            {
                _timer = new System.Timers.Timer(timeoutMinutes.Value * 60000);
                _timer.Elapsed += OnTimerElapsed;
            }
        }

        public bool ActiveState
        {
            set; get;
        }

        public string WsUrl
        {
            set; get;
        }

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (DateTime.UtcNow > _tickTime)
            {
                Reconnect();
            }
        }
               
        protected virtual void SetSecurityProtocol()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private void InitClient(string url)
        {
            _client = new WebSocket(url);
            SetSecurityProtocol();

            _client.OnClose += OnClose;
            _client.OnOpen += OnOpen;
            _client.OnError += OnError;
        }
                
        protected void SendMessage(Action<WebSocket> action)
        {
            if (_client != null && _client.ReadyState == WebSocketState.Open)
            {
                action(_client);
            }
        }

        protected virtual void Reconnect()
        {
            BeforeReconnect(this, null);
            CloseForReconnect();
            Thread.Sleep(3000);
            Connect();
            AfterReconnect(this, null);
        }

        protected virtual void Connect()
        {
            int tryCount = _tryCount;
            var exName = GetExchangeName();

            lock (_client)
            {
                _client.ConnectAsync();

                while (_client.ReadyState != WebSocketState.Open)
                {
                    if (tryCount-- == 0)
                    {
                        var msg = String.Format("Connection failed [{0}]", exName.ToString());
                        Log.WriteInfo(msg, (int)exName);
                        throw new InvalidOperationException(msg);
                    }
                    Log.WriteInfo(String.Format("[{0}] connecting...", exName.ToString()), (int)exName);
                    Thread.Sleep(1000);
                }
            }
        }

        public override void Logon()
        {
            if (_client == null)
            {
                InitClient(WsUrl);
            }
            _client.OnMessage += OnMessage;
            Connect();
            ActiveState = true;

            StartTimer();
        }

        protected void StartTimer()
        {
            if (_timer != null)
                _timer.Start();
        }

        protected void StopTimer()
        {
            if (_timer != null)
                _timer.Stop();

            _tickTime = DateTime.UtcNow;
        }

        public override void Logout()
        {
            if (_client == null)
                return;

            StopTimer();

            _client.OnMessage -= OnMessage;
            ActiveState = false;

            if (_account != null)
                _account.Stop();

            CloseForReconnect();            
        }

        protected virtual void CloseForReconnect()
        {
            var exName = GetExchangeName();
            lock (_client)
            {
                _client.CloseAsync();

                while (_client.ReadyState != WebSocketState.Closed)
                {
                    Log.WriteInfo(String.Format("[{0}] closing...", exName.ToString()), (int)exName);
                    Thread.Sleep(500);
                }
            }
        }

        protected virtual void SendPong()
        {
        }

        protected virtual void OnMessage(object sender, MessageEventArgs e)
        {
        }

        protected virtual void OnError(object sender, ErrorEventArgs e)
        {
            Log.WriteInfo(String.Format("[{0}] {1}", GetExchangeName(), e.Message), (int)GetExchangeName());
        }

        protected virtual void OnClose(object sender, CloseEventArgs e)
        {
            Log.WriteInfo(String.Format("[{0}] closed", GetExchangeName()), (int)GetExchangeName());
        }

        protected virtual void OnOpen(object sender, EventArgs e)
        {
            Log.WriteInfo(String.Format("[{0}] connected", GetExchangeName()), (int)GetExchangeName());
        }
    }
}
