using BotCoin.DataType;
using BotCoin.Service;
using System.Collections.Generic;

namespace BotCoin.Exchange
{
    public interface IExchange
    {
        ExchangeBtcOrder BtcAskOrder { set; get; }
        ExchangeBtcOrder BtcBidOrder { set; get; }
        ExchangeBchOrder BchBidOrder { set; get; }
        ExchangeBchOrder BchAskOrder { set; get; }
        ExchangeLtcOrder LtcBidOrder { set; get; }
        ExchangeLtcOrder LtcAskOrder { set; get; }
        ExchangeEthOrder EthBidOrder { set; get; }
        ExchangeEthOrder EthAskOrder { set; get; }
        ExchangeXrpOrder XrpBidOrder { set; get; }
        ExchangeXrpOrder XrpAskOrder { set; get; }
        ExchangeDashOrder DashBidOrder { set; get; }
        ExchangeDashOrder DashAskOrder { set; get; }
        ExchangeIotaOrder IotaBidOrder { set; get; }
        ExchangeIotaOrder IotaAskOrder { set; get; }
        double EthBalance { set; get; }
        double BtcBalance { set; get; }
        double BchBalance { set; get; }
        double LtcBalance { set; get; }
        double XrpBalance { set; get; }
        double DashBalance { set; get; }
        double IotaBalance { set; get; }
        double UsdBalance { set; get; }
        TradingState TradingState { set; get; }
        CurrencyName Currency { get; }
        double CurrencyRate { get; }
        double TradingFee { get; }
        int CryptoCurrencyDecimal { get; }
        int CountryCurrencyDecimal { get; }
        List<IExchange> Exchanges { set; get; }
        IDbRepository DbGateway { set; get; }

        void Logon();

        void Logout();

        double ConvertPriceToUsd(double price);

        double GetMinTradeValue(CurrencyName currency, double value = 0);

        void InitExchange();

        void RemoveOrders();

        UserAccount GetBalances();

        ExchangeName GetExchangeName();

        OrderResponse PlaceOrder(double price, double amount, CurrencyName currency, OrderSide side, TradeOrderType type = TradeOrderType.Limit);

        bool CancelOrder(string orderId, string symbol = null);
    }
}
