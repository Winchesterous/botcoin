using System;
using BotCoin.DataType.Exchange;
using BotCoin.Exchange;
using Newtonsoft.Json;
using BotCoin.DataType.Database;
using WebSocketDotNet;
using BotCoin.DataType;

namespace BotCoin.BitmexScalper.Domain
{
    internal class BitmexBotClient : BaseWebSocketExchange
    {
        readonly System.Timers.Timer _timer;

        public event EventHandler<DbIndicatorVwapEventArgs> VwapGainReceived;
        public event EventHandler<DbPositionEventArgs> PositionReceived;
        public event EventHandler<DbPositionEventArgs> TradeReceived;
        
        public BitmexBotClient(MainWindow mainWnd, ExchangeSettingsData setting, int pingTimeoutMinutes) : base(setting)
        {
            this.VwapGainReceived += (s, e) => { };
            this.PositionReceived += (s, e) => { };
            this.TradeReceived    += (s, e) => { };

            _timer = new System.Timers.Timer(pingTimeoutMinutes * 60000);
            _timer.Elapsed += (s, e) =>
            {
                if (!_client.IsAlive)
                {
                    Reconnect();
                    Log.WriteWarning("BtxBot reconnect after ping");
                    mainWnd.LogServiceEvent("BitmexBot was reconnected");
                }
            };
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Bitmex;
        }

        protected override void OnMessage(object sender, MessageEventArgs e)
        {
            var obj = JsonConvert.DeserializeObject<DbMessage>(e.Data);
            if (obj.Positions != null)
            {
                PositionReceived(this, new DbPositionEventArgs(obj.Positions));
            }
            if (obj.Trades != null)
            {
                TradeReceived(this, new DbPositionEventArgs(obj.Trades));
            }
            if (obj.VwapGains != null)
            {
                VwapGainReceived(this, new DbIndicatorVwapEventArgs(obj.VwapGains.ToArray()));
            }
        }

        public override void Logon()
        {
            base.Logon();
            _timer.Start();
        }

        public override void Logout()
        {
            _timer.Stop();
            base.Logout();
        }
    }
}
