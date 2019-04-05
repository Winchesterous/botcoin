using BotCoin.DataType;
using BotCoin.Logger;
using BotCoin.Service;
using System;
using System.Configuration;
using System.ServiceProcess;
using System.Timers;

namespace BotCoin.InitService
{
    class SelfHost
    {
        public const string BotcoinTradeDataService = "BotCoinTradeDatabot";

        readonly ServiceEventLogger _log;
        readonly Timer _timer;
        readonly int _limitMinutes;
        
        public SelfHost()
        {
            _log = new ServiceEventLogger(ServiceName.InitService, new DbRepositoryService());
            _limitMinutes = Int32.Parse(ConfigurationManager.AppSettings["IntervalMinutes"]);

            _timer = new Timer(_limitMinutes * 60000);
            _timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var db = (DbRepositoryService)_log.DbRepository;
                var exch = db.ShouldRestartTradeDataBot(_limitMinutes);

                if (exch != ExchangeName.Undefined)
                {
                    var sc = new ServiceController(BotcoinTradeDataService);

                    _log.WriteInfo("-------Stopping-------");
                    _log.WriteInfo("REASON: " + exch.ToString());
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    _log.WriteInfo(".....Starting.....");
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    _log.WriteInfo("-------Started-------");
                }
            }
            catch (Exception ex)
            {
                _log.WriteError(ex.Message);
            }
        }

        public void Start()
        {
            _timer.Start();
            _log.WriteInfo("Start");

            OnTimerElapsed(null, null);
        }

        public void Stop()
        {
            _timer.Stop();
            _log.WriteInfo("Stop");
        }
    }
}
