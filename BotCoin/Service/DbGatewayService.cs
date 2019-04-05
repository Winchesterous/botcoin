using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Database;
using BotCoin.DataType.Exchange;
using BotCoin.DataType.RemoteCommand;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System;

namespace BotCoin.Service
{
    public class DbGatewayService : RemoteService, IDbRepository
    {        
        public DbGatewayService(ServiceName service)
        {
            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            var element = config.FindConnectionElement(service.ToString());
#if true
            var domainName = element.DomainName;
            var ipHostInfo = Dns.Resolve(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            if (ipAddress.ToString() == "185.226.113.60")
            {
                var str = domainName.Split('.');
                domainName = string.Format("{0}-test.{1}.{2}", str[0], str[1], str[2]);
            }
#endif
            _endPoint = new IPEndPoint(GetAddressByHost(domainName), element.Port);
        }

        public string SessionId
        {
            set { throw new NotImplementedException(); }
        }

        public UserAccount GetLastBalances(ExchangeName exchange)
        {
            return SendReceiveMessage<DbBalanceCommand, UserAccount>(
                () => new DbBalanceCommand { Exchange = exchange },
                () => RemoteCommandType.DbBalance
                );
        }

        public double GetCurrencyRate(CurrencyName currency)
        {
            return SendReceiveMessage<CurrencyRateCommand, double>(
                () => new CurrencyRateCommand { Currency = currency },
                () => RemoteCommandType.CurrencyRate
                );
        }

        public DbExchange GetExchangeInfo(ExchangeName exchange)
        {
            return SendReceiveMessage<ExchangeInfoCommand, DbExchange>(
                 () => new ExchangeInfoCommand { Exchange = exchange },
                 () => RemoteCommandType.DbExchangeInfo
                 );
        }

        public ExchangeSettingsData[] GetExchangeSettings()
        {
            return SendReceiveMessage<byte[], ExchangeSettingsData[]>(
                () => new byte[1],
                () => RemoteCommandType.ExchangeSettings
                );
        }

        public Dictionary<string, bool> GetRestEnabledExchanges()
        {
            return SendReceiveMessage<byte[], Dictionary<string, bool>>(
                () => new byte[1],
                () => RemoteCommandType.RestEnabledExchanges
                );
        }

        public Dictionary<string, bool> GetWebSocketEnabledExchanges()
        {
            return SendReceiveMessage<byte[], Dictionary<string, bool>>(
                () => new byte[1],
                () => RemoteCommandType.WsEnabledExchanges
                );
        }

        public void SendPrices(ExchangePricesEventArgs args)
        {
            SendMessage<ExchangePricesCommand>(
                () => new ExchangePricesCommand { CommandType = RemoteCommandType.ExchangePrices, Prices = args },
                () => RemoteCommandType.ExchangePrices
                );
        }

        public void WriteServiceEvent(string sessionId, ServiceEventData data)
        {
            WriteServiceEvent(sessionId, data.EventType, data.ServiceName, data.Message);
        }

        public void WriteServiceEvent(string sessionId, ServiceEventType type, ServiceName name, string msg, int? exchangeId = null)
        {
            var cmd = new ServiceEventCommand
            {
                CommandType = RemoteCommandType.DbWriteServiceEvent,
                SessionId = sessionId,
                ServiceName = name,
                EventType = type,
                Message = msg,
                ExchangeId = exchangeId
            };

            SendMessage<ServiceEventCommand>(
                    () => cmd,
                    () => RemoteCommandType.DbWriteServiceEvent
                    );
        }

        public void RunRestScheduler(CurrencyName instrument)
        {
            SendMessage<RestSchedulerCommand>(
                    () => new RestSchedulerCommand { Instrument = instrument, Run = true },
                    () => RemoteCommandType.RunRestScheduler
                    );
        }

        public void StopRestScheduler(CurrencyName instrument)
        {
            SendMessage<RestSchedulerCommand>(
                    () => new RestSchedulerCommand { Instrument = instrument },
                    () => RemoteCommandType.RunRestScheduler
                    );
        }        
    }
}
