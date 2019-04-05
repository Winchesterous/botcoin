using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Exchange;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace BotCoin.Instruments
{
    public abstract class Instrument
    {
        protected Action<ExchangeName, OrderBookEventArgs> _orderBookHandler;
        protected Action<ExchangeName, TradeEventArgs> _tradeHandler;
        private List<Tuple<double, double>> _profitRatios;
        protected readonly TradingStrategyElement TradingStrategy;
        protected readonly ProfitRatioStrategyElement ProfitRatioStrategy;
        //protected double AvgAmount;
        protected double BalanceStep;
        private double MinUsdProfit;
        double _min;
        double _max;

        protected Instrument()
        {
            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            _profitRatios = new List<Tuple<double, double>>();

            ProfitRatioStrategy = config.Settings.ProfitRatioStrategy;
            TradingStrategy     = config.Settings.TradingStrategy;
            MinUsdProfit        = ProfitRatioStrategy.MinUsdProfit;
            
            InitProfitRatios(ProfitRatioStrategy.ProfitRatio);
        }
                
        private void InitProfitRatios(string profitRatio)
        {
            if (profitRatio.Length > 0)
            {
                foreach (var pair in profitRatio.Split(',').Select(s => s.Trim()))
                {
                    var arr = pair.Split(':').Select(s => s.Trim()).ToArray();
                    _profitRatios.Add(new Tuple<double, double>(Double.Parse(arr[0]), Double.Parse(arr[1])));
                }
            }
        }

        public void SetAveragePrice(double price)
        {
            _min = price / 1.4;
            _max = price * 1.4;
        }

        public bool CheckConstraint(double bidPrice, double askPrice, double currencyRate)
        {
            if (bidPrice > 0)
            {
                var price = bidPrice / currencyRate;
                if (price < _min || price > _max)
                    return false;
            }
            if (askPrice > 0)
            {
                var price = askPrice / currencyRate;
                if (price < _min || price > _max)
                    return false;
            }
            return true;
        }

        protected void OnOrderBook(object sender, OrderBookEventArgs e, CurrencyName instrument)
        {
            e.Instrument1 = instrument;
            _orderBookHandler(((IExchange)sender).GetExchangeName(), e);
        }

        protected void OnTrade(object sender, TradeEventArgs e, CurrencyName instrument)
        {
            e.Instrument1 = instrument;
            _tradeHandler(((IExchange)sender).GetExchangeName(), e);
        }

        public static void InitWebsocketInstruments(Dictionary<CurrencyName, Instrument> instruments)
        {
            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            var instrumentArray = config.WebsocketExchanges.Instrument.Split(',').Select(s => s.Trim().ToUpper()).ToArray();

            foreach (var instrument in instrumentArray)
            {
                if (instrument == "BTC") instruments[CurrencyName.BTC] = new InstrumentBTC();
                if (instrument == "BCH") instruments[CurrencyName.BCH] = new InstrumentBCH();
                if (instrument == "LTC") instruments[CurrencyName.LTC] = new InstrumentLTC();
                if (instrument == "ETH") instruments[CurrencyName.ETH] = new InstrumentETH();
                if (instrument == "XRP") instruments[CurrencyName.XRP] = new InstrumentXRP();
                if (instrument == "DSH") instruments[CurrencyName.DSH] = new InstrumentDASH();
                if (instrument == "IOTA") instruments[CurrencyName.IOTA] = new InstrumentIOTA();
            }
        }

        public static void InitRestInstruments(Dictionary<CurrencyName, Instrument> instruments)
        {
            var config = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            var instrumentArray = config.RestExchanges.Instrument.Split(',').Select(s => s.Trim().ToUpper()).ToArray();

            if (config.RestExchanges.Count > 0)
            {
                Utils.ThrowIf(instrumentArray.Length > 1, "Too many instruments for the REST mode");
                Utils.ThrowIf(instrumentArray.Length == 0, "Instruments not defined");
            }

            var instrument = instrumentArray[0];

            if (instrument == "BTC") instruments[CurrencyName.BTC] = new InstrumentBTC();
            if (instrument == "BCH") instruments[CurrencyName.BCH] = new InstrumentBCH();
            if (instrument == "LTC") instruments[CurrencyName.LTC] = new InstrumentLTC();
            if (instrument == "ETH") instruments[CurrencyName.ETH] = new InstrumentETH();
            if (instrument == "XRP") instruments[CurrencyName.XRP] = new InstrumentXRP();
            if (instrument == "DSH") instruments[CurrencyName.DSH] = new InstrumentDASH();

            Utils.ThrowIf(instruments.Count == 0, "Instrument wasn't created");
        }

        public double GetBalanceStep()
        {
            return BalanceStep;
        }

        //public double GetAvgAmount()
        //{
        //    return AvgAmount;
        //}

        public double GetMinUsdProfit()
        {
            return MinUsdProfit;
        }

        public List<Tuple<double, double>> GetProfitRatios()
        {
            return _profitRatios;
        }

        public double GetAskPrice(IExchange ex)
        {
            var order = GetAskOrder(ex);
            return order == null ? 0 : order.Price;
        }

        public double GetAskAmount(IExchange ex)
        {
            return GetAskOrder(ex).Amount[0];
        }

        public double GetAskOrderAmount(IExchange ex)
        {
            return GetAskOrder(ex).Amount[1];
        }

        public double GetBidPrice(IExchange ex)
        {
            var order = GetBidOrder(ex);
            return order == null ? 0 : order.Price;
        }

        public double GetBidAmount(IExchange ex)
        {
            return GetBidOrder(ex).Amount[0];
        }

        public double GetBidOrderAmount(IExchange ex)
        {
            return GetBidOrder(ex).Amount[1];
        }

        public bool ValidOrderPrices(IExchange ex, OrderInfo bidOrder, OrderInfo askOrder)
        {
            var bid = GetBidOrder(ex);
            var ask = GetAskOrder(ex);

            bid.Price = ex.ConvertPriceToUsd(bidOrder.Price);
            ask.Price = ex.ConvertPriceToUsd(askOrder.Price);

            if (bidOrder.OrderAmount != 0 && bidOrder.OrderAmount < GetBalanceStep())
                return false;

            if (askOrder.OrderAmount != 0 && askOrder.OrderAmount < GetBalanceStep())
                return false;

            bid.Amount[0] = bidOrder.Amount;
            bid.Amount[1] = bidOrder.OrderAmount;
            ask.Amount[0] = askOrder.Amount;
            ask.Amount[1] = askOrder.OrderAmount;

            return true;
        }

        public bool ContainsOrders(IExchange ex)
        {
            return GetBidOrder(ex) != null && GetAskOrder(ex) != null;
        }

        public void InitExchangePrice(ExchangeOrder order, ExchangePricesEventArgs args, OrderSide type)
        {
            Action<int, OrderSide> init = (i, type_) =>
            {
                if (order.Price != 0)
                {
                    if (type_ == OrderSide.BID)
                    {
                        //if ((GetPrice(args, i) - order.Price) / order.Price > 0.001)
                        {
                            order.Price = GetPrice(args, i);
                            order.Amount = GetAmount(args, i);
                        }
                    }
                    else if (type_ == OrderSide.ASK)
                    {
                        //if ((order.Price - GetPrice(args, i)) / order.Price > 0.001)
                        {
                            order.Price = GetPrice(args, i);
                            order.Amount = GetAmount(args, i);
                        }
                    }
                }
                else
                {
                    order.Price = GetPrice(args, i);
                    order.Amount = GetAmount(args, i);
                }
            };

            if (type == OrderSide.BID)
                init(0, type);
            else if (type == OrderSide.ASK)
                init(1, type);
            else
                Utils.ThrowIf(true, "BOTH order type");
        }

        public void CreateOrder(IExchange ex, Instrument ins, ExchangePricesEventArgs args)
        {
            Instrument2 = args.Instrument2;

            var type = ins.GetOrderType(args);
            Action<OrderSide> create = type_ =>
            {
                if (type_ == OrderSide.BID)
                {
                    if (GetBidOrder(ex) == null)
                        CreateOrder(ex, type_, args);
                    else
                        InitExchangePrice(GetBidOrder(ex), args, type_);
                }
                else if (type_ == OrderSide.ASK)
                {
                    if (GetAskOrder(ex) == null)
                        CreateOrder(ex, type_, args);
                    else
                        InitExchangePrice(GetAskOrder(ex), args, type_);
                }
            };

            if (type == OrderSide.BID)
                create(type);
            else if (type == OrderSide.ASK)
                create(type);
            else
            {
                create(OrderSide.BID);
                create(OrderSide.ASK);
            }
        }        

        public virtual void SubscribeTrade(IExchange ex, Action<ExchangeName, TradeEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        public virtual void UnsubscribeTrade(IExchange ex)
        {
            throw new NotImplementedException();
        }

        public CurrencyName Instrument2
        {
            set; get;
        }

        public abstract CurrencyName GetInstrument();

        public abstract double GetCryptoBalance(IExchange ex);

        public abstract void SubscribeOrderBook(IExchange ex, Action<ExchangeName, OrderBookEventArgs> handler);
                
        public abstract void UnsubscribeOrderBook(IExchange ex);

        public abstract void InitExchangePrice(BaseExchange ex, ExchangePricesEventArgs arg);

        public abstract ExchangeOrder GetBidOrder(IExchange ex);

        protected abstract ExchangeOrder GetAskOrder(IExchange ex);

        public abstract OrderInfo CreateBidOrder(IExchange ex, out double expectedAmount);

        public abstract OrderInfo CreateAskOrder(IExchange ex, out double expectedAmount);
                
        public abstract OrderSide GetOrderType(ExchangePricesEventArgs args);

        public abstract double GetPrice(ExchangePricesEventArgs args, int index);

        public abstract double[] GetAmount(ExchangePricesEventArgs args, int index);

        public abstract void CreateOrder(IExchange ex, OrderSide type, ExchangePricesEventArgs args);

        public abstract Dictionary<string,double> InitTempOrderBook(ExchangePricesEventArgs data);
    }
}
