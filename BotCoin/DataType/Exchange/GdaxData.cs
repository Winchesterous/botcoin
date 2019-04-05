using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BotCoin.DataType.Exchange
{
    public class GdaxError
    {
        public string Message { set; get; }
        public bool HasError { get { return !String.IsNullOrEmpty(Message); } }
    }

    public class GdaxChannel : GdaxError
    {
        public string Name { set; get; }
        [JsonProperty("product_ids")]
        public string[] Symbols { set; get; }
    }

    public class GdaxRequest
    {
        [JsonProperty("product_ids")]
        public string[] Symbols { set; get; }
        [JsonProperty("channels")]
        public object[] Channels { set; get; }
        [JsonProperty("type")]
        public string Type { set; get; }
    }

    public class GdaxTicker : GdaxError
    {
        [JsonProperty("last_size")]
        public double? LastSize { set; get; }
        [JsonProperty("best_ask")]
        public double? BestAsk { set; get; }
        [JsonProperty("best_bid")]
        public double? BestBid { set; get; }
        public long Sequence { set; get; }
        public DateTime? Time { set; get; }
        public string Type { set; get; }
        public string Side { set; get; }  
        public double? Price { set; get; }        
    }

    public class GdaxResponse : GdaxTicker
    {
        [JsonProperty("product_id")]
        public string ProductId { set; get; }
        [JsonProperty("order_id")]
        public string OrderId { set; get; }
        [JsonProperty("order_type")]        
        public string OrderType { set; get; }
        [JsonProperty("remaining_size")]
        public double? RemainingSize { set; get; }
        [JsonProperty("maker_order_id")]
        public string MakerOrderId { set; get; }
        [JsonProperty("taker_order_id")]
        public string TakerOrderId { set; get; }
        [JsonProperty("stop_type")]
        public string StopType { set; get; }
        [JsonProperty("stop_price")]
        public double? StopPrice { set; get; }
        [JsonProperty("new_size")]
        public double? NewSize { set; get; }
        [JsonProperty("old_size")]
        public double? OldSize { set; get; }
        [JsonProperty("new_funds")]
        public double? NewFunds { set; get; }
        [JsonProperty("old_funds")]
        public double? OldFunds { set; get; }                
        public DateTime? Timestamp { set; get; }
        public string Reason { set; get; }              
        public double? Funds { set; get; }
        public double? Size { set; get; }

        public bool IsMarketOrder
        {
            get { return String.Compare(OrderType, "market", true) == 0; }
        }
        public bool IsBuyOrder
        {
            get { return String.Compare(Side, "buy", true) == 0; }
        }

        public DynamicParameters ToSqlParameters()
        {
            var p = new DynamicParameters();
            Action setPriceQty = () =>
            {
                if (String.Compare(OrderType, "market", true) == 0)
                {
                    if (String.Compare(Side, "buy", true) == 0)
                    {
                        if (Funds.HasValue) p.Add("@BidFunds", Funds.Value);
                        if (Size.HasValue) p.Add("@BidQty", Size.Value);
                    }
                    else
                    {
                        if (Funds.HasValue) p.Add("@AskFunds", Funds.Value);
                        if (Size.HasValue) p.Add("@AskQty", Size.Value);
                    }
                }
                else
                {
                    if (String.Compare(Side, "buy", true) == 0)
                    {
                        if (Size.HasValue) p.Add("@BidQty", Size.Value);
                        if (Price.HasValue) p.Add("@BidPrice", Price.Value);
                    }
                    else
                    {
                        if (Size.HasValue) p.Add("@AskQty", Size.Value);
                        if (Price.HasValue) p.Add("@AskPrice", Price.Value);
                    }
                }
            };
            p.Add("@Timestamp", Time.Value);
            p.Add("@CreatedAt", DateTime.UtcNow);
			p.Add("@Sequence", Sequence);
            p.Add("@Instrument1", CurrencyName.BTC.ToString());
            p.Add("@Instrument2", CurrencyName.USD.ToString());
            p.Add("@OrderId", OrderId);

            if (Type == "activate")
                OrderType = "stop";

            if (String.IsNullOrEmpty(OrderType))
                OrderType = "Limit";
            else if (String.Compare(OrderType, "limit", true) == 0)
                OrderType = "Limit";
            else if (String.Compare(OrderType, "market", true) == 0)
                OrderType = "Market";
            
            p.Add("@OrderType", OrderType);
            
            if (!String.IsNullOrEmpty(StopType))
            {
                p.Add("@OrderType", "stop");
            }
            if (Type == "received")
            {
                p.Add("@OrderState", "Recv");
                setPriceQty();
            }
            else if (Type == "open")
            {
                p.Add("@OrderState", "Open");
                if (RemainingSize.HasValue)
                {
                    Size = RemainingSize.Value;
                    p.Add("@IsFullyFilled", Size == 0);
                }
                setPriceQty();
            }
            else if (Type == "done")
            {
                p.Add("@OrderState", "Done");
                p.Add("@OrderReason", Reason);
                if (RemainingSize.HasValue)
                {
                    Size = RemainingSize.Value;
                    p.Add("@IsFullyFilled", Size == 0);
                }
                setPriceQty();
            }
            else if (Type == "match")
            {
                p.Add("@OrderState", "Match");
                p.Add("@MakerOrderId", MakerOrderId);
                p.Add("@TakerOrderId", TakerOrderId);
                setPriceQty();
            }
            else if (Type == "change")
            {
                p.Add("@OrderState", "Change");
                if (String.Compare(OrderType, "market", true) == 0)
                {
                    if (String.Compare(Side, "buy", true) == 0)
                    {
                        p.Add("@BidFunds", NewFunds.Value);
                        p.Add("@AskFunds", OldFunds.Value);
                    }
                    else
                    {
                        p.Add("@AskFunds", NewFunds.Value);
                        p.Add("@BidFunds", OldFunds.Value);
                    }
                }
                else
                {
                    if (String.Compare(Side, "buy", true) == 0)
                    {
                        p.Add("@BidPrice", Price.Value);
                        p.Add("@BidQty", NewSize.Value);
                        p.Add("@AskQty", OldSize.Value);
                    }
                    else
                    {
                        p.Add("@AskPrice", Price.Value);
                        p.Add("@AskQty", NewSize.Value);
                        p.Add("@BidQty", OldSize.Value);
                    }
                }
            }
            else if (Type == "activate")
            {
                p.Add("@OrderState", "Activate");
                p.Add("@StopType", StopType);

                if (StopPrice.HasValue) p.Add("@StopPrice", StopPrice.Value);
                if (Funds.HasValue) p.Add("@Funds", Funds.Value);
                setPriceQty();
            }
            return p;
        }
    }
}
