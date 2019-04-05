using BotCoin.ApiClient;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using BotCoin.Instruments;
using BotCoin.Logger;
using BotCoin.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BotCoin.Exchange
{
    public class BaseExchange : IExchange
    {
        public readonly double MinUsdValueRatio;
        readonly CurrencyName _currency;
        readonly double BestPriceRatio;

        protected BaseRestExchange _restExchange;
        protected AccountManagement _account;
        protected double BTCUSDMinValue;
        protected double BCHUSDMinValue;
        protected double LTCUSDMinValue;
        protected double ETHUSDMinValue;
        protected double XRPUSDMinValue;
        protected double DASHUSDMinValue;
        protected double IOTAUSDMinValue;

        protected BaseExchange(ExchangeSettingsData setting)
        {
            if (String.IsNullOrEmpty(setting.CurrencyName))
                setting.CurrencyName = "USD";

            BestPriceRatio   = 0.97;
            MinUsdValueRatio = 0.001;   // 0.1%
            _currency        = setting.Currency;

            CountryCurrencyDecimal = 2;
            CryptoCurrencyDecimal  = 6;
            TradingState           = TradingState.Ok;
        }

        public int CryptoCurrencyDecimal { private set; get; }

        public int CountryCurrencyDecimal { private set; get; }

        public virtual void InitExchange()
        {
            var ex = DbGateway.GetExchangeInfo(GetExchangeName());

            BTCUSDMinValue  = ex.UsdMinValue;
            LTCUSDMinValue  = ex.UsdMinValue;
            BCHUSDMinValue  = ex.UsdMinValue;
            ETHUSDMinValue  = ex.UsdMinValue;
            XRPUSDMinValue  = ex.UsdMinValue;
            DASHUSDMinValue = ex.UsdMinValue;
            IOTAUSDMinValue = ex.UsdMinValue;
            TradingState    = TradingState.Ok;
            TradeFee        = ex.TradeFee;

            _account = new AccountManagement(this, Log);
            _account.UpdateBalances();
        }

        protected void InitBitstampConfiguration(BitstampClient client)
        {
            var configs = client.GetExchangeConfiguration();
            var btc = configs.Where(c => c.UrlSymbol == "btcusd").Single();

            BTCUSDMinValue = btc.MinOrderSize(CurrencyName.USD);
            BCHUSDMinValue = configs.Where(c => c.UrlSymbol == "bchusd").Single().MinOrderSize(CurrencyName.USD);
            LTCUSDMinValue = configs.Where(c => c.UrlSymbol == "ltcusd").Single().MinOrderSize(CurrencyName.USD);
            ETHUSDMinValue = configs.Where(c => c.UrlSymbol == "ethusd").Single().MinOrderSize(CurrencyName.USD);
            XRPUSDMinValue = configs.Where(c => c.UrlSymbol == "xrpusd").Single().MinOrderSize(CurrencyName.USD);

            CountryCurrencyDecimal = btc.CountryCurrencyDecimal;
            CryptoCurrencyDecimal = btc.CryptoCurrencyDecimal;
        }

        public virtual void Logon()
        {
        }

        public virtual void Logout()
        {
        }

        #region Properties
        public ExchangeBtcOrder BtcBidOrder { set; get; }
        public ExchangeBtcOrder BtcAskOrder { set; get; }
        public ExchangeBchOrder BchBidOrder { set; get; }
        public ExchangeBchOrder BchAskOrder { set; get; }
        public ExchangeLtcOrder LtcBidOrder { set; get; }
        public ExchangeLtcOrder LtcAskOrder { set; get; }
        public ExchangeEthOrder EthBidOrder { set; get; }
        public ExchangeEthOrder EthAskOrder { set; get; }
        public ExchangeXrpOrder XrpBidOrder { set; get; }
        public ExchangeXrpOrder XrpAskOrder { set; get; }
        public ExchangeDashOrder DashAskOrder { set; get; }
        public ExchangeDashOrder DashBidOrder { set; get; }
        public ExchangeIotaOrder IotaBidOrder { set; get; }
        public ExchangeIotaOrder IotaAskOrder { set; get; }
        public IServiceEventLogger Log { set; get; }
        public IDbRepository DbGateway { set; get; }
        public bool Blocked { set; get; }

        public double CurrencyRate
        {
            get { return _account.CurrencyRate; }
        }

        public double TradeFee { private set; get; }

        public CurrencyName Currency
        {
            get { return _currency; }
        }

        public TradingState TradingState { set; get; }

        public List<IExchange> Exchanges { set; get; }

        public double EthBalance
        {
            get { return _account.EthBalance; }
            set { _account.EthBalance = value; }
        }

        public double BtcBalance
        {
            get { return _account.BtcBalance; }
            set { _account.BtcBalance = value; }
        }

        public double BchBalance
        {
            get { return _account.BchBalance; }
            set { _account.BchBalance = value; }
        }

        public double LtcBalance
        {
            get { return _account.LtcBalance; }
            set { _account.LtcBalance = value; }
        }

        public double XrpBalance
        {
            get { return _account.XrpBalance; }
            set { _account.XrpBalance = value; }
        }

        public double DashBalance
        {
            get { return _account.DashBalance; }
            set { _account.DashBalance = value; }
        }

        public double IotaBalance
        {
            get { return _account.IotaBalance; }
            set { _account.IotaBalance = value; }
        }

        public double UsdBalance
        {
            get { return _account.UsdBalance; }
            set { _account.UsdBalance = value; }
        }

        public double TradingFee
        {
            get { return _account.Fee.TradingFee; }
        }
        #endregion

        public double ConvertPriceToUsd(double price)
        {
            double currentPrice = price;

            if (_account.CurrencyRate != 1)
                currentPrice = Math.Round(currentPrice / _account.CurrencyRate, 4);

            return currentPrice;
        }

        public bool ValidOrderPrices(OrderBookEventArgs args, Instrument ins)
        {
            double expectedAmount;

            var bidOrder = ins.CreateBidOrder(this, out expectedAmount);
            var askOrder = ins.CreateAskOrder(this, out expectedAmount);

            GetPrice(args.OrderBook.Bids, args.OrderBook.Asks, expectedAmount, bidOrder, askOrder);

            if (bidOrder.Price == 0 && askOrder.Price == 0)
                return false;

            //if (!ins.CheckConstraint(bidOrder.Price, askOrder.Price, CurrencyRate))
            //    return false;

            return ins.ValidOrderPrices(this, bidOrder, askOrder);
        }

        private void GetPrice(Order[] bids, Order[] asks, double expectedAmount, OrderInfo bidOrder, OrderInfo askOrder)
        {
            double price, orderAmount;

            GetPrice(bids, expectedAmount, out price, out orderAmount);
            bidOrder.Price = price;
            bidOrder.OrderAmount = orderAmount;

            GetPrice(asks, expectedAmount, out price, out orderAmount);
            askOrder.Price = price;
            askOrder.OrderAmount = orderAmount;

            bidOrder.Amount =
            askOrder.Amount = expectedAmount;
        }

        private void GetPrice(Order[] orders, double amount, out double price, out double orderAmount)
        {
            double sum = 0, bestPrice = 0;
            orderAmount = 0;
            price = 0;

            if (orders == null || orders.Length == 0)
                return;

            for (int i = 0; i < orders.Length; i++)
            {
                if (orders[i].Amount == 0 || orders[i].Price == 0)
                    continue;

                if (bestPrice == 0)
                    bestPrice = orders[i].Price;

                orderAmount = orders[i].Amount;
                price = orders[i].Price;
                sum += orderAmount;

                if (sum >= amount)
                    break;
            }

            if (bestPrice / price < BestPriceRatio)
                price = 0;
        }

        public virtual ExchangeName GetExchangeName()
        {
            throw new NotSupportedException();
        }

        public double GetMinTradeValue(CurrencyName currency, double value = 0)
        {
            if (currency == CurrencyName.USD)
                return BTCUSDMinValue;

            switch (currency)
            {
            case CurrencyName.BTC: return BTCUSDMinValue / value;
            case CurrencyName.BCH: return BCHUSDMinValue / value;
            case CurrencyName.LTC: return LTCUSDMinValue / value;
            case CurrencyName.ETH: return ETHUSDMinValue / value;
            case CurrencyName.XRP: return XRPUSDMinValue / value;
            case CurrencyName.DSH: return DASHUSDMinValue / value;
            case CurrencyName.IOTA: return IOTAUSDMinValue / value;
            default: throw new InvalidOperationException("GetMinTradeValue(). Invalid currency.");
            }
        }

        public void RemoveOrders()
        {
            BtcBidOrder = BtcAskOrder = null;
            LtcBidOrder = LtcAskOrder = null;
            BchBidOrder = BchAskOrder = null;
            EthBidOrder = EthAskOrder = null;
            XrpBidOrder = XrpAskOrder = null;
            DashBidOrder = DashAskOrder = null;
        }

        public virtual UserAccount GetBalances()
        {
            return null;
        }

        public virtual OrderResponse PlaceOrder(double price, double amount, CurrencyName currency, OrderSide side, TradeOrderType type = TradeOrderType.Limit)
        {
            return null;
        }

        public virtual bool CancelOrder(string orderId, string symbol = null)
        {
            return false;
        }

        public virtual string CancelOrders(string[] orderIds, string symbol = null)
        {
            return null;
        }

        public virtual OrderResponse[] GetActiveOrders(CurrencyName currency = CurrencyName.Undefined)
        {
            return new OrderResponse[0];
        }

        public virtual OrderBook GetMarketSummaries()
        {
            return _restExchange == null ? null : _restExchange.GetBtcOrderBook();
        }

        public virtual OrderBook GetBtcOrderBook()
        {
            return _restExchange == null ? null : _restExchange.GetBtcOrderBook();
        }

        public virtual OrderBook GetEthOrderBook()
        {
            return _restExchange == null ? null : _restExchange.GetEthOrderBook();
        }

        public virtual OrderBook GetBchOrderBook()
        {
            return _restExchange == null ? null : _restExchange.GetBchOrderBook();
        }

        public virtual OrderBook GetLtcOrderBook()
        {
            return _restExchange == null ? null : _restExchange.GetLtcOrderBook();
        }

        public virtual OrderBook GetXrpOrderBook()
        {
            return _restExchange == null ? null : _restExchange.GetXrpOrderBook();
        }

        public virtual OrderBook GetDashOrderBook()
        {
            return _restExchange == null ? null : _restExchange.GetDashOrderBook();
        }
    }
}
