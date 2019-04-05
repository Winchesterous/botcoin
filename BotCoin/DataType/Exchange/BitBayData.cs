using Newtonsoft.Json;

namespace BotCoin.DataType.Exchange
{
    public class BitBayError
    {
        public int Code { set; get; }
        public string Message { set; get; }
    }

    public class BitBayUserInfo : BitBayError
    {
        public class BitBayBalance
        {
            public double Available { set; get; }
            public double Locked { set; get; }
        }
        public class BitBayBalances
        {
            public BitBayBalance BTC { set; get; }
            public BitBayBalance ETH { set; get; }
            public BitBayBalance DASH { set; get; }
            public BitBayBalance XRP { set; get; }
            public BitBayBalance LTC { set; get; }
            public BitBayBalance BCC { set; get; }
            public BitBayBalance PLN { set; get; }
            public BitBayBalance USD { set; get; }
        }
        public class BitBayAddress
        {
            public string BTC { set; get; }
        }
        public bool Success { get; set; }
        public double Fee { set; get; }
        public BitBayAddress Addresses { get; set; }
        public BitBayBalances Balances { get; set; }
    }

    public class BitBayPlaceOrder
    {
        public bool Success { set; get; }
        [JsonProperty("order_id")]
        public string OrderId { set; get; }
        public double? Amount { set; get; }
        public bool Rate { set; get; }
        public bool Price { set; get; }
        public double Fee { set; get; }
        //"wrong" : null, "bought" : null
    }
}
