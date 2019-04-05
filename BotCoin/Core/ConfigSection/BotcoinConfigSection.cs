using BotCoin.DataType;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace BotCoin.Core
{
    public class BotcoinConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("bitmexScalper")]
        public BitmexScalperElement BitmexScalper
        {
            get { return ((BitmexScalperElement)(base["bitmexScalper"])); }
        }

        [ConfigurationProperty("bitmexBot")]
        public BitmexBotElement BitmexBot
        {
            get { return ((BitmexBotElement)(base["bitmexBot"])); }
        }

        [ConfigurationProperty("restExchanges")]
        public ExchangesCollection RestExchanges
        {
            get { return ((ExchangesCollection)(base["restExchanges"])); }
        }

        [ConfigurationProperty("websocketExchanges")]
        public ExchangesCollection WebsocketExchanges
        {
            get { return ((ExchangesCollection)(base["websocketExchanges"])); }
        }

        [ConfigurationProperty("connections")]
        public ConnectionsCollection Connections
        {
            get { return ((ConnectionsCollection)(base["connections"])); }
        }

        [ConfigurationProperty("host")]
        public ConnectionElement Host
        {
            get { return ((ConnectionElement)(base["host"])); }
        }

        [ConfigurationProperty("settings")]
        public SettingsElement Settings
        {
            get { return ((SettingsElement)(base["settings"])); }
        }

        public Dictionary<string, bool> GetRestEnabledExchanges()
        {
            var exchanges = new Dictionary<string, bool>();
            var list = new List<ExchangeName>();

            Utils.MapExchangeAttributes(
                (attrName, isWebsocket) =>
                {
                    if (!isWebsocket)
                    {
                        var elem = RestExchanges.FindExchangeElement(attrName);
                        if (elem != null && elem.Enable == 1)
                            exchanges[attrName] = false;
                    }
                });

            return exchanges;
        }
        
        public Dictionary<string, bool> GetWebSocketEnabledExchanges()
        {
            var exchanges = new Dictionary<string, bool>();
            var list = new List<ExchangeName>();

            Utils.MapExchangeAttributes(
                (attrName, isWebsocket) =>
                {
                    if (isWebsocket)
                    {
                        var elem = WebsocketExchanges.FindExchangeElement(attrName);
                        if (elem != null && elem.Enable == 1)
                            exchanges[attrName] = false;
                    }
                });

            return exchanges;
        }
    }
}
