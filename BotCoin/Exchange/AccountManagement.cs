using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Logger;
using BotCoin.Service;
using System.Threading;

namespace BotCoin.Exchange
{
    public class AccountManagement
    {
        readonly CancellationPattern _cancellation;
        readonly IServiceEventLogger _log;
        readonly AutoResetEvent _event;
        readonly IExchange _ex;
        readonly object _obj;

        public AccountManagement(BaseExchange ex, IServiceEventLogger log)
        {
            CurrencyRate  = 1;
            _cancellation = new CancellationPattern();
            _event        = new AutoResetEvent(false);
            Fee           = new ExchangeFee { TradingFee = ex.TradeFee };

            _obj = new object();
            _log = log;
            _ex  = ex;                       
        }
        
        public void UpdateBalances()
        {            
            if (_ex.Currency != CurrencyName.USD)
                CurrencyRate = _ex.DbGateway.GetCurrencyRate(_ex.Currency);

            var account = _ex.DbGateway.GetLastBalances(_ex.GetExchangeName());

            UsdBalance  = account.UsdBalance;
            BtcBalance  = account.BtcBalance;
            BchBalance  = account.BchBalance.HasValue ? account.BchBalance.Value : 0;
            LtcBalance  = account.LtcBalance.HasValue ? account.LtcBalance.Value : 0;
            EthBalance  = account.EthBalance.HasValue ? account.EthBalance.Value : 0;
            XrpBalance  = account.XrpBalance.HasValue ? account.XrpBalance.Value : 0;
            DashBalance = account.DashBalance.HasValue ? account.DashBalance.Value : 0;

            Utils.ThrowIf(UsdBalance < 0, "[{0}] USD balance {1}", _ex.GetExchangeName(), UsdBalance);
            Utils.ThrowIf(BtcBalance < 0, "[{0}] BTC balance {1}", _ex.GetExchangeName(), BtcBalance);
            Utils.ThrowIf(BchBalance < 0, "[{0}] BCH balance {1}", _ex.GetExchangeName(), BchBalance);
            Utils.ThrowIf(LtcBalance < 0, "[{0}] LTC balance {1}", _ex.GetExchangeName(), LtcBalance);
            Utils.ThrowIf(EthBalance < 0, "[{0}] ETH balance {1}", _ex.GetExchangeName(), EthBalance);
            Utils.ThrowIf(XrpBalance < 0, "[{0}] XRP balance {1}", _ex.GetExchangeName(), XrpBalance);
            Utils.ThrowIf(DashBalance < 0, "[{0}] DASH balance {1}", _ex.GetExchangeName(), DashBalance);
        }

        #region Properties
        public ExchangeFee Fee { private set; get; }
        public double CurrencyRate { private set; get; }
        public double UsdBalance { set; get; }
        public double BtcBalance { set; get; }
        public double BchBalance { set; get; }
        public double LtcBalance { set; get; }
        public double EthBalance { set; get; }
        public double XrpBalance { set; get; }
        public double DashBalance { set; get; }
        public double IotaBalance { set; get; }
        #endregion

        public void Stop()
        {
            _cancellation.Cancel();
            _event.Set();
        }
    }
}
