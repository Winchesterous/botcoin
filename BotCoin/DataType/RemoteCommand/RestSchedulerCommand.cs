namespace BotCoin.DataType.RemoteCommand
{
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class RestSchedulerCommand : RemoteCommand
    {
        public CurrencyName Instrument { set; get; }

        public bool Run { set; get; }
    }
}
