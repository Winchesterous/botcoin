using System;
using System.Collections.Generic;

namespace BotCoin.DataType.Database
{
    public class DbPriceLevel
    {
        public DbPriceLevel()
        {
            ConfirmedDates = new List<DateTime>();
        }

        public string Id { set; get; }
        public double Price { set; get; }
        public bool IsActual { set; get; }
        public DateTime? Date2 { set; get; }
        public DateTime? LevelDate { set; get; }
        public List<DateTime> ConfirmedDates { private set; get; }

        public string PriceStr
        {
            set { _str = value; }
            get { return _str == null ? Price.ToString("0.00") : _str; }
        }        
        string _str;
    }
}
