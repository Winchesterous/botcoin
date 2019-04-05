using System.Configuration;

namespace BotCoin.Core
{
    public class ProfitRatioStrategyElement : ConfigurationElement
    {
        [ConfigurationProperty("minUsdProfit", IsRequired = true)]
        public double MinUsdProfit
        {
            get { return ((double)(base["minUsdProfit"])); }
        }

        [ConfigurationProperty("profitRatio", IsRequired = true)]
        public string ProfitRatio
        {
            get { return ((string)(base["profitRatio"])); }
        }
    }
}
