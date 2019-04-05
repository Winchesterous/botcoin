using System.Configuration;

namespace BotCoin.Core
{
    [ConfigurationCollection(typeof(BitmexContractElement), AddItemName = "contract")]
    public class BitmexContractsCollection : ConfigurationElementCollection
    {
        [ConfigurationProperty("code")]
        public string BitmexCode
        {
            get { return ((string)(base["code"])); }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new BitmexContractElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((BitmexContractElement)(element)).Name;
        }

        public BitmexContractElement this[int idx]
        {
            get { return (BitmexContractElement)BaseGet(idx); }
        }
    }
}
