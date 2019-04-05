using System.Configuration;

namespace BotCoin.Core
{
    public class BitmexContractElement : ConfigurationElement
    {
        [ConfigurationProperty("name")]
        public string Name
        {
            get { return ((string)(base["name"])); }
        }

        [ConfigurationProperty("symbol")]
        public string Symbol
        {
            get { return ((string)(base["symbol"])); }
        }

        [ConfigurationProperty("priceSlip")]
        public int PriceSlip
        {
            get { return ((int)(base["priceSlip"])); }
        }

        [ConfigurationProperty("stopSlip")]
        public int StopSlip
        {
            get { return ((int)(base["stopSlip"])); }
        }
            
    }
}
