using System;

namespace BotCoin.DataType
{
    public class ServiceEventData
    {
        public ServiceEventData()
        {
            Timestamp = DateTime.UtcNow;
        }

        public DateTime Timestamp { private set; get; }

        public ServiceEventType EventType { set; get; }

        public ServiceName ServiceName { set; get; }

        public string Message { set; get; }

        public string SessionId { set; get; }

        public int? ExchangeId { set; get; }
    }
}
