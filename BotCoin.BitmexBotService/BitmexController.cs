using BotCoin.ApiClient;
using BotCoin.Core;
using BotCoin.DataType.Database;
using BotCoin.DataType.Exchange;
using BotCoin.DataType.WebApi;
using BotCoin.Exchange;
using BotCoin.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Timers;

namespace BotCoin.BitmexBotService
{
    internal class BitmexController
    {
        readonly RestApiClient2 _apiClient;
        readonly WebSocketServer _wsServer;
        readonly PositionRequest _posRequest;
        readonly IndicatorRequest _vwapRequest;
        readonly OrderRequest _orderRequest;
        readonly MarginRequest _marginRequest;
        readonly WalletRequest _walletRequest;        
        readonly Timer _instrumentSettingsTimer;
        readonly Timer _vwapGainsTimer;
        DbIndicatorVwapLite[] _vwaps;
        BitmexExchange _bitmex;
        BitmexUser _btxUser;

        public BitmexController()
        {
            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");

            _apiClient = new RestApiClient2(config.Connections.GetElement("WebApi").Url);
            Log        = new RestServiceEventLogger(_apiClient, DataType.ServiceName.BitmexBot);            
            _wsServer  = new WebSocketServer(Log, config.Connections.GetElement("WebSocket").Url);
            _vwaps     = new DbIndicatorVwapLite[] { };

            _instrumentSettingsTimer = new Timer(config.BitmexBot.TimeoutHours * 3600000);
            _vwapGainsTimer = new Timer(20000);

            _marginRequest = new MarginRequest(_apiClient);
            _walletRequest = new WalletRequest(_apiClient);
            _orderRequest  = new OrderRequest(_apiClient);
            _posRequest    = new PositionRequest(_apiClient);
            _vwapRequest   = new IndicatorRequest(_apiClient);

            _instrumentSettingsTimer.Elapsed += OnInstrumentSettingsTimerElapsed;
            _vwapGainsTimer.Elapsed += OnVwapGainsTimerElapsed;
        }
                
        private void OnVwapGainsTimerElapsed(object sender, ElapsedEventArgs e)
        {
            DoAction(() =>
            {
                var items = _vwapRequest.GetVwapGains(DateTime.UtcNow, "Bitmex");
                if (_vwaps.Length > 0)
                {
                    var added = items.Select(i => i.Id).Except(_vwaps.Select(i => i.Id)).ToArray();
                    if (added.Length > 0)
                    {
                        var msg = new DbMessage { VwapGains = new List<DbIndicatorVwapLite>() };
                        foreach (var id in added)
                        {
                            msg.VwapGains.Add(items.Where(i => i.Id == id).Single());
                        }
                        _wsServer.SendMessage(msg);
                    }
                }
                _vwaps = items;
            });
        }

        private void OnInstrumentSettingsTimerElapsed(object sender, ElapsedEventArgs e)
        {
            DoAction(() =>
            {
                var settings = _bitmex.Exchange.Client.GetInstrumentSettings();
                SaveInstruments(settings);
            });
        }

        private void SaveInstruments(BitmexInstrumentSettings[] settings)
        {
            var request = new SettingRequest(_apiClient);

            settings = settings.Where(s => s.MakerFee.HasValue).ToArray();
            request.SaveBitmexInstruments(_btxUser.Id, settings);
        }

        public IServiceEventLogger Log
        {
            private set; get;
        }

        private void DoAction(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Log.WriteError(ex);
            }
        }

        public void Start()
        {
            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            var btxSetting = SettingRequest.Get(_apiClient, config.BitmexBot.AccountMode);

            _bitmex = new BitmexExchange(btxSetting);
            _bitmex.Log = Log;

            _bitmex.ExecutionChanged += OnExecutionChanged;
            _bitmex.MarginChanged    += OnMarginChanged;
            _bitmex.OrderChanged     += OnOrderChanged;
            _bitmex.WalletChanged    += OnWalletChanged;
            _bitmex.AuthPassed       += OnAuthPassed;

            _bitmex.Logon();           

            _btxUser = _bitmex.Exchange.Client.GetAccount();            
            _bitmex.SubscriptionAuth(null);

            _instrumentSettingsTimer.Start();
            _vwapGainsTimer.Start();
            _wsServer.Start();

            OnInstrumentSettingsTimerElapsed(null, null);
            OnVwapGainsTimerElapsed(null, null);
        }
                
        public void Stop()
        {
            _instrumentSettingsTimer.Stop();
            _vwapGainsTimer.Stop();
            _bitmex.Logout();
            _wsServer.Stop();
        }
                
        private void OnWalletChanged(object sender, DataType.ExchangePricesEventArgs e)
        {
            DoAction(() => _walletRequest.SaveWallet(e.BtxWallet));
        }

        private void OnExecutionChanged(object sender, DataType.BitmexEventArgs e)
        {
            if (e.BtxExecution.Commission.HasValue)
            {
                DoAction(() =>
                {
                    var msg = _posRequest.SavePosition(e.BtxExecution, _btxUser);
                    if (msg != null) _wsServer.SendMessage(msg);
                });
            }
        }

        private void OnOrderChanged(object sender, DataType.ExchangePricesEventArgs e)
        {
            foreach (var order in e.BtxOrders)
            {
                DoAction(() => _orderRequest.Save(order, _btxUser));
            }
        }

        private void OnMarginChanged(object sender, DataType.ExchangePricesEventArgs e)
        {
            DoAction(() => _marginRequest.SaveChanges(e.BtxMargin));
        }

        private void OnAuthPassed(object sender, EventArgs e)
        {
            if (sender != null)
            {
                _bitmex.Subscription(true, BtxSubscriptionItem.execution.ToString());
                _bitmex.Subscription(true, BtxSubscriptionItem.funding.ToString());
                _bitmex.Subscription(true, BtxSubscriptionItem.order.ToString());
                _bitmex.Subscription(true, BtxSubscriptionItem.margin.ToString());
                _bitmex.Subscription(true, BtxSubscriptionItem.wallet.ToString());
            }
            else
                Log.WriteError("Authorization failed");
        }
    }
}
