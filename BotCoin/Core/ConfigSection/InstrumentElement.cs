using BotCoin.DataType;
using System.Configuration;
using System;

namespace BotCoin.Core
{
    public class InstrumentElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return ((string)(base["name"])); }
        }

        public CurrencyName Instrument1
        {
            get { return (CurrencyName)Enum.Parse(typeof(CurrencyName), Name.Split('-')[0]); }
        }

        public CurrencyName Instrument2
        {
            get { return (CurrencyName)Enum.Parse(typeof(CurrencyName), Name.Split('-')[1]); }
        }
    }
}
