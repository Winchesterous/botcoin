using System;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Reflection;
using BotCoin.Exchange;

namespace BotCoin.Core
{
    public static class Utils
    {
        public static void MapExchangeAttributes(Action<string, Type> action)
        {
            MapExchangeAttributes((attrName, isWebsocket, type) => action(attrName, type));
        }

        public static void MapExchangeAttributes(Action<string, bool> action)
        {
            MapExchangeAttributes((attrName, isWebsocket, type) => action(attrName, isWebsocket));
        }

        public static void MapExchangeAttributes(Action<string, bool, Type> action)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (Type type in Assembly.LoadFile(Path.Combine(path, "BotCoin.dll")).GetTypes())
            {
                var attrs = type.GetCustomAttributes(typeof(ExchangeAttribute), true);
                if (attrs.Length == 1)
                {
                    var attributeName = ((ExchangeAttribute)attrs[0]).Name;
                    var isWebsocket = ((ExchangeAttribute)attrs[0]).IsWebsocket;
                    action(attributeName, isWebsocket, type);
                }
            }
        }

        public static void ThrowIf(bool expression, string message, params object[] args)
        {
            if (expression)
                throw new InvalidOperationException(String.Format(message, args));
        }

        public static double Round(double value, IExchange ex, bool isCurrency = true)
        {
            return Math.Round(value, isCurrency ? ex.CountryCurrencyDecimal : ex.CryptoCurrencyDecimal);
        }
    }
}
