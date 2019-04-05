namespace BotCoin.DataType.RemoteCommand
{
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class ArbitrageCommand : RemoteCommand
    {
        public enum CommandState { Start, Stop, Status};

        public ArbitrageCommand() { base.CommandType = RemoteCommandType.Arbitrage; }

        public CommandState Type { set; get; }
    }
}
