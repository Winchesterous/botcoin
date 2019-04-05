using Newtonsoft.Json;

namespace BotCoin.DataType.Exchange
{
    public class WexError
    {
        public bool Success { get; set; }
        public string Error { set; get; }
    }

    public class WexUserInfo : WexError
    {
        public class WexResult
        {
            public int Open_orders { set; get; }
            public long Server_time { set; get; }
            public WexRight Rights { set; get; }
            public WexFund Funds { set; get; }
        }
        public class WexFund
        {
            public double Usd { set; get; }
            public double Usdt { set; get; }
            public double Btc { set; get; }
            public double Eth { set; get; }
            public double Ltc { set; get; }
            public double Dsh { set; get; }
            public double Bch { set; get; }
        }
        public class WexRight
        {
            public int Info { set; get; }
            public int Trade { set; get; }
            public int Withdraw { set; get; }
        }
        public WexResult Return { get; set; }        
    }    

    public class WexOrderResponse : WexError
    {
        public class WexOrderResponseItem
        {
            [JsonProperty("order_id")]
            public long OrderId { set; get; }
            public double Received { set; get; }
            public double Remains { set; get; }            
        }
        public WexOrderResponseItem[] Return { set; get; }
    }
}
