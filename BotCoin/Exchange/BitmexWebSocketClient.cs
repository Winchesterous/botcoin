using BotCoin.DataType.Exchange;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotCoin.Exchange
{
    public class BitmexWebSocketClient : BaseWebSocketExchange
    {
        private CancellationTokenSource _wsCancelToken;
        private ClientWebSocket _wsClient;
        private Uri _wsUri;

        readonly ArraySegment<byte> _buffer;
        readonly StringBuilder _strBuilder;
        readonly int WebSocketTimeout;
        readonly object _obj;
        
        public BitmexWebSocketClient(ExchangeSettingsData setting, int? timeoutMinutes = null) 
            : base(setting, timeoutMinutes)
        {
            WebSocketTimeout = 500;
            
            _buffer     = WebSocket.CreateClientBuffer(UInt16.MaxValue, UInt16.MaxValue);
            _strBuilder = new StringBuilder();
            _wsUri      = new Uri(setting.WebsocketUrl);
            _obj        = new object();

            SetSecurityProtocol();
        }

        private void Init()
        {
            if (_wsClient == null || _wsClient.State != WebSocketState.Open)
            {
                _wsCancelToken = new CancellationTokenSource();
                _wsClient = new ClientWebSocket();
            }
        }

        public override void Logout()
        {
            StopTimer();            
            CloseForReconnect();
        }

        protected override void CloseForReconnect()
        {
            lock (_obj)
            {
                if (_wsCancelToken != null)
                    _wsCancelToken.Cancel();

                if (_wsClient == null)
                    return;

                if (_wsClient.State == WebSocketState.Open)
                    _wsClient.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "CloseForReconnect", CancellationToken.None).Wait();
            }
            Log.WriteInfo("[Bitmex] " + _wsClient.State.ToString(), (int)DataType.ExchangeName.Bitmex);
        }

        protected override void Connect()
        {
            Init();

            _wsClient.ConnectAsync(_wsUri, _wsCancelToken.Token).ContinueWith(t =>
            {
                while (_wsClient.State == WebSocketState.Connecting)
                    Thread.Sleep(WebSocketTimeout);

                StartTimer();
                Task.Run(() => WebSocketReader());

                Log.WriteInfo("[Bitmex] " + _wsClient.State.ToString(), (int)DataType.ExchangeName.Bitmex);
            })
            .Wait(_wsCancelToken.Token);
        }

        public override void Logon()
        {
            Connect();
        }

        protected void SendMessage(string message)
        {
            lock (_obj)
            {
                if (_wsClient != null && _wsClient.State == WebSocketState.Open)
                {
                    var bytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                    _wsClient.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                }
            }
        }

        private async Task WebSocketReader()
        {
            WebSocketReceiveResult result = null;
            while (!_wsCancelToken.IsCancellationRequested)
            {
                try
                {
                    do
                    {
                        lock (_obj) if (_wsClient.State != WebSocketState.Open)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        result = await _wsClient.ReceiveAsync(_buffer, CancellationToken.None);

                        if (result.Count > 0)
                            _strBuilder.Append(Encoding.UTF8.GetString(_buffer.Array, 0, result.Count));
                    }
                    while (!result.EndOfMessage);

                    if (result.Count > 0)
                    {
                        var msg = _strBuilder.ToString();
                        _strBuilder.Clear();
                        OnMessage(msg);
                    }
                }
                catch (Exception ex)
                {
                    var exName = GetExchangeName();
                    Log.WriteError(String.Format("[{0}] {1}", exName, ex.Message), (int)exName);
                }
            }
        }

        protected virtual void OnMessage(string message)
        {
        }
    }
}
