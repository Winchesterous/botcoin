using Newtonsoft.Json;
using System;

namespace BotCoin.DataType.Exchange
{
    public class BitmexResponse
    {
        public class BitmexErrorInternal
        {
            public string Name { set; get; }
            public string Message { set; get; }
        }
        public BitmexErrorInternal Error { set; get; }
        public bool HasError { get { return Error != null && Error.Name.Length > 0; } }
        public ApiResult Response
        {
            set { _response = value; _response.Content = null; }
            get { return _response; }
        }
        ApiResult _response;
    }

    public class BitmexWsCommand
    {
        [JsonProperty("op")]
        public string Command { set; get; }

        [JsonProperty("args")]
        public object[] Agruments { set; get; }
    }

    public class BitmexWsResponse : BitmexResponse
    {
        public bool Success { set; get; }
        public string Subscribe { set; get; }
        public string Table { set; get; }
        public string Action { set; get; }
        public BitmexWsCommand Request { set; get; }
    }

    public class BitmexOrder : BitmexWsResponse
    {
        public BitmexOrderData[] Data { set; get; }
    }

    public class BitmexOrderData
    {
        public string Error { set; get; }
        public string OrderId { set; get; }
        public string Symbol { set; get; }
        public string Side { set; get; }
        public string OrdType { set; get; }
        public string TimeInForce { set; get; }
        public string ExecInst { set; get; }
        public double? Price { set; get; }
        public double? StopPx { set; get; }
        public double? PegOffsetValue { set; get; }
        public string OrdStatus { set; get; }
        public string Triggered { set; get; }
        public string Text { set; get; }
        public string Message { set; get; }
        public int Account { set; get; }
        public string SettlCurrency { set; get; }
        public DateTime Timestamp { set; get; }
        public DateTime? TransactTime { set; get; }
        public int? OrderQty { set; get; }
        public int? LeavesQty { set; get; }
        public int? CumQty { set; get; }
        public double? AvgPx { set; get; }
        public bool HasError
        {
            get { return Error != null && Error.Length > 0; }
        }
        public bool IsLimitOrder
        {
            get { return String.Compare(OrdType, "Limit", true) == 0; }
        }
        public bool IsMarketOrder
        {
            get { return String.Compare(OrdType, "Market", true) == 0; }
        }
        public bool IsMarketStopOrder
        {
            get { return String.Compare(OrdType, "Stop", true) == 0; }
        }
        public bool IsLimitStopOrder
        {
            get { return String.Compare(OrdType, "StopLimit", true) == 0; }
        }
        public bool IsTakeProfitLimitOrder
        {
            get { return String.Compare(OrdType, "LimitIfTouched", true) == 0; }
        }
        public bool IsTakeProfitMarketOrder
        {
            get { return String.Compare(OrdType, "MarketIfTouched", true) == 0; }
        }
        public bool IsCreated
        {
            get { return String.Compare(OrdStatus, "New", true) == 0; }
        }
        public bool IsCanceled
        {
            get { return String.Compare(OrdStatus, "Canceled", true) == 0; }
        }
        public bool IsFilled
        {
            get { return String.Compare(OrdStatus, "Filled", true) == 0; }
        }
        public bool IsPartiallyFilled
        {
            get { return String.Compare(OrdStatus, "PartiallyFilled", true) == 0; }
        }
        public bool IsRejected
        {
            get { return String.Compare(OrdStatus, "Rejected", true) == 0; }
        }
        public bool HasTriggered
        {
            get { return String.Compare(Triggered, "StopOrderTriggered", true) == 0; }
        }
    }

    public class BitmexOrder10Book : BitmexWsResponse
    {
        public BitmexOrderBook10Data[] Data { set; get; }
    }

    public class BitmexOrderBook10Data
    {
        public string Symbol { set; get; }
        public double[][] Asks { set; get; }
        public double[][] Bids { set; get; }
    }

    public class BitmexOrderL2Book : BitmexWsResponse
    {
        public BitmexOrderBookL2Data[] Data { set; get; }
    }

    public class BitmexOrderBookL2Data
    {
        public string Symbol { set; get; }
        public string Side { set; get; }
        public long Id { set; get; }
        public long Size { set; get; }
        public double? Price { set; get; }
        public double BidPrice { set; get; }
        public double AskPrice { set; get; }
        public OrderSide OrderSide
        {
            get
            {
                if (Side[0] == 'B') return OrderSide.BID;
                if (Side[0] == 'S') return OrderSide.ASK;
                throw new InvalidOperationException("Invalid side type " + Side);
            }
        }
    }

    public class BitmexTrade : BitmexWsResponse
    {
        public BitmexTradeData[] Data { set; get; }
    }

    public class BitmexTradeData
    {
        public DateTime Timestamp { set; get; }
        public string Side { set; get; }
        public string Symbol { set; get; }
        public string TickDirection { set; get; }
        public long Size { set; get; }
        public long GrossValue { set; get; }
        public double Price { set; get; }
    }

    public class BitmexInstrument : BitmexWsResponse
    {
        public BitmexInstrumentData[] Data { set; get; }
    }

    public class BitmexInstrumentData
    {
        public string Symbol { set; get; }
        public double? OpenInterest { set; get; }
        public double? OpenValue { set; get; }
        public double? LastPrice { set; get; }
        public double? Vwap { set; get; }
        public double? HighPrice { set; get; }
        public double? LowPrice { set; get; }
        public string LastTickDirection { set; get; }
        public DateTime? FundingTimestamp { set; get; }
        public double? FundingRate { set; get; }
        public double? IndicativeFundingRate { set; get; }
        public DateTime Timestamp { set; get; }
        public long? TotalVolume { set; get; }
        public long? Volume { set; get; }
        public long? Volume24h { set; get; }
        public long? TotalTurnover { set; get; }
        public long? TurnOver { set; get; }
        public long? TurnOver24h { set; get; }
        public string HomeNotional24h { set; get; }
        public long? ForeignNotional24h { set; get; }
        public TickDirection TickDirection
        {
            get
            {
                return String.IsNullOrEmpty(LastTickDirection)
                    ? TickDirection.Undefined
                    : (TickDirection)Enum.Parse(typeof(TickDirection), LastTickDirection);
            }
        }
    }

