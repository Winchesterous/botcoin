using System.Configuration;

namespace BotCoin.Core
{
    public class SettingsElement : ConfigurationElement
    {
        [ConfigurationProperty("instruments")]
        public InstrumentsCollection Instruments
        {
            get { return ((InstrumentsCollection)(base["instruments"])); }
        }

        [ConfigurationProperty("profitRatioStrategy")]
        public ProfitRatioStrategyElement ProfitRatioStrategy
        {
            get { return ((ProfitRatioStrategyElement)(base["profitRatioStrategy"])); }
        }

        [ConfigurationProperty("tradingStrategy")]
        public TradingStrategyElement TradingStrategy
        {
            get { return ((TradingStrategyElement)(base["tradingStrategy"])); }
        }

        [ConfigurationProperty("restScheduler")]
        public RestSchedulerElement RestScheduler
        {
            get { return ((RestSchedulerElement)(base["restScheduler"])); }
        }
    }
}
