namespace BotCoin.DataType.RemoteCommand
{
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class ExchangeInfoCommand : RemoteCommand
    {
        public ExchangeInfoCommand() { CommandType = RemoteCommandType.DbExchangeInfo; }

        public ExchangeName Exchange { set; get; }
    }
}