    public class BitmexLiquidation : BitmexWsResponse
    {
        public BitmexLiquidationData[] Data { set; get; }
    }

    public class BitmexLiquidationData
    {
        public string OrderId { set; get; }
        public string Symbol { set; get; }
        public string Side { set; get; }
        public double? Price { set; get; }
        public int? LeavesQty { set; get; }
    }

    public class BitmexWallet : BitmexWsResponse
    {
        public BitmexWalletData[] Data { set; get; }
    }
        
    public class BitmexWalletData
    {
        public string Addr { set; get; }
        public DateTime Timestamp { set; get; }
        public long Account { set; get; }
        public long Amount { set; get; }
        public long DeltaAmount { set; get; }
        public long? Withdrawn { set; get; }
        public long? DeltaDeposited { set; get; }
        public long? DeltaWithdrawn { set; get; }
        public double BalanceXBT
        {
            get { return BitmexMargin.ToBtc(Amount); }
        }
    }

    public class BitmexMargin : BitmexWsResponse
    {
        private static long SatoshiXbt = 100000000;
        public static double ToBtc(long value, int digits = 8)
        {
            return Math.Round(value / (double)SatoshiXbt, digits);
        }
        public BitmexMarginData[] Data { set; get; }
    }

    public class BitmexMarginData
    {
        public DateTime Timestamp { set; get; }
        public long Account { set; get; }
        public long? WalletBalance { set; get; }
        public long? MarginBalance { set; get; }
        public long? AvailableMargin { set; get; }
        public double? MarginUsedPcnt { set; get; }
        public long? RealisedPnl { set; get; }
        public long? GrossComm { set; get; }
    }

    public class BitmexPosition : BitmexWsResponse
    {
        public BitmexPositionData[] Data { set; get; }
    }

    public class BitmexPositionData 
    {
        public string Symbol { set; get; }
        public double? Leverage { set; get; }
        public bool? CrossMargin { set; get; }
        public double? Commission { set; get; }
        public double? AvgEntryPrice { set; get; }
        public double? MarkPrice { set; get; }
        public double? LastPrice { set; get; }        
        public double? LiquidationPrice { set; get; }
        public long? CurrentQty { set; get; }        
        public long? PosCost { set; get; }
        public long? PosMargin { set; get; }        
        public long? RealisedPnl { set; get; }
        public bool? IsOpen { set; get; }
        public DateTime? Timestamp { set; get; }
        public string Message { set; get; }
        
        public long? PositionSize
        {
            get
            {
                if (CurrentQty.HasValue) return CurrentQty.Value;
                return null;
            }
        }
        public double? FeePaid
        {
            get
            {
                if (!Commission.HasValue)
                    return null;
                var size = PositionSize;
                if (!size.HasValue)
                    return null;
                return Math.Round(Commission.Value * size.Value, 8);
            }
        }
        public bool? IsCrossMargin
        {
            get
            {
                if (!CrossMargin.HasValue) return null;
                if (!Leverage.HasValue) return null;
                return CrossMargin.Value && Leverage.Value == 100;
            }
        }
        public string PositionSide
        {
            get
            {
                if (!CurrentQty.HasValue) return null;
                return CurrentQty.Value > 0 ? "Buy" : "Sell";
            }
        }
    }

    public class BitmexFunding : BitmexWsResponse
    {
        public BitmexFundingData[] Data { set; get; }
    }

    public class BitmexFundingData
    {
        public DateTime Timestamp { set; get; }
        public string Symbol { set; get; }
        public DateTime FundingInterval { set; get; }
        public double FundingRate { set; get; }
        public double FundingRateDaily { set; get; }
    }

    public class BitmexUser
    {
        public string Id { set; get; }
        public string UserName { set; get; }
        public string FirstName { set; get; }
        public string LastName { set; get; }
    }

    public class BitmexInstrumentSettings
    {
        public string Symbol { set; get; }
        public DateTime Timestamp { set; get; }        
        public double? MakerFee { set; get; }
        public double? TakerFee { set; get; }
        public double? SettlementFee { set; get; }
        public double? TickSize { set; get; }
        public int? Index { set; get; }
    }

    public class BitmexExecution : BitmexWsResponse
    {
        public BitmexTradeHistory[] Data { set; get; }
    }

    public class BitmexTradeHistory
    {
        public DateTime TransactTime { set; get; }
        public string ExecId { set; get; }
        public string OrderId { set; get; }
        public string OrdStatus { set; get; }
        public string Side { set; get; }
        public string Symbol { set; get; }
        public string Text { set; get; }
        public double? Price { set; get; }
        public double? LastPx { set; get; }
        public int OrderQty { set; get; }
        public int? LastQty { set; get; }
        public int? LeavesQty { set; get; }
        public int? CumQty { set; get; }
        public double? Commission { set; get; }
        public long? ExecComm { set; get; }
        public long? ExecCost { set; get; }
        public double? HomeNotional { set; get; }
        public long? ForeignNotional { set; get; }

        public bool IsLastCloseTrade
        {
            get { return LeavesQty.HasValue && LeavesQty.Value == 0; }
        }
    }
}
