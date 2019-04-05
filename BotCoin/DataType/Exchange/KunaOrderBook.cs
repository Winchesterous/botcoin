using System.Collections.Generic;

namespace BotCoin.DataType.Exchange
{
    public class KunaOrderBook
    {
        public List<KunaOrder> Asks { get; set; }

        public List<KunaOrder> Bids { get; set; }
    }
}
