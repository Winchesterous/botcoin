using Newtonsoft.Json;
using System;

namespace BotCoin.DataType.Exchange
{
    public class KunaUserInfo
    {
        public class KunaAccount
        {
            public string Currency { set; get; }
            public double Balance { set; get; }
            public double Locked { set; get; }
        }
        public string Email { get; set; }
        public bool Activated { get; set; }
        public KunaAccount[] Accounts { get; set; }      
        public KunaError Error { get; set; } 
    }

    public class KunaOrdersHistory
    {
        public long Id { set; get; }
        public double Price { set; get; }
        public double Volume { set; get; }
        public double Funds { set; get; }
        public string Market { set; get; }
        public string Side { set; get; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { set; get; }
        [JsonProperty("order_id")]
        public long OrderId { set; get; }
    }

    public class KunaError
    {
        public int Code { set; get; }
        public string Message { set; get; }
        public string Error { set; get; }
        public int HttpCode { set; get; }
    }
}
