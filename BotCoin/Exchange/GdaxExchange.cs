using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using WebSocketDotNet;

namespace BotCoin.Exchange
{
    [Exchange(Name="Gdax",IsWebsocket = true)]
    public class GdaxExchange : BaseWebSocketExchange
    {
        public event EventHandler<ExchangePricesEventArgs> InstrumentReceived;
        public event EventHandler<TickerEventArgs> TickerReceived;

        public GdaxExchange(ExchangeSettingsData setting) : base(setting)
        {
            this.InstrumentReceived += (s, e) => { };
            this.TickerReceived += (s, e) => { };
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Gdax;
        }

        public void Subscription(bool enable, params string[] channel)
        {
            var json = JsonConvert.SerializeObject(new GdaxRequest
            {
                Type = enable ? "subscribe" : "unsubscribe",                
                Symbols = new string[] { "BTC-USD" },
                Channels = channel
            });
            SendMessage(c => c.Send(json));
        }

        protected override void OnMessage(object sender, MessageEventArgs e)
        {
            var response = JsonConvert.DeserializeObject<GdaxResponse>(e.Data);
            if (response.HasError)
            {
                System.Diagnostics.Debug.WriteLine("ERR " + response.Message);
            }
            if (!response.Time.HasValue)
                return;

            if (response.Type == "ticker")
            {
                var args = new TickerEventArgs(response);
                TickerReceived(this, args);
            }
            else
            {
                var args = new ExchangePricesEventArgs(response);
                InstrumentReceived(this, args);
            }
        }
    }
}
