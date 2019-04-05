using System;

namespace BotCoin.DataType.Database
{
    public class DbCurrencyRate
    {
        public double Rate { set; get; }
        public DateTime RateDate { set; get; }
        public short CurrencyId { set; get; }

        public CurrencyName Currency
        {
            get { return (CurrencyName)CurrencyId; }
        }
    }
}
