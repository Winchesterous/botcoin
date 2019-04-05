using BotCoin.DataType.Exchange;
using System.Collections.Generic;

namespace BotCoin.Exchange
{
    public class BitmexOrderBookL2
    {
        readonly Dictionary<string,SortedList<long, double>> _bids;
        readonly Dictionary<string, double> _tickSizes;
        readonly Dictionary<string, int> _indexes;

        public BitmexOrderBookL2()
        {            
            _tickSizes = new Dictionary<string, double>();
            _indexes   = new Dictionary<string, int>();
            _bids      = new Dictionary<string, SortedList<long, double>>();
        }

        public void Initialize(BitmexInstrumentSettings setting)
        {
            var symbol = setting.Symbol;

            _tickSizes[symbol] = setting.TickSize.Value;
            _indexes[symbol]   = setting.Index.Value;
            _bids[symbol]      = new SortedList<long, double>();
        }

        private double GetPriceById(long id, int idx, double tickSize)
        {
            return ((100000000L * idx) - id) * tickSize;
        }

        public string ProcessBook(BitmexOrderL2Book book, ref double bid, ref double ask)
        {
            SortedList<long, double> bids = null;
            string symbol = null;
            double tickSize = 0;
            int idx = 0;

            bool delete = book.Action.StartsWith("del");
            if (!delete)
            {
                bool create = book.Action.StartsWith("ins") || book.Action.StartsWith("upd") || book.Action.StartsWith("par");
                if (!create) return symbol;
            }
            if (book.Data.Length > 0)
            {
                symbol   = book.Data[0].Symbol;
                bids     = _bids[symbol];
                idx      = _indexes[symbol];
                tickSize = symbol.StartsWith("XBT") ? 0.01 : _tickSizes[symbol]; // LEGACY_TICKS = {"XBTUSD":0.01}
            }
            foreach (var item in book.Data)
            {
                if (item.OrderSide == DataType.OrderSide.BID)
                {
                    if (delete)
                    {
                        if (bids.ContainsKey(item.Id))
                            bids.Remove(item.Id);
                    }
                    else
                    {
                        if (!bids.ContainsKey(item.Id))
                        {
                            var price = GetPriceById(item.Id, idx, tickSize);
                            bids.Add(item.Id, price);
                        }
                    }
                }
            }
            if (_bids.Count > 0)
            {
                if (_bids[symbol].Count > 0)
                {
                    var k = _bids[symbol].Keys[0];
                    bid   = _bids[symbol][k];
                    ask   = bid + _tickSizes[symbol];
                }
            }
            return symbol;
        }
    }
}
