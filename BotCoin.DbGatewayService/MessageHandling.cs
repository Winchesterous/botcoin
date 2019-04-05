using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Database;
using BotCoin.DataType.Exchange;
using BotCoin.DataType.RemoteCommand;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace BotCoin.DbGatewayService
{
    internal class MessageHandling
    {
        readonly ServiceEventLogger _log;
        readonly ExchangePricesPublisher _restScheduler;
        readonly DbRepository _dbRepo;
        readonly bool _logMatchingData;
        readonly object _obj;

        public MessageHandling(DbRepository dbRepo, ServiceEventLogger log, ExchangePricesPublisher scheduler)
        {
            _logMatchingData = Int32.Parse(ConfigurationManager.AppSettings["LogMatchingData"]) == 1;

            _restScheduler = scheduler;
            _dbRepo  = dbRepo;
            _log     = log;
            _obj     = new object();
        }

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
                    var currency = _dbRepo.GetCurrencyRate(cmd.Currency);
                    response = MessagePackSerializer.Serialize<double>(currency);
                    break;
                }
            case RemoteCommandType.DbExchangeInfo:
                {
                    var cmd = MessagePackSerializer.Deserialize<ExchangeInfoCommand>(bytes);
                    var dbInfo = _dbRepo.GetExchangeInfo(cmd.Exchange);
                    response = MessagePackSerializer.Serialize<DbExchange>(dbInfo);
                    break;
                }
            case RemoteCommandType.DbBalance:
                {
                    var cmd = MessagePackSerializer.Deserialize<DbBalanceCommand>(bytes);
                    var db = _dbRepo.GetLastBalances(cmd.Exchange);
                    response = MessagePackSerializer.Serialize<DbAccount>(db);
                    break;
                }
            case RemoteCommandType.DbSyncBalance:
                {
                    var cmd = MessagePackSerializer.Deserialize<DbSyncBalancesCommand>(bytes);
                    cmd = _dbRepo.SyncBalances(cmd);
                    response = MessagePackSerializer.Serialize<DbSyncBalancesCommand>(cmd);
                    break;
                }
            case RemoteCommandType.DbServiceEventSession:
                {
                    var cmd = MessagePackSerializer.Deserialize<ServiceEventCommand>(bytes);
                    int id = 0;
                    lock (_obj) id = _dbRepo.GetServiceEventSession();
                    response = MessagePackSerializer.Serialize<int>(id);
                    break;
                }
            case RemoteCommandType.DbWriteServiceEvent:
                {
                    var cmd = MessagePackSerializer.Deserialize<ServiceEventCommand>(bytes);
                    _dbRepo.WriteServiceEvent(new ServiceEventData
                    {
                        ServiceName = cmd.ServiceName,
                        SessionId = cmd.SessionId,
                        EventType = cmd.EventType,
                        Message = cmd.Message,
                        ExchangeId = cmd.ExchangeId
                    });
                    break;
                }
            case RemoteCommandType.MatchingData:
                {
                    var cmd = MessagePackSerializer.Deserialize<MatchingDataCommand>(bytes);
                    _dbRepo.SaveTransaction(cmd.Data, ex => _log.WriteError(ex.Message));
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
                    var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
                    var enabledExchanges = config.GetRestEnabledExchanges();
                    response = MessagePackSerializer.Serialize<Dictionary<string, bool>>(enabledExchanges);
                    break;
                }
            case RemoteCommandType.WsEnabledExchanges:
                {
                    var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
                    var enabledExchanges = config.GetWebSocketEnabledExchanges();
                    response = MessagePackSerializer.Serialize<Dictionary<string, bool>>(enabledExchanges);
                    break;
                }
            case RemoteCommandType.ExchangeSettings:
                {
                    var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
                    var settings = new List<ExchangeSettingsData>();
                    foreach (ExchangeElement setting in config.ExchangeSettings)
                    {
                        settings.Add(new ExchangeSettingsData
                        {
                            Exchange = (ExchangeName)Enum.Parse(typeof(ExchangeName), setting.Name),
                            Currency = setting.Currency,
                            RestUrl = setting.RestUrl,
                            WebsocketUrl = setting.WsUrl,
                            PublicKey = setting.PublicKey,
                            PrivateKey = setting.PrivateKey,
                            UserId = setting.UserId
                        });
                    }
                    response = MessagePackSerializer.Serialize<ExchangeSettingsData[]>(settings.ToArray());
                    break;
                }
            case RemoteCommandType.ExchangePrices:
                {
                    if (_logMatchingData)
                    {
                        var cmd = MessagePackSerializer.Deserialize<ExchangePricesCommand>(bytes);
                        _dbRepo.SaveOrderBook(cmd.Prices);
                    }
                    break;
                }
            case RemoteCommandType.CanResetBalances:
                {
                    var cmd = MessagePackSerializer.Deserialize<CanResetBalancesCommand>(bytes);
                    var result = _dbRepo.CanResetBalances(cmd.Exchanges);
                    response = MessagePackSerializer.Serialize<Tuple<bool, DbAccount[]>>(result);
                    break;
                }
            }
            return response;
        }
    }
}
