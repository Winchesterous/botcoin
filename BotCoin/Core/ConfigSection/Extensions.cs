using System;

namespace BotCoin.Core
{
    public static class Extensions
    {
        public static ConnectionElement FindConnectionElement(this BotcoinConfigSection config, string name)
        {
            foreach (ConnectionElement element in config.Connections)
            {
                if (String.Compare(element.Name, name, false) == 0)
                    return element;
            }
            throw new InvalidOperationException(String.Format("Connection element '{0}' hasn't been found", name));
        }

        public static ExchangeElement FindExchangeElement(this ExchangesCollection config, string name)
        {
            foreach (ExchangeElement element in config)
            {
                if (String.Compare(element.Name, name, false) == 0)
                    return element;
            }
            return null;
        }
    }
}
