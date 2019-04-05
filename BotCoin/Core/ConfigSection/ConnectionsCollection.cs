using System.Configuration;

namespace BotCoin.Core
{
    [ConfigurationCollection(typeof(ConnectionElement), AddItemName = "connection")]
    public class ConnectionsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ConnectionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ConnectionElement)(element)).Name;
        }

        public ConnectionElement GetElement(string key)
        {
            foreach (ConnectionElement attr in this)
            {
                if (attr.Name == key)
                    return attr;
            }
            return null;
        }
    }
}
