using BotCoin.DataType.Database;
using BotCoin.Logger;
using Fleck;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace BotCoin.BitmexBotService
{
    internal class WebSocketServer
    {
        readonly List<IWebSocketConnection> _sockets;
        readonly Fleck.WebSocketServer _server;
        readonly IServiceEventLogger _log;
        readonly object _obj;

        public WebSocketServer(IServiceEventLogger log, string webSocketUrl)
        {
            _server  = new Fleck.WebSocketServer(webSocketUrl);
            _sockets = new List<IWebSocketConnection>();
            _obj     = new object();
            _log     = log;
        }

        public void Start()
        {
            _server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    _log.WriteInfo("----Connect client----");
                    lock (_obj) _sockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    _log.WriteInfo("Client closed");
                    lock (_obj) _sockets.Remove(socket);
                };
                socket.OnPing = msg =>
                {
                    socket.SendPong(Encoding.UTF8.GetBytes("pong"));
                };
            });
        }

        public void Stop()
        {
            lock (_obj)
            {
                _sockets.ForEach(s => s.Close());
                _sockets.Clear();
            }
            _server.Dispose();
        }

        public void SendMessage(DbMessage msg)
        {
            var json = JsonConvert.SerializeObject(msg);
            lock (_obj) _sockets.ForEach(s => s.Send(json));
        }
    }
}
