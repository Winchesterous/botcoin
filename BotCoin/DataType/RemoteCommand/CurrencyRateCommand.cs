namespace BotCoin.DataType.RemoteCommand
{
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class CurrencyRateCommand : RemoteCommand
    {
        public CurrencyRateCommand() { CommandType = RemoteCommandType.DbExchangeInfo; }

        public CurrencyName Currency { set; get; }
    }
}
