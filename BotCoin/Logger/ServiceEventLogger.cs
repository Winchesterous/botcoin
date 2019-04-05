using BotCoin.DataType;
using BotCoin.Service;
using System;

namespace BotCoin.Logger
{
    public class ServiceEventLogger : IServiceEventLogger
    {
        public readonly ServiceName Service;                
        public readonly string SessionId;
        private readonly IDbRepository _dbRepo;

        public ServiceEventLogger(IDbRepository dbRepo) : this(ServiceName.Undefined, dbRepo)
        {
        }

        public ServiceEventLogger(ServiceName serviceName, IDbRepository dbRepo)
        {
            SessionId = Guid.NewGuid().ToString();
            _dbRepo   = dbRepo;
            Service   = serviceName;

            _dbRepo.SessionId = SessionId;
        }

        public IDbRepository DbRepository
        {
            get { return _dbRepo; }
        }

        private void WriteEvent(ServiceEventType type, ServiceName service, string msg, int? exchangeId = null)
        {
            _dbRepo.WriteServiceEvent(SessionId, type, service, msg, exchangeId);
        }

        public void WriteInfo(string message, int? exchangeId = null)
        {
            WriteEvent(ServiceEventType.Info, Service, message, exchangeId);
        }

        public void WriteInfo(string sessionId, string message, ServiceName service, int? exchangeId = null)
        {
            _dbRepo.WriteServiceEvent(sessionId, ServiceEventType.Info, service, message, exchangeId);
        }

        public void WriteError(string message, int? exchangeId = null)
        {
            WriteEvent(ServiceEventType.Fail, Service, message, exchangeId);
        }

        public void WriteError(string sessionId, string message, ServiceName service, int? exchangeId = null)
        {
            _dbRepo.WriteServiceEvent(sessionId, ServiceEventType.Fail, service, message, exchangeId);
        }

        public void WriteWarning(string message)
        {
            WriteEvent(ServiceEventType.Warn, Service, message);
        }

        public void WriteWarning(string sessionId, string message, ServiceName service, int? exchangeId = null)
        {
            _dbRepo.WriteServiceEvent(sessionId, ServiceEventType.Warn, service, message, exchangeId);
        }

        public void WriteError(Exception ex, int? exchangeId = default(int?))
        {
            WriteError(String.Format("[{0}] {1} {2}", ex.GetType(), ex.Message, ex.StackTrace));
        }
    }
}
