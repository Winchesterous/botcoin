using BotCoin.DataType;
using System.Configuration;

namespace BotCoin.Core
{
    public class ExchangeElement : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return ((string)(base["name"])); }
        }

        [ConfigurationProperty("enable", DefaultValue = 0)]
        public int Enable
        {
            get { return ((int)(base["enable"])); }
        }

        [ConfigurationProperty("publicKey", DefaultValue = "")]
        public string PublicKey
        {
            get { return ((string)(base["publicKey"])); }
        }

        [ConfigurationProperty("userId", DefaultValue = "")]
        public string UserId
        {
            get { return ((string)(base["userId"])); }
        }

        [ConfigurationProperty("pusherKey", DefaultValue = "")]
        public string PusherKey
        {
            get { return ((string)(base["pusherKey"])); }
        }

        [ConfigurationProperty("privateKey", DefaultValue = "")]
        public string PrivateKey
        {
            get { return ((string)(base["privateKey"])); }
        }

        [ConfigurationProperty("restUrl")]
        public string RestUrl
        {
            get { return ((string)(base["restUrl"])); }
        }

        [ConfigurationProperty("wsUrl")]
        public string WsUrl
        {
            get { return ((string)(base["wsUrl"])); }
        }

        [ConfigurationProperty("currency")]
        public CurrencyName Currency
        {
            get { return ((CurrencyName)(base["currency"])); }
        }
    }
}
