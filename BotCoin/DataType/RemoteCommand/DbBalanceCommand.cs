namespace BotCoin.DataType.RemoteCommand
{
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class DbBalanceCommand : RemoteCommand
    {
        public DbBalanceCommand() { CommandType = RemoteCommandType.DbBalance; }
        public ExchangeName Exchange { set; get; }
    }
}
