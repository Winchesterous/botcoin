using BotCoin.ApiClient;
using BotCoin.DataType;
using BotCoin.DataType.WebApi;
using System;

namespace BotCoin.Logger
{
    public class RestServiceEventLogger : IServiceEventLogger
    {
        readonly LogRequest _request;
        public readonly string SessionId;

        public RestServiceEventLogger(RestApiClient2 api, ServiceName servName)
        {
            _request = new LogRequest(api, servName);
            SessionId = Guid.NewGuid().ToString();
        }

        public Service.IDbRepository DbRepository
        {
            get { throw new NotSupportedException(); }
        }

        public void WriteInfo(string message, int? exchangeId = null)
        {
            _request.WriteLog(SessionId, message, ServiceEventType.Info, exchangeId);
        }

        public void WriteError(string message, int? exchangeId = null)
        {
            _request.WriteLog(SessionId, message, ServiceEventType.Fail, exchangeId);
        }

        public void WriteWarning(string message)
        {
            _request.WriteLog(SessionId, message, ServiceEventType.Warn);
        }

        public void WriteError(Exception ex, int? exchangeId = default(int?))
        {
            WriteError(String.Format("[{0}] {1} {2}", ex.GetType(), ex.Message, ex.StackTrace));
        }
    }
}
