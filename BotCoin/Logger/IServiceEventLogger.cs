using System;

namespace BotCoin.Logger
{
    public interface IServiceEventLogger
    {
        BotCoin.Service.IDbRepository DbRepository { get; }

        void WriteInfo(string message, int? exchangeId = null);

        void WriteError(string message, int? exchangeId = null);

        void WriteError(Exception ex, int? exchangeId = null);

        void WriteWarning(string message);
    }
}
