using BotCoin.Core;
using BotCoin.Logger;
using BotCoin.Service;
using System;
using System.Configuration;
using System.Diagnostics;

namespace BotCoin.ArbitrageService
{    
    internal class SelfHost
    {
        public static string ServiceName = "ArbitrageService";

        readonly CurrencyRateService _rateService;        
        readonly MessageHandling _msgHandling;
        readonly MatchingEngine _matching;
        readonly TcpService _tcp;

        public SelfHost()
        {
            var log = new ServiceEventLogger(DataType.ServiceName.Arbitrage, new DbRepositoryService());

            _msgHandling = new MessageHandling(log, new RestScheduler(log));
            _matching    = new MatchingEngine(log, _msgHandling);
            _rateService = new CurrencyRateService();            
            _tcp         = new TcpService();

            _msgHandling.MatchingEngine = _matching;
            _msgHandling.ExchangePrices += _matching.OnExchangePrices;

            if (!EventLog.SourceExists(ServiceName))
                EventLog.CreateEventSource(ServiceName, ServiceName + "Log");
        }

        public void Start()
        {
            try
            {
                _matching.Log.SetSession();
                _matching.Log.WriteInfo("Starting");

                _msgHandling.UpdateCurrencyRates(() => _rateService.GetRates());
                _matching.Start();
                StartDbGateway();

                _matching.Log.WriteInfo("Started");
            }
            catch (Exception ex)
            {
                _matching.Log.WriteError(String.Format("[{0}] {1} {2}", ex.GetType(), ex.Message, ex.StackTrace));
            }
        }

        private void StartDbGateway()
        {
            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");           
            _tcp.StartServer(config.FindConnectionElement(DataType.ServiceName.Arbitrage.ToString()).Port);

            _tcp.StartListener(DataType.ServiceName.Arbitrage,
                bytes => _msgHandling.MessageHandler(bytes),
                (ns, bytes) =>
                {
                    if (bytes != null)
                        ns.Write(bytes, 0, bytes.Length);
                },
                data =>
                {
                    data.SessionId = _matching.Log.SessionId;
                    _msgHandling.DbRepository.WriteServiceEvent(data);
                });
        }

        public void Stop()
        {
            try
            {
                _matching.Log.WriteInfo("Stopping");

                _matching.Stop();
                StopDbGateway();

                _matching.Log.WriteInfo("Stopped");
            }
            catch (Exception ex)
            {
                _matching.Log.WriteError(String.Format("[{0}] {1} {2}", ex.GetType(), ex.Message, ex.StackTrace));
            }
        }

        private void StopDbGateway()
        {
            _tcp.StopServer();
        }
    }
}
