using BotCoin.DataType.Exchange;
using System;

namespace BotCoin.DataType
{   
    public class TradeEventArgs : EventArgs
    {
        public static DateTime StartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public TradeEventArgs()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public TradeEventArgs(BitstampTrade data, string channel) : this()
        {
            TradeId       = data.Id.ToString();
            Quantity      = data.Amount;
            Price         = data.Price;            
            TradeType     = data.TradeType;
            BuyerOrderId  = data.BuyerOrderId.ToString();
            SellerOrderId = data.SellerOrderId.ToString();
            Timestamp     = StartTime.AddSeconds(data.Timestamp);
            Exchange      = ExchangeName.Bitstamp;

            string pair = "btcusd";
            int    len  = "live_trades".Length;
            string s1   = channel.Substring(len, channel.Length - len);

            if (s1.StartsWith("_"))
                pair = s1.Substring(1, s1.Length - 1);

            var symbol = OrderBookEventArgs.ParseSymbol(pair);
            Instrument1 = symbol[0];
            Instrument2 = symbol[1];
        }

        public TradeEventArgs(BinanceOrderBook orderBook) : this()
        {
            var data = orderBook.Data;
            var pair = orderBook.Stream.Split('@')[0];
            var symbol = OrderBookEventArgs.ParseSymbol(pair);

            Quantity      = data.Amount;
            Price         = data.Price;
            Instrument1   = symbol[0];
            Instrument2   = symbol[1];
            BuyerOrderId  = data.BuyerOrderId;
            SellerOrderId = data.SellerOrderId;
            CreatedAt     = StartTime.AddMilliseconds(data.EventTime);
            Timestamp     = StartTime.AddMilliseconds(data.TradeTime);
            Exchange      = ExchangeName.Binance;
            TradeId       = data.TradeId;
            IsBuyerMarketMaker = data.IsBuyerMarketMaker;
        }

        public TradeEventArgs(OkExTradeItem data) : this()
        {
            TradeId     = data.TradeId.ToString();
            Price       = data.Price;
            Quantity    = data.Amount;
            CreatedAt   = data.CreatedAt;
            Timestamp   = data.Timespan;
            TradeType   = data.TradeType;
            Instrument1 = data.Instrument1;
            Instrument2 = data.Instrument2;
            Exchange    = ExchangeName.OkEx; 
        }

        public TradeEventArgs(BitmexEventArgs args) : this()
        {
            BtxTrades = args.BtxTrades;
            Exchange = ExchangeName.Bitmex;
        }

        public DateTime CreatedAt { private set; get; }
        public DateTime Timestamp { set; get; }
        public CurrencyName Instrument1 { set; get; }
        public CurrencyName Instrument2 { set; get; }
        public ExchangeName Exchange { set; get; }
        public long OrderId { set; get; }
        public string BuyerOrderId { set; get; }
        public string SellerOrderId { set; get; }
        public string TradeId { set; get; }
        public double Price { set; get; }
        public double Quantity { set; get; }
        public string TradeType { set; get; }
        public bool? IsBuyerMarketMaker { set; get; }
        public BitmexTradeData[] BtxTrades { set; get; }
    }
}
