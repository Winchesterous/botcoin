using System.Configuration;

namespace BotCoin.Core
{
    public class BitmexScalperElement : ConfigurationElement
    {
        [ConfigurationProperty("contracts")]
        public BitmexContractsCollection Contracts
        {
            get { return ((BitmexContractsCollection)(base["contracts"])); }
        }

        [ConfigurationProperty("xbtBalance")]
        public double XbtBalance
        {
            get { return ((double)(base["xbtBalance"])); }
        }

        [ConfigurationProperty("riskPercent")]
        public double RiskPercent
        {
            get { return ((double)(base["riskPercent"])); }
        }

        [ConfigurationProperty("accountMode")]
        public string AccountMode
        {
            get { return ((string)(base["accountMode"])); }
        }
    }
}
