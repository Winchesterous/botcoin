using BotCoin.DataType.Exchange;
using System;

namespace BotCoin.DataType
{
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class ExchangePricesEventArgs : EventArgs
    {
        public ExchangePricesEventArgs()
        {
            CreatedAt = DateTime.UtcNow;
            OrderId = String.Empty;
        }

        public ExchangePricesEventArgs(double price1, double price2, string symbol) : this()
        {
            Exchange = ExchangeName.Bitmex;
            Symbol = symbol;

            if (symbol.StartsWith("XBT"))       BtcPrice = new double[2] { price1, price2 };
            else if (symbol.StartsWith("ETH"))  EthPrice = new double[2] { price1, price2 };
            else if (symbol.StartsWith("LTC"))  LtcPrice = new double[2] { price1, price2 };
            else if (symbol.StartsWith("EOS"))  EosPrice = new double[2] { price1, price2 };
            else if (symbol.StartsWith("XRP"))  XrpPrice = new double[2] { price1, price2 };
            else if (symbol.StartsWith("ADA"))  AdaPrice = new double[2] { price1, price2 };
            else if (symbol.StartsWith("TRX"))  TrxPrice = new double[2] { price1, price2 };
            else if (symbol.StartsWith("BCH"))  BchPrice = new double[2] { price1, price2 };
            else throw new InvalidOperationException(String.Format("Instrument {0} doesn't supported", symbol));
        }
        public ExchangePricesEventArgs(OrderBookEventArgs e, CurrencyName ins1, ExchangeName ex = ExchangeName.Bitstamp, CurrencyName ins2 = CurrencyName.USD) : this()

        {
            Instrument1    = ins1;
            Instrument2    = ins2;
            OrderBook      = e.OrderBook;
            Exchange       = ex;
            OrderId        = e.OrderId;
            Timestamp      = e.Timestamp;
            IsOrderDeleted = e.IsOrderDeleted;
            MicroTimestamp = e.MicroTimestamp;
        } 

        public ExchangePricesEventArgs(BinanceOrderBook orderBook) : this()
        {
            var pair = orderBook.Stream.Split('@')[0];
            var symbol = OrderBookEventArgs.ParseSymbol(pair);

            Instrument1 = symbol[0];
            Instrument2 = symbol[1];
            Exchange    = ExchangeName.Binance;
            OrderBook   = new OrderBookEventArgs(orderBook).OrderBook;
        }

        public ExchangePricesEventArgs(BitmexMarginData margin) : this()
        {
            Exchange  = ExchangeName.Bitmex;
            BtxMargin = margin;
        }

        public ExchangePricesEventArgs(BitmexWalletData wallet) : this()
        {
            Exchange  = ExchangeName.Bitmex;
            BtxWallet = wallet;
        }

        public ExchangePricesEventArgs(BitmexOrderData[] orders, DateTime? dt = null) : this()
        {
            Exchange  = ExchangeName.Bitmex;
            BtxOrders = orders;
            Symbol    = orders[0].Symbol;

            if (dt.HasValue)
                CreatedAt = dt.Value;
        }

        public ExchangePricesEventArgs(BitmexPositionData[] positions) : this()
        {
            Exchange     = ExchangeName.Bitmex;
            BtxPositions = positions;
        }

        public ExchangePricesEventArgs(BitmexFundingData[] funding) : this()
        {
            Exchange   = ExchangeName.Bitmex;
            BtxFunding = funding;
        }

        public ExchangePricesEventArgs(GdaxResponse response) : this()
        {
            Instrument1 = CurrencyName.BTC;
            Instrument2 = CurrencyName.USD;
            Exchange    = ExchangeName.Gdax;
            Gdax        = response;
        }

        public ExchangePricesEventArgs(OkExOrderBook orderBook, string channel) : this()
        {
            var instruments = OkExTrade.ParseInstruments(channel, "_depth_");

            Instrument1 = instruments[0];
            Instrument2 = instruments[1];
            Exchange    = ExchangeName.OkEx;
            OrderBook   = new OrderBookEventArgs(orderBook).OrderBook;
        }

        public ExchangePricesEventArgs(HitBtcOrderBook orderBook) : this()
        {
            var instruments = OrderBookEventArgs.ParseSymbol(orderBook.Params.Symbol);

            Instrument1 = instruments[0];
            Instrument2 = instruments[1];
            Exchange    = ExchangeName.HitBtc;
            OrderBook   = new OrderBookEventArgs(orderBook).OrderBook;
        }

        public string Symbol { set; get; }
        public string OrderId { set; get; }
        public long MicroTimestamp { set; get; }
        public DateTime Timestamp { set; get; }
        public DateTime CreatedAt { set; get; }
        public bool IsOrderDeleted { set; get; }
        public CurrencyName Instrument1 { set; get; }
        public CurrencyName Instrument2 { set; get; }
        public ExchangeName Exchange { set; get; }
        public double[] BtcPrice { set; get; }
        public double[][] BtcAmount { set; get; }
        public double[] BchPrice { set; get; }
        public double[][] BchAmount { set; get; }
        public double[] EthPrice { set; get; }
        public double[][] EthAmount { set; get; }
        public double[] LtcPrice { set; get; }
        public double[][] LtcAmount { set; get; }
        public double[] XrpPrice { set; get; }
        public double[][] XrpAmount { set; get; }
        public double[] EosPrice { set; get; }
        public double[] DashPrice { set; get; }
        public double[] AdaPrice { set; get; }
        public double[] TrxPrice { set; get; }
        public double[][] DashAmount { set; get; }
        public double[] IotaPrice { set; get; }
        public double[][] IotaAmount { set; get; }
        public OrderBook OrderBook { set; get; }
        public BitmexOrderData[] BtxOrders { set; get; }
        public BitmexPositionData[] BtxPositions { set; get; }
        public BitmexFundingData[] BtxFunding { set; get; }
        public BitmexMarginData BtxMargin { set; get; }
        public BitmexWalletData BtxWallet { set; get; }
        public GdaxResponse Gdax { set; get; }
    }
}
