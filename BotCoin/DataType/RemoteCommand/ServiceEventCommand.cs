namespace BotCoin.DataType.RemoteCommand
{
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class ServiceEventCommand : RemoteCommand
    {
        public ServiceEventType EventType { set; get; }

        public ServiceName ServiceName { set; get; }

        public string Message { set; get; }

        public string SessionId { set; get; }

        public int? ExchangeId { set; get; }
    }
}
