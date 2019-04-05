using System.Configuration;

namespace BotCoin.Core
{
    [ConfigurationCollection(typeof(ExchangeElement), AddItemName = "exchange")]
    public class ExchangesCollection : ConfigurationElementCollection
    {
        [ConfigurationProperty("instrument")]
        public string Instrument
        {
            get { return ((string)(base["instrument"])); }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ExchangeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExchangeElement)(element)).Name;
        }

        public ExchangeElement this[int idx]
        {
            get { return (ExchangeElement)BaseGet(idx); }
        }
    }
}
