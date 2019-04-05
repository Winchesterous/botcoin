namespace BotCoin.DataType.Exchange
{
    public class BtcTradeOrderBook
    {
        public class BtcTradeOrderBookItem
        {
            public double price { set; get; }
            public double currency_trade { set; get; }
            public double currency_base { set; get; }
        }

        public double orders_sum { set; get; }
        public double max_price { set; get; }
        public double min_price { set; get; }
        public BtcTradeOrderBookItem[] list { set; get; }
    }
}
