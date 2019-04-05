using System;

namespace BotCoin.Core
{
    public class ExchangeAttribute : Attribute
    {
        public string Name
        {
            set; get;
        }

        public bool IsWebsocket
        {
            set; get;
        }
    }
}
