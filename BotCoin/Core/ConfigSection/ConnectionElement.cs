using System.Configuration;

namespace BotCoin.Core
{
    public class ConnectionElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return ((string)(base["name"])); }
        }

        [ConfigurationProperty("domainName", IsKey = true)]
        public string DomainName
        {
            get { return ((string)(base["domainName"])); }
        }

        [ConfigurationProperty("port")]
        public int Port
        {
            get { return ((int)(base["port"])); }
        }

        [ConfigurationProperty("url")]
        public string Url
        {
            get { return ((string)(base["url"])); }
        }

        [ConfigurationProperty("timeoutMinute")]
        public int TimoutMinute
        {
            get { return ((int)(base["timeoutMinute"])); }
        }
    }
}
