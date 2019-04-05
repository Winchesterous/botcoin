using System.Text;

namespace BotCoin.DataType.Exchange
{
    public class KrakenOrderBook
    {
        public class OrderBook
        {
            public object[] Asks { set; get; }
            public object[] Bids { set; get; }
        }

        public class OrderBookResult
        {
            public OrderBook XXBTZUSD { set; get; }
            public OrderBook XETHZUSD { set; get; }
            public OrderBook XLTCZUSD { set; get; }
            public OrderBook BCHUSD { set; get; }
            public OrderBook XXRPZUSD { set; get; }
            public OrderBook DASHUSD { set; get; }
        }

        public string[] Error { get; set; }
        public OrderBookResult Result { get; set; }

        public string GetError()
        {
            var str = new StringBuilder();
            str.AppendLine("Kraken error.");
            for (int i = 0; i < Error.Length; i++)
            {
                str.AppendLine(Error[i]);
            }
            return str.ToString();
        }
    }
}
