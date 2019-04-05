using BotCoin.DataType;
using BotCoin.Logger;
using System;

namespace BotCoin.DbGatewayService
{
    internal class ServiceEventLogger : IServiceEventLogger
    {
        public readonly ServiceName Service;
        readonly DbRepository _dbRepo;
        int _sessionId;

        public ServiceEventLogger()
        {
            Service = ServiceName.DbGateway;
            _dbRepo = new DbRepository();
        }

        public int SessionId {  get { return _sessionId; } }

        public void SetSession()
        {
            if (_sessionId == 0)
                _sessionId = _dbRepo.GetServiceEventSession();
        }

        private void WriteEvent(ServiceEventType type, ServiceName service, string msg)
        {
            _dbRepo.WriteServiceEvent(new ServiceEventData { SessionId = _sessionId, EventType = type, ServiceName = Service, Message = msg });
        }

        public void WriteInfo(string message, int? exchangeId = null)
        {
            WriteEvent(ServiceEventType.Info, Service, message);
        }

        public void WriteError(string message, int? exchangeId = null)
        {
            WriteEvent(ServiceEventType.Fail, Service, message);
        }

        public void WriteWarning(string message)
        {
            WriteEvent(ServiceEventType.Warn, Service, message);
        }
    }
}
