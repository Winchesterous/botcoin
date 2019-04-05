using System;
using System.Collections.Generic;

namespace BotCoin.DataType.Exchange
{
    public class OkExOrderBook
    {
        public int Binary { set; get; }
        public string Channel { set; get; }
        public OkExResponseData Data { set; get; }

        public void SortAsks()
        {
            if (Data.Asks != null)
            {
                var comparer = Comparer<double>.Default;
                Array.Sort<double[]>(Data.Asks, (x, y) => comparer.Compare(x[0], y[0]));
            }
        }
    }
}
