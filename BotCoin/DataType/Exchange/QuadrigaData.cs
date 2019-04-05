using Newtonsoft.Json;
using System;

namespace BotCoin.DataType.Exchange
{
    public class QuadrigaUserInfo
    {
        public class QuadrigaFees
        {
            public double Btc_cad { set; get; }
            public double Eth_cad { set; get; }
            public double Ltc_cad { set; get; }
            public double Bch_cad { set; get; }
        }
        public double Fee { set; get; }
        public QuadrigaFees Fees { set; get; }
        [JsonProperty("btc_available")]
        public double BTC { set; get; }
        [JsonProperty("bch_available")]
        public double BCH { set; get; }
        [JsonProperty("eth_available")]
        public double ETH { set; get; }
        [JsonProperty("ltc_available")]
        public double LTC { set; get; }
        [JsonProperty("cad_available")]
        public double CAD { set; get; }
        [JsonProperty("usd_available")]
        public double USD { set; get; }
    }

    public class QuadrigaApiRequest
    {
        public string key { set; get; }
        public long nonce { set; get; }
        public string signature { set; get; }
    }

    public class QuadrigaCancelOrderRequest : QuadrigaApiRequest
    {
        public string id { set; get; }
    }

    public class QuadrigaLimitOrderRequest : QuadrigaApiRequest
    {
        public double amount { set; get; }
        public double price { set; get; }
        public string book { set; get; }
    }

    public class QuadrigaError
    {
        public int Code { set; get; }
        public string Message { set; get; }
    }

    public class QuadrigaOrderResponse
    {        
        [JsonProperty("id")]
        public string OrderId { set; get; }
        public DateTime Datetime { set; get; }
        public int Type { set; get; }
        public double Price { set; get; }
        public double Amount { set; get; }
        public string Book { set; get; }
        public int Status { set; get; }
        public QuadrigaError Error { set; get; }
    }
}
