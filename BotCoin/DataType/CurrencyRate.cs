using System;

namespace BotCoin.DataType
{
    public class ForeignCurrencyRate
    {
        public class Items
        {
            public double CAD { set; get; }
            public double AUD { set; get; }
            public double JPY { set; get; }
            public double MXN { set; get; }
            public double PLN { set; get; }
            public double ZAR { set; get; }
            public double THB { set; get; }
        }

        public string Base { set; get; }
        public DateTime Date { set; get; }
        public Items Rates { set; get; }
    }

    public class UahCurrencyRate
    {
        public string Cc { set; get; }
        public double Rate { set; get; }
        public string ExchangeDate { set; get; }
    }

    public class CurrencyRate
    {
        public CurrencyRate()
        {
            ExchangeDate = DateTime.UtcNow;
        }

        public DateTime ExchangeDate { private set; get; }
        public CurrencyName Currency { set; get; }
        public double Rate { set; get; }
    }
}
