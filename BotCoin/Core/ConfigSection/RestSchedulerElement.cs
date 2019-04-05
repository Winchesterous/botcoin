using System.Configuration;

namespace BotCoin.Core
{
    public class RestSchedulerElement : ConfigurationElement
    {
        [ConfigurationProperty("timeoutSeconds", IsKey = true, IsRequired = true)]
        public int TimeoutSeconds
        {
            get { return ((int)(base["timeoutSeconds"])); }
        }
    }
}
