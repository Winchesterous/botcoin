using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Logger;
using MessagePack;
using System;
using System.Configuration;

namespace BotCoin.Service
{
    internal class RestExchangeListener
    {
        readonly DbGatewayService _dbGateway;
        readonly ServiceEventLogger _log;
        readonly CurrencyName _instrument;
        readonly TcpService _tcp;

        public event EventHandler<CurrencyName> ExchangePricesUpdate;

        public RestExchangeListener(DbGatewayService dbGateway, ServiceEventLogger log)
        {
            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            _instrument = (CurrencyName)Enum.Parse(typeof(CurrencyName), config.RestExchanges.Instrument.Split(',')[0].Trim().ToUpper());

            _dbGateway = dbGateway;
            _tcp       = new TcpService();
            _log       = log;
        }

        public void Start()
        {
            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            _tcp.StartServer(config.FindConnectionElement("RestScheduler").Port);

            _dbGateway.RunRestScheduler(_instrument);

            _tcp.StartListener(_log.Service,
                bytes =>
                {
                    var currency = MessagePackSerializer.Deserialize<CurrencyName>(bytes);
                    ExchangePricesUpdate(this, currency);
                    return null;
                },
                null,
                data => _dbGateway.WriteServiceEvent(_log.SessionId, data)
                );
        }

        public void Stop()
        {
            _dbGateway.StopRestScheduler(_instrument);
            _tcp.StopServer();
        }
    }
}
