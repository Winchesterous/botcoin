using MessagePack;

namespace BotCoin.DataType.Database
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class DbExchange
    {
        public double TradeFee { set; get; }
        public int UsdMinValue { set; get; }
    }
}
