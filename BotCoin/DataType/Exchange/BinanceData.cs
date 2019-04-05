namespace BotCoin.DataType.Exchange
{
    public class BinanceAccount
    {
        public class BinanceBalance
        {
            public string Asset { set; get; }
            public double Free { set; get; }
            public double Locked { set; get; }
        }
        public BinanceBalance[] Balances { set; get; }
        public double MakerCommission { set; get; }
        public double takerCommission { set; get; }
        public double buyerCommission { set; get; }
        public double sellerCommission { set; get; }
        public bool CanTrade { set; get; }
        public bool CanWithdraw { set; get; }
        public bool CanDeposit { set; get; }
        public long UpdateTime { set; get; }
    }
}
