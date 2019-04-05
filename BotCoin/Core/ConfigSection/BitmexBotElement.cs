using System.Configuration;

namespace BotCoin.Core
{
    public class BitmexBotElement : ConfigurationElement
    {
        [ConfigurationProperty("contracts")]
        public BitmexContractsCollection Contracts
        {
            get { return ((BitmexContractsCollection)(base["contracts"])); }
        }

        [ConfigurationProperty("accountMode")]
        public string AccountMode
        {
            get { return ((string)(base["accountMode"])); }
        }

        [ConfigurationProperty("timeoutHours")]
        public int TimeoutHours
        {
            get { return ((int)(base["timeoutHours"])); }
        }
    }
}
