using System.Collections.Generic;

namespace BotCoin.DataType.Exchange
{
    public class CexAuth
    {
        public string key { set; get; }
        public string signature { set; get; }
        public long timestamp { set; get; }
    }

    public class CexRequest
    {
        public string e { set; get; }
        public CexAuth auth { set; get; }
    }

    public class CexResponse
    {
        public string e { set; get; }
        public long time { set; get; }
        public string reason { set; get; }
    }

    public class CexOrderBookRequest : CexRequest
    {
        public class CexOrderBookData
        {
            public string[] pair { set; get; }
            public bool subscribe { set; get; }
            public int depth { set; get; }
        }
        public CexOrderBookData data { set; get; }
        public string oid { set; get; }
    }

    public class CexOrderBookResponse : CexResponse
    {
        public class CexOrderBookData
        {
            public string error { set; get; }
            public long timestamp { set; get; }
            public string pair { set; get; }
            public long time { set; get; }
            public long id { set; get; }
            public double sell_total { set; get; }
            public double buy_total { set; get; }
            public double[][] asks { set; get; }
            public double[][] bids { set; get; }
        }
        public CexOrderBookData data { set; get; }
        public string oid { set; get; }
        public string ok { set; get; }
    }

    class CexAuthResponse : CexResponse
    {
        public class CexData
        {
            public string ok { set; get; }
            public string error { set; get; }
        }
        public string ok { set; get; }
        public CexData data { set; get; }
    }
}
