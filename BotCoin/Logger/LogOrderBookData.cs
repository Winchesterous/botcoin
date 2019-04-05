using BotCoin.DataType;
using System;

namespace BotCoin.Logger
{
    public class LogOrderBookData
    {
        public double[][] Bids { set; get; }
        public double[][] Asks { set; get; }
        public DateTime CreatedAt { set; get; }
        public DateTime? Timestamp { set; get; }
        public ExchangeName Exchange { set; get; }
        public CurrencyName Currency { set; get; }
        public CurrencyName CryptoCurrency { set; get; }
        public string ErrorMessage { set; get; }
        public long? OrderId { set; get; }
        public bool? IsDeleted { set; get; }
    }
}
