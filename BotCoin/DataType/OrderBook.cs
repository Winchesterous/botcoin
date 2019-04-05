namespace BotCoin.DataType
{
    public class OrderBook
    {
        public class OrderBookError
        {
            public string Code { set; get; }
            public string Message { set; get; }
        }

        public OrderBook()
        {
            Currency = CurrencyName.USD;
        }

        public OrderBookError Error { set; get; }        
        public Order[] Bids { set; get; }
        public Order[] Asks { set; get; }
        public CurrencyName Currency { set; get; }
        public long? Timestamp { set; get; }
    }
}
