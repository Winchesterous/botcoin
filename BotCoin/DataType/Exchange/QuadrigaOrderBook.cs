namespace BotCoin.DataType.Exchange
{
    public class QuadrigaOrderBook
    {
        public long Timestamp { set; get; }
        public double[][] Asks { set; get; }
        public double[][] Bids { set; get; }
        public QuadrigaError Error { set; get; }
    }
}
