using MessagePack;

namespace BotCoin.DataType
{
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class UserAccount
    {
        public UserAccount() { CurrencyRate = 1; }

        public ExchangeName Exchange { set; get; }
        public double InitUsdBalance { set; get; }        
        public double CurrencyRate { set; get; }
        public double Balance { set; get; }
        public double BtcBalance { set; get; }
        public double? BchBalance { set; get; }
        public double? LtcBalance { set; get; }
        public double? EthBalance { set; get; }
        public double? XrpBalance { set; get; }
        public double? DashBalance { set; get; }

        [IgnoreMember]
        public double UsdBalance
        {
            get { return Balance / CurrencyRate; }
        }
    }
}
