using BotCoin.DataType.Exchange;
using System;
using System.Collections.Generic;

namespace BotCoin.DataType
{
    public class OrderBookEventArgs : EventArgs
    {
        public static DateTime StartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        static int OrdersLimit = 15;
                
        public OrderBookEventArgs()
        {
            CreatedAt = DateTime.UtcNow;
            Instrument2 = CurrencyName.USD;
        }

        private void InitAsks(OrderBook orderBook, double[][] orders)
        {
            if (orders == null)
                return;

            OrderBook.Asks = new Order[orders.Length];
            for (int i = 0; i < OrderBook.Asks.Length; i++)
                OrderBook.Asks[i] = new Order { Price = orders[i][0], Amount = orders[i][1] };
        }

        private void InitBids(OrderBook orderBook, double[][] orders)
        {
            if (orders == null)
                return;

            OrderBook.Bids = new Order[orders.Length];
            for (int i = 0; i < OrderBook.Bids.Length; i++)
                OrderBook.Bids[i] = new Order { Price = orders[i][0], Amount = orders[i][1] };
        }

        public OrderBookEventArgs(HitBtcOrderBook orders, DateTime? time = null) : this()
        {
            OrderBook = new OrderBook();
            CreatedAt = time.HasValue ? time.Value : DateTime.MinValue;

            if (orders.Params == null)
                return;

            var book = orders.Params;
            if (book.Ask != null)
            {
                int count = book.Ask.Length > OrdersLimit ? OrdersLimit : book.Ask.Length;
                OrderBook.Asks = new Order[count];

                for (int i = 0; i < count; i++)
                    OrderBook.Asks[i] = new Order { Price = book.Ask[i].Price, Amount = book.Ask[i].Size };
            }
            if (book.Bid != null)
            {
                int count = book.Bid.Length > OrdersLimit ? OrdersLimit : book.Bid.Length;
                OrderBook.Bids = new Order[count];

                for (int i = 0; i < count; i++)
                    OrderBook.Bids[i] = new Order { Price = book.Bid[i].Price, Amount = book.Bid[i].Size };
            }
        }

        public OrderBookEventArgs(CexOrderBookResponse orders) : this()
        {
            var data = orders.data;

            OrderId   = data.id.ToString();
            OrderBook = new OrderBook();
            Timestamp = data.time != 0 ? StartTime.AddMilliseconds(data.time) : StartTime.AddSeconds(data.timestamp);

            InitAsks(OrderBook, orders.data.asks);
            InitBids(OrderBook, orders.data.bids);
        }

        public OrderBookEventArgs(OkExOrderBook orders, CurrencyName instrument2 = CurrencyName.Undefined) : this()
        {
            if (orders.Data.Timestamp > 0)
                Timestamp = StartTime.AddMilliseconds(orders.Data.Timestamp);

            OrderBook = new OrderBook();

            if (instrument2 != CurrencyName.Undefined)
            {
                Instrument2 = instrument2;
            }
            InitAsks(OrderBook, orders.Data.Asks);
            InitBids(OrderBook, orders.Data.Bids);
        }
        
        public OrderBookEventArgs(XBtcResponseResult response) : this()
        {
            Action<XBtcOrder[]> initAsks = orders =>
            {
                if (orders != null)
                {
                    OrderBook.Asks = new Order[orders.Length];
                    for (int i = 0; i < orders.Length; i++)
                        OrderBook.Asks[i] = new Order { Price = orders[i].Price, Amount = orders[i].Volume };
                }
            };
            Action<XBtcOrder[]> initBids = orders =>
            {
                if (orders != null)
                {
                    OrderBook.Bids = new Order[orders.Length];
                    for (int i = 0; i < orders.Length; i++)
                        OrderBook.Bids[i] = new Order { Price = orders[i].Price, Amount = orders[i].Volume };
                }
            };

            OrderBook = new OrderBook();

            if (response.Snapshot != null)
            {
                initAsks(response.Snapshot[0].Asks);
                initBids(response.Snapshot[0].Bids);
            }
            else
            {
                initAsks(response.Asks);
                initBids(response.Bids);
            }
        }

        public OrderBookEventArgs(BinanceOrderBook binance, string symbol = null) : this()
        {
            OrderBook = new OrderBook();
            if (symbol != null)
            {
                Instrument2 = ParseSymbol(symbol)[1];
            }
            if (binance.Data.Bids != null)
            {
                OrderBook.Bids = new Order[binance.Data.Bids.Length];
                for (int i = 0; i < binance.Data.Bids.Length; i++)
                    OrderBook.Bids[i] = new Order { Price = Double.Parse(binance.Data.Bids[i][0].ToString()), Amount = Double.Parse(binance.Data.Bids[i][1].ToString()) };
            }
            if (binance.Data.Asks != null)
            {
                OrderBook.Asks = new Order[binance.Data.Asks.Length];
                for (int i = 0; i < binance.Data.Asks.Length; i++)
                    OrderBook.Asks[i] = new Order { Price = Double.Parse(binance.Data.Asks[i][0].ToString()), Amount = Double.Parse(binance.Data.Asks[i][1].ToString()) };
            }
        }

