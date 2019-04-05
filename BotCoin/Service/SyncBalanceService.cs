using BotCoin.DataType;
using BotCoin.Exchange;
using BotCoin.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Timers;

namespace BotCoin.Service
{
    public class SyncBalanceService
    {
        readonly static bool ProductionMode;
        readonly DbRepositoryService _dbRepo;
        readonly ServiceEventLogger _log;
        readonly Timer _timer;
        List<IExchange> _exchanges;

        static SyncBalanceService()
        {
            var connStr = ConfigurationManager.ConnectionStrings["Botcoin"].ConnectionString;
            var builder = new SqlConnectionStringBuilder(connStr);
            ProductionMode = String.Compare(builder.InitialCatalog, "botcoin", true) == 0;
        }

        public SyncBalanceService(ServiceEventLogger log)
        {
            int timeout = Int32.Parse(ConfigurationManager.AppSettings["SyncBalancesTimeoutMinutes"]);
            _dbRepo = (DbRepositoryService)log.DbRepository;
            _log = log;
            
            _timer = new Timer(timeout * 60000);
            _timer.Elapsed += OnTimerElapsed;
        }        

        private void UpdateBalances()
        {
            var accounts = new List<UserAccount>();
            _exchanges.ForEach(ex =>
            {
                try
                {
                    if (_dbRepo.CanUpdateBalances(ex))
                    {
                        accounts.Add(ex.GetBalances());
                    }
                }
                catch (WebException wex)
                {
                    _log.WriteError("Get balance. " + wex.Message, (int)ex.GetExchangeName());
                }
            });

            if (accounts.Count > 0)
                _dbRepo.UpdateBalances(accounts);
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateBalances();
        }

        public void Start(List<IExchange> exchanges)
        {
            if (ProductionMode)
            {
                _exchanges = exchanges;

                UpdateBalances();
                _timer.Start();
            }
        }

        public void Stop()
        {
            if (ProductionMode)
            {
                _timer.Stop();
            }
        }
    }
}
