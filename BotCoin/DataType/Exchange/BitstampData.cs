using Newtonsoft.Json;
using System;

namespace BotCoin.DataType.Exchange
{
    public class BitstampTrade
    {
        public long Id { set; get; }
        public long Timestamp { set; get; }
        public double Amount { set; get; }
        public double Price { set; get; }
        public int Type { set; get; }

        [JsonProperty("buy_order_id")]
        public long BuyerOrderId { set; get; }

        [JsonProperty("sell_order_id")]
        public long SellerOrderId { set; get; }

        public string TradeType
        {
            get { return Type == 0 ? "Buy" : "Sell"; }
        }
    }

    public class ExchangeConfiguration
    {
        [JsonProperty("base_decimals")]
        public int BaseDecimals { set; get; }

        [JsonProperty("counter_decimals")]
        public int CounterDecimals { set; get; }

        [JsonProperty("minimum_order")]
        public string MinimumOrder { set; get; }

        [JsonProperty("url_symbol")]
        public string UrlSymbol { set; get; }

        public int CryptoCurrencyDecimal { get { return BaseDecimals; } }

        public int CountryCurrencyDecimal { get { return CounterDecimals; } }

        public double MinOrderSize(CurrencyName currency)
        {
            int index = MinimumOrder.IndexOf(currency.ToString());
            return Double.Parse(MinimumOrder.Substring(0, index).Trim());
        }
    }

    public class BitstampAccount
    {
        #region Error
        public string Code { set; get; }
        public string Status { set; get; }
        public string Reason { set; get; }
        public bool HasError
        {
            get
            {
                return String.Compare(Status, "error", true) == 0;
            }
        }
        #endregion
        #region BTC
        [JsonProperty("btc_available")]
        public double BtcAvailable { set; get; }
        [JsonProperty("btc_balance")]
        public double BtcBalance { set; get; }
        [JsonProperty("btcusd_fee")]
        public double FeeBtcUsd { set; get; }
        #endregion
        #region BCH
        [JsonProperty("bch_balance")]
        public double BchBalance { set; get; }
        [JsonProperty("bch_available")]
        public double BchAvailable { set; get; }
        [JsonProperty("bchusd_fee")]
        public double FeeBchUsd { set; get; }
        #endregion
        #region LTC
        [JsonProperty("ltc_available")]
        public double LtcAvailable { set; get; }
        [JsonProperty("ltcusd_fee")]
        public double FeeLtcUsd { set; get; }
        #endregion
        #region ETH
        [JsonProperty("eth_available")]
        public double EthAvailable { set; get; }
        [JsonProperty("Ethusd_fee")]
        public double FeeEthUsd { set; get; }
        #endregion
        #region XRP
        [JsonProperty("xrp_available")]
        public double XrpAvailable { set; get; }
        [JsonProperty("xrpusd_fee")]
        public double FeeXrpUsd { set; get; }
        #endregion
        #region USD        
        [JsonProperty("usd_available")]
        public double UsdAvailable { set; get; }
        [JsonProperty("usd_balance")]
        public double UsdBalance { set; get; }
        #endregion
    }

    public class BitstampError
    {
        public class BitstampErrorReason
        {
            [JsonProperty("__all__")]
            public string[] All { set; get; }
        }
        public string Error { set; get; }
        public string Status { set; get; }
        public string Code { set; get; }
        public BitstampErrorReason Reason { set; get; }
    }

    public class BitstampOrderResponse : BitstampError
    {
        [JsonProperty("id")]
        public string OrderId { set; get; }
        public float Amount { set; get; }
        public float Price { set; get; }
        public DateTime Datetime { set; get; }
    }

    public class BitstampOrderData
    {
        [JsonProperty("id")]
        public string OrderId { set; get; }

        [JsonProperty("type")]
        public string OrderType { set; get; }
        public DateTime DateTime { set; get; }
        public double Price { set; get; }
        public double Amount { set; get; }
        public string Error { set; get; }
        public string Reason { set; get; }
        public bool IsBuyOrder
        {
            get { return String.Compare(OrderType, "Buy", true) == 0; }
        }
    }

    public class BitstampTicker
    {
        public double Vwap { set; get; }
        public double Volume { set; get; }
        public double High { set; get; }
        public double Low { set; get; }
        public double Open { set; get; }
        public double Last { set; get; }
        public long Timestamp { set; get; }
        public double Bid { set; get; }
        public double Ask { set; get; }
        public DateTime? Time { set; get; }
    }

    public class BitstampCandle
    {
        public DateTime Time { set; get; }
        public double Open { set; get; }
        public double High { set; get; }
        public double Close { set; get; }
        public double Low { set; get; }
    }

    public class BitstampOrderBook
    {
        public long microtimestamp { set; get; }
        public string id { set; get; }
        public long datetime { set; get; }
        public double amount { set; get; }
        public double price { set; get; }
        public int order_type { set; get; }

        public bool BidOrder
        {
            get { return order_type == 0; }
        }
    }
}