        public OrderBookEventArgs(WexOrderBook orders) : this()
        {
            OrderBook = new OrderBook();

            InitAsks(OrderBook, orders.Ask);
            InitBids(OrderBook, orders.Bid);
        }

        public OrderBookEventArgs(BitstampOrderBook order, OrderState state) : this()
        {
            OrderBook = new OrderBook();
            OrderId = order.id;
            IsOrderDeleted = state == OrderState.Deleted;
            MicroTimestamp = order.microtimestamp;

            if (order.datetime > 0)
                Timestamp = StartTime.AddSeconds(order.datetime);

            if (order.BidOrder)
            {
                OrderBook.Bids = new Order[1] { new Order { Price = order.price, Amount = order.amount } };
                IsBidOrder = true;
            }
            else
                OrderBook.Asks = new Order[1] { new Order { Price = order.price, Amount = order.amount } };
        }

        public OrderBookEventArgs(BitmexOrderL2Book orders) : this()
        {
            OrderBook = new OrderBook();
            var asks = new List<Order>();
            var bids = new List<Order>();

            foreach (var order in orders.Data)
            {
                if (order.OrderSide == OrderSide.ASK)
                    asks.Add(new Order { Price = order.Price.Value, Amount = order.Size });
                else
                    bids.Add(new Order { Price = order.Price.Value, Amount = order.Size });
            }

            OrderBook.Asks = asks.ToArray();
            OrderBook.Bids = bids.ToArray();
        }

        public static CurrencyName[] ParseSymbol(string symbol)
        {
            symbol = symbol.ToLower();

            var instruments = new CurrencyName[2];
            var instrument  = CurrencyName.Undefined;
            int length      = 3, startIndex = 0;
            var str         = symbol.Substring(startIndex, length);

            Action parsing = () =>
            {                
                if (!Enum.TryParse(str, true, out instrument))
                {
                    str = symbol.Substring(startIndex, length + 1);
                    if (!Enum.TryParse(str, true, out instrument))
                    {
                        str = symbol.Substring(startIndex, length + 2);
                        if (!Enum.TryParse(str, true, out instrument))
                        {
                            throw new InvalidOperationException("Currency symbol parsing has been failed " + symbol);
                        }
                        startIndex = 5;
                    }
                    else
                        startIndex = 4;
                }
                else
                    startIndex = 3;

                switch (instrument)
                {
                case CurrencyName.BCC: instrument = CurrencyName.BCH; break;
                case CurrencyName.DASH: instrument = CurrencyName.DSH; break;
                case CurrencyName.USDT: instrument = CurrencyName.USD; break;
                }                
            };

            if (symbol.StartsWith("bchabc"))
            {
                instruments[0] = CurrencyName.BCHABC;
                if (symbol.EndsWith("btc"))
                    instruments[1] = CurrencyName.BTC;
                else if (symbol.EndsWith("usdt"))
                    instruments[1] = CurrencyName.USDT;
                else
                    throw new InvalidOperationException("Currency symbol parsing has been failed " + symbol);
            }
            else if (symbol.StartsWith("bchsv"))
            {
                instruments[0] = CurrencyName.BCHSV;
                if (symbol.EndsWith("btc"))
                    instruments[1] = CurrencyName.BTC;
                else if (symbol.EndsWith("usdt"))
                    instruments[1] = CurrencyName.USDT;
                else
                    throw new InvalidOperationException("Currency symbol parsing has been failed " + symbol);
            }
            else
            {
                parsing();
                instruments[0] = instrument;

                str = symbol.Substring(startIndex, length);
                parsing();
                instruments[1] = instrument;
            }
            return instruments;
        }

        public OrderBook OrderBook
        {
            set
            {
                if (value.Timestamp.HasValue)
                    Timestamp = StartTime.AddSeconds(value.Timestamp.Value);

                _orderBook = value;
            }
            get
            {
                return _orderBook;
            }
        }
        OrderBook _orderBook;

        public DateTime CreatedAt { set; get; }

        public DateTime Timestamp { set; get; }
        public long MicroTimestamp { set; get; }

        public CurrencyName Instrument1 { set; get; }

        public CurrencyName Instrument2 { set; get; }

        public string OrderId { set; get; }

        public bool IsBidOrder { set; get; }

        public bool IsOrderDeleted { private set; get; }
    }
}
