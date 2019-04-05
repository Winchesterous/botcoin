using BotCoin.Core;
using System;
using System.Linq;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using BotCoin.DataType;
using System.Collections.Generic;
using BotCoin.Service;

namespace BotCoin.DbGatewayService
{
    internal class ExchangePricesPublisher
    {
        readonly List<Tuple<CurrencyName, IPEndPoint>> _connections;
        readonly ServiceEventLogger _log;
        readonly TcpService _tcp;
        readonly object _obj;
        readonly Timer _timer;
        bool _started;
        int _timeout, _counter;

        public ExchangePricesPublisher(ServiceEventLogger log)
        {
            _connections = new List<Tuple<CurrencyName, IPEndPoint>>();
            _tcp         = new TcpService();
            _obj         = new object();
            _timeout     = Int32.Parse(ConfigurationManager.AppSettings["RestSchedulerTimeoutSec"]);
            _log         = log;

            _timer = new Timer();
            _timer.Elapsed += OnTimerElapsed;
        }

        private void CreateConnection(CurrencyName instrument)
        {
            var config     = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            var connection = config.FindConnectionElement("RestScheduler");
            var strAddr    = String.Format(connection.DomainName, instrument.ToString().ToLower());
            var ipAddr     = Dns.GetHostEntry(strAddr).AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).First();

            _log.WriteInfo(String.Format("{0} subscribed", instrument.ToString()));
            _connections.Add(new Tuple<CurrencyName, IPEndPoint>(instrument, new IPEndPoint(ipAddr, connection.Port)));
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Tuple<CurrencyName, IPEndPoint> pair = null;
            lock (_obj)
            {
                if (_connections.Count == 0) return;
                pair = _connections[_counter];
            }
            try
            {
                _tcp.InvokeServer(pair.Item2, new byte[] { (byte)pair.Item1 });
            }
            catch (SocketException sx)
            {
                _log.WriteError(String.Format("[{0}] {1}", sx.SocketErrorCode.ToString(), sx.Message));
            }
            if (++_counter == _connections.Count)
                _counter = 0;
        }

        public void Connect(CurrencyName instrument)
        {
            if (_started)
                _timer.Stop();

            lock (_obj)
            {
                CreateConnection(instrument);
                _timer.Interval = _timeout * 1000 / _connections.Count;
            }

            TimerAction(true);
        }

        public void Disconnect(CurrencyName instrument)
        {
            if (!_started)
                return;

            _counter = 0;
            TimerAction(false);

            lock (_obj)
            {
                var connection = _connections.Where(p => p.Item1 == instrument).SingleOrDefault();
                _connections.Remove(connection);
                _log.WriteInfo(String.Format("{0} unsubscribed", instrument.ToString()));
                TimerAction(_connections.Count > 0);
            }
        }

        private void TimerAction(bool start)
        {
            if (start)
            {
                _started = true;
                _timer.Start();
            }
            else
            {
                _started = false;
                _timer.Stop();
            }
        }
        public void Reset()
        {
            lock (_obj) _connections.Clear();
            _counter = 0;

            TimerAction(false);
            _timer.Elapsed -= OnTimerElapsed;
        }
    }
}
