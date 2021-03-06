﻿using System;

namespace BotCoin.DataType.Exchange
{
    //public class XBtcLoginRequestArgs
    //{
    //    public string AuthType { set; get; }
    //    public string WebApiId { set; get; }
    //    public string WebApiKey { set; get; }
    //    public long Timestamp { set; get; }
    //    public string Signature { set; get; }
    //    public string DeviceId { set; get; }
    //    public string AppSessionId { set; get; }
    //}

    //public class XBtcSubscribeRequestArgs
    //{
    //    public XBtcRequestItem[] Subscribe { set; get; }
    //}

    //public class XBtcUnsubscribeRequestArgs
    //{
    //    public string[] Unsubscribe { set; get; }
    //}

    //public class XBtcRequestItem
    //{
    //    public string Symbol { set; get; }
    //    public int BookDepth { set; get; }
    //}

    public class BtccLoginRequest
    {
        public string ClientRequestId { set; get; }
        public string Date { set; get; }
        public string Account { set; get; }
        public string Signature { set; get; }
    }

    public class BtccOrderBookRequest
    {
        public string Symbol { set; get; }
        public int QuoteType { set; get; }
    }

    //public class XBtcSubscribeRequest : XBtcRequest
    //{
    //    public XBtcSubscribeRequestArgs Params { set; get; }
    //}

    //public class XBtcUnsubscribeRequest : XBtcRequest
    //{
    //    public XBtcUnsubscribeRequestArgs Params { set; get; }
    //}

    public class BtccResponse
    {
        public string MsgType { set; get; }
    }

    //public class XBtcResponseResult
    //{
    //    public string Info { set; get; }
    //    public bool TwoFactorFlag { set; get; }
    //    public XBtcSnapshot[] Snapshot { set; get; }
    //    public long Timestamp { set; get; }
    //    public string Symbol { set; get; }
    //    public XBtcOrder[] Bids { set; get; }
    //    public XBtcOrder[] Asks { set; get; }
    //    public XBtcOrder BestBid { set; get; }
    //    public XBtcOrder BestAsk { set; get; }
    //}

    //public class XBtcSnapshot
    //{
    //    public long Timestamp { set; get; }
    //    public string Symbol { set; get; }
    //    public XBtcOrder[] Bids { set; get; }
    //    public XBtcOrder[] Asks { set; get; }
    //    public XBtcOrder BestBid { set; get; }
    //    public XBtcOrder BestAsk { set; get; }
    //}
    //public class XBtcOrder
    //{
    //    public double Volume { set; get; }
    //    public double Price { set; get; }
    //}
}
