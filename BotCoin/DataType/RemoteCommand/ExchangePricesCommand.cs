namespace BotCoin.DataType.RemoteCommand
{
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class ExchangePricesCommand : RemoteCommand
    {
        public ExchangePricesCommand() { CommandType = RemoteCommandType.ExchangePrices; }
        
        public ExchangePricesEventArgs Prices { set; get; }
    }
}
