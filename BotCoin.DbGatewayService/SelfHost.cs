using BotCoin.Core;
using BotCoin.Service;
using System.Configuration;
using System;

namespace BotCoin.DbGatewayService
{
    internal class SelfHost
    {
        private ExchangePricesPublisher _pricePublisher;
        readonly MessageHandling _msgHandling;
        readonly ServiceEventLogger _log;
        readonly DbRepository _dbRepo;
        readonly TcpService _tcp;

        public SelfHost()
        {            
            _log            = new ServiceEventLogger();
            _tcp            = new TcpService();
            _dbRepo         = new DbRepository();
            _pricePublisher = new ExchangePricesPublisher(_log);
            _msgHandling    = new MessageHandling(_dbRepo, _log, _pricePublisher);
        }

        public void Start()
        {
            _log.SetSession();
            _log.WriteInfo("Starting");

            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            _tcp.StartServer("127.0.0.1", config.FindConnectionElement("DbGateway").Port);

            _tcp.StartListener(_log.Service,
                bytes => _msgHandling.MessageHandler(bytes),
                (ns, bytes) =>
                {
                    if (bytes != null)
                        ns.Write(bytes, 0, bytes.Length);
                },
                data =>
                {
                    data.SessionId = _log.SessionId;
                    _dbRepo.WriteServiceEvent(data);
                });

            _log.WriteInfo("Started");
        }

        public void Stop()
        {
            _pricePublisher.Reset();
            _tcp.StopServer();

            _log.WriteInfo("Stopped");
        }
    }
}
