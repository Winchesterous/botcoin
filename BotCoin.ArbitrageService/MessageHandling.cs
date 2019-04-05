using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Database;
using BotCoin.DataType.Exchange;
using BotCoin.DataType.RemoteCommand;
using BotCoin.Logger;
using BotCoin.Service;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace BotCoin.ArbitrageService
{
    internal class MessageHandling
    {
        public readonly DbRepositoryService DbRepository;
        
        readonly ServiceEventLogger _log;
        readonly RestScheduler _restScheduler;
        object _obj = new object();

        public event EventHandler<ExchangePricesEventArgs> ExchangePrices;

        public MessageHandling(ServiceEventLogger log, RestScheduler scheduler)
        {
            DbRepository   = new DbRepositoryService();
            _restScheduler = scheduler;
            _log           = log;
        }

        internal MatchingEngine MatchingEngine { set; get; }

        public byte[] MessageHandler(byte[] bytes)
        {
            byte[] response = null;
            try
            {
                var segment = new ArraySegment<byte>(bytes, 1, bytes.Length - 1);
                response = HandleMessage(segment, (RemoteCommandType)bytes[0]);
            }
            catch (Exception ex)
            {
                _log.WriteError(String.Format("{0} {1}", (RemoteCommandType)bytes[0], ex.Message));
            }
            return response;
        }
        
        private byte[] HandleMessage(ArraySegment<byte> bytes, RemoteCommandType cmdType)
        {
            byte[] response = null;

            switch (cmdType)
            {
            case RemoteCommandType.CurrencyRate:
                {
                    var cmd = MessagePackSerializer.Deserialize<CurrencyRateCommand>(bytes);
                    var currency = DbRepository.GetCurrencyRate(cmd.Currency);
                    response = MessagePackSerializer.Serialize<double>(currency);
                    break;
                }
            case RemoteCommandType.DbExchangeInfo:
                {
                    var cmd = MessagePackSerializer.Deserialize<ExchangeInfoCommand>(bytes);
                    var dbInfo = DbRepository.GetExchangeInfo(cmd.Exchange);
                    response = MessagePackSerializer.Serialize<DbExchange>(dbInfo);
                    break;
                }
            case RemoteCommandType.DbBalance:
                {
                    var cmd = MessagePackSerializer.Deserialize<DbBalanceCommand>(bytes);
                    response = MessagePackSerializer.Serialize<UserAccount>(DbRepository.GetLastBalances(cmd.Exchange));
                    break;
                }
            case RemoteCommandType.DbServiceEventSession:
                {
                    var cmd = MessagePackSerializer.Deserialize<ServiceEventCommand>(bytes);
                    response = MessagePackSerializer.Serialize<int>(DbRepository.GetServiceEventSession());
                    break;
                }
            case RemoteCommandType.DbWriteServiceEvent:
                {
                    var cmd = MessagePackSerializer.Deserialize<ServiceEventCommand>(bytes);
                    DbRepository.WriteServiceEvent(cmd.SessionId, cmd.EventType, cmd.ServiceName, cmd.Message, cmd.ExchangeId);
                    break;
                }
            case RemoteCommandType.RunRestScheduler:
                {
                    var cmd = MessagePackSerializer.Deserialize<RestSchedulerCommand>(bytes);
                    if (cmd.Run)
                        _restScheduler.Connect(cmd.Instrument);
                    else
                        _restScheduler.Disconnect(cmd.Instrument);
                    break;
                }
            case RemoteCommandType.RestEnabledExchanges:
                {
                    response = MessagePackSerializer.Serialize<Dictionary<string, bool>>(GetRestEnabledExchanges());
                    break;
                }
            case RemoteCommandType.WsEnabledExchanges:
                {
                    response = MessagePackSerializer.Serialize<Dictionary<string, bool>>(GetWebSocketEnabledExchanges());
                    break;
                }
            case RemoteCommandType.ExchangeSettings:
                {
                    response = MessagePackSerializer.Serialize<ExchangeSettingsData[]>(GetExchangeSettings());
                    break;
                }
            case RemoteCommandType.ExchangePrices:
                {
                    var cmd = MessagePackSerializer.Deserialize<ExchangePricesCommand>(bytes);
                    lock (this) ExchangePrices(this, cmd.Prices);
                    break;
                }
            case RemoteCommandType.Arbitrage:
                {
                    var cmd = MessagePackSerializer.Deserialize<ArbitrageCommand>(bytes);
                    switch (cmd.Type)
                    {
                    case ArbitrageCommand.CommandState.Start:
                        MatchingEngine.SingleInstrumentArbitrage();
                        response = MessagePackSerializer.Serialize<bool>(true);
                        break;
                    case ArbitrageCommand.CommandState.Stop:
                        MatchingEngine.StopMatching();
                        response = MessagePackSerializer.Serialize<bool>(true);
                        break;
                    case ArbitrageCommand.CommandState.Status:
                        response = MessagePackSerializer.Serialize<string>(MatchingEngine.GetState());
                        break;
                    }
                    break;
                }
            }
            return response;
        }

        public Dictionary<string, bool> GetRestEnabledExchanges()
        {
            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            return config.GetRestEnabledExchanges();
        }

        public Dictionary<string,bool> GetWebSocketEnabledExchanges()
        {
            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            return config.GetWebSocketEnabledExchanges();
        }

        public void UpdateCurrencyRates(Func<CurrencyRate[]> getRates)
        {
            if (DbRepository.CanInsertCurrencies())
                DbRepository.SaveCurrencies(getRates());
        }

        public ExchangeSettingsData[] GetExchangeSettings()
        {
            return DbRepository.GetExchangeSettings();
        }
    }
}
