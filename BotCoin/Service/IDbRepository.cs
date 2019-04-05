using BotCoin.DataType;
using BotCoin.DataType.Database;

namespace BotCoin.Service
{
    public interface IDbRepository
    {
        void WriteServiceEvent(string sessionId, ServiceEventType type, ServiceName name, string msg, int? exchangeId = null);

        double GetCurrencyRate(CurrencyName currency);

        DbExchange GetExchangeInfo(ExchangeName ex);

        UserAccount GetLastBalances(ExchangeName exchange);

        string SessionId { set; }
    }
}
