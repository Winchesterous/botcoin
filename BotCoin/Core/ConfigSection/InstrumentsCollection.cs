using System.Configuration;

namespace BotCoin.Core
{
    [ConfigurationCollection(typeof(InstrumentElement), AddItemName = "pair")]
    public class InstrumentsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new InstrumentElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((InstrumentElement)(element)).Name;
        }

        public InstrumentElement this[int idx]
        {
            get { return (InstrumentElement)BaseGet(idx); }
        }

        [ConfigurationProperty("enable", IsRequired = true)]
        public int Enable
        {
            get { return ((int)(base["enable"])); }
        }

        [ConfigurationProperty("minProfitRatio", IsRequired = true)]
        public double MinProfitRatio
        {
            get { return ((double)(base["minProfitRatio"])); }
        }
    }
}
