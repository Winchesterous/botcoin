using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using System;
using System.Net;

namespace BotCoin.Exchange
{
    public class BaseRestExchange : BaseExchange, IExchangeEvents
    {
        public const int WaitWhenException = 30;

        protected object _obj = new object();
        DateTime _exceptionExpired = DateTime.Now;

        public event EventHandler<OrderBookEventArgs> OnBtcOrderBook;
        public event EventHandler<OrderBookEventArgs> OnBchOrderBook;
        public event EventHandler<OrderBookEventArgs> OnLtcOrderBook;
        public event EventHandler<OrderBookEventArgs> OnEthOrderBook;
        public event EventHandler<OrderBookEventArgs> OnXrpOrderBook;
        public event EventHandler<OrderBookEventArgs> OnDashOrderBook;

        protected BaseRestExchange(ExchangeSettingsData setting) : base(setting)
        {
        }

        public override void Logout()
        {
            _account.Stop();
            lock (_obj)
            {
                OnBtcOrderBook = null;
                OnBchOrderBook = null;
                OnEthOrderBook = null;
                OnLtcOrderBook = null;
                OnXrpOrderBook = null;
                OnDashOrderBook = null;
            }
        }

        public void RestRequestHandler(CurrencyName instrument)
        {
            if (_exceptionExpired > DateTime.Now)
                return;
            lock (this)
            {
                try
                {
                    if (OnBtcOrderBook != null && instrument == CurrencyName.BTC)
                    {
                        var response = GetBtcOrderBook();
                        lock (_obj)
                            if (OnBtcOrderBook != null)
                                OnBtcOrderBook(this, new OrderBookEventArgs { OrderBook = response });
                    }
                    if (OnBchOrderBook != null && instrument == CurrencyName.BCH)
                    {
                        var response = GetBchOrderBook();
                        if (response != null)
                        {
                            lock (_obj)
                                if (OnBchOrderBook != null)
                                    OnBchOrderBook(this, new OrderBookEventArgs { OrderBook = response });
                        }
                    }
                    if (OnEthOrderBook != null && instrument == CurrencyName.ETH)
                    {
                        var response = GetEthOrderBook();
                        if (response != null)
                        {
                            lock (_obj)
                                if (OnEthOrderBook != null)
                                    OnEthOrderBook(this, new OrderBookEventArgs { OrderBook = response });
                        }
                    }
                    if (OnLtcOrderBook != null && instrument == CurrencyName.LTC)
                    {
                        var response = GetLtcOrderBook();
                        if (response != null)
                        {
                            lock (_obj)
                                if (OnLtcOrderBook != null)
                                    OnLtcOrderBook(this, new OrderBookEventArgs { OrderBook = response });
                        }
                    }
                    if (OnXrpOrderBook != null && instrument == CurrencyName.XRP)
                    {
                        var response = GetXrpOrderBook();
                        if (response != null)
                        {
                            lock (_obj)
                                if (OnXrpOrderBook != null)
                                    OnXrpOrderBook(this, new OrderBookEventArgs { OrderBook = response });
                        }
                    }
                    if (OnDashOrderBook != null && instrument == CurrencyName.DSH)
                    {
                        var response = GetDashOrderBook();
                        if (response != null)
                        {
                            lock (_obj)
                                if (OnDashOrderBook != null)
                                    OnDashOrderBook(this, new OrderBookEventArgs { OrderBook = response });
                        }
                    }
                }
                catch (WebException ex)
                {
                    Log.WriteError(String.Format("[{0}] {1}", GetExchangeName(), ex.Message), (int)GetExchangeName());
                    _exceptionExpired = DateTime.Now.AddSeconds(WaitWhenException);
                }
                catch (Exception ex)
                {
                    Log.WriteError(String.Format("[{0}] Terminated! {1}: {2}", GetExchangeName(), ex.GetType(), ex.StackTrace), (int)GetExchangeName());
                    _exceptionExpired = DateTime.Now.AddYears(1);
                }
            }
        }

        protected virtual Trade GetBtcTrade()
        {
            throw new NotSupportedException();
        }

        protected virtual Trade GetBchTrade()
        {
            return null;
        }

        protected virtual Trade GetLtcTrade()
        {
            return null;
        }

        protected virtual Trade GetEthTrade()
        {
            return null;
        }
    }
}
