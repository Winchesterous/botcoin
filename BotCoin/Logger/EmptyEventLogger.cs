using BotCoin.ApiClient;
using BotCoin.DataType;
using System;

namespace BotCoin.Logger
{
    public class EmptyEventLogger : IServiceEventLogger
    {
        public readonly string SessionId;

        public EmptyEventLogger()
        {
            SessionId = Guid.NewGuid().ToString();
        }

        public Service.IDbRepository DbRepository
        {
            get { throw new NotSupportedException(); }
        }

        public void WriteInfo(string message, int? exchangeId = null)
        {
        }

        public void WriteError(string message, int? exchangeId = null)
        {
        }

        public void WriteWarning(string message)
        {
        }
    }
}
