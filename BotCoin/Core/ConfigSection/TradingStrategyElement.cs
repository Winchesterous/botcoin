using System.Configuration;

namespace BotCoin.Core
{
    public class TradingStrategyElement : ConfigurationElement
    {
        [ConfigurationProperty("lowPriceRate")]
        public double LowPriceRate
        {
            get { return ((double)(base["lowPriceRate"])); }
        }

        [ConfigurationProperty("minMatchingRatio")]
        public double MinMatchingRatio
        {
            get { return ((double)(base["minMatchingRatio"])); }
        }

        [ConfigurationProperty("avgBtcAmount")]
        public double AvgBtcAmount
        {
            get { return ((double)(base["avgBtcAmount"])); }
        }

        [ConfigurationProperty("avgLtcAmount")]
        public double AvgLtcAmount
        {
            get { return ((double)(base["avgLtcAmount"])); }
        }

        [ConfigurationProperty("avgDashAmount")]
        public double AvgDashAmount
        {
            get { return ((double)(base["avgDashAmount"])); }
        }

        [ConfigurationProperty("avgBchAmount")]
        public double AvgBchAmount
        {
            get { return ((double)(base["avgBchAmount"])); }
        }

        [ConfigurationProperty("avgEthAmount")]
        public double AvgEthAmount
        {
            get { return ((double)(base["avgEthAmount"])); }
        }

        [ConfigurationProperty("avgXrpAmount")]
        public double AvgXrpAmount
        {
            get { return ((double)(base["avgXrpAmount"])); }
        }
    }
}
