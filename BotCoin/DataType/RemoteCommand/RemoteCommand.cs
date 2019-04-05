using MessagePack;

namespace BotCoin.DataType.RemoteCommand
{
    public enum RemoteCommandType
    {
        Undefined,
        Arbitrage,
        CurrencyRate,
        DbServiceEventSession,
        DbWriteServiceEvent,
        DbExchangeInfo,
        DbBalance,
        RunRestScheduler,
        StopRestScheduler,
        RestEnabledExchanges,
        WsEnabledExchanges,
        ExchangeSettings,
        ExchangePrices
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class RemoteCommand
    {
        public RemoteCommandType CommandType { set; get; }
    }
}
