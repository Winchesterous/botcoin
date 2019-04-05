using BotCoin.Instruments;
using Newtonsoft.Json;

namespace BotCoin.DataType.Exchange
{
    public class BittrexData
    {
        public Instrument Instrument1 { get; set; }
        public Instrument Instrument2 { get; set; }
        public double BaseRatio { get; set; }
    }

    public class BittrexArbitrageData
    {
        public CurrencyName Instrument1 { set; get; }
        public CurrencyName Instrument2 { set; get; }
        public ExchangeName Exchange1 { set; get; }
        public ExchangeName Exchange2 { set; get; }
        public double SellPrice { set; get; }
        public double BuyPrice { set; get; }
        public double BittrexRatio { set; get; }
        public double Ratio { set; get; }
        public double ProfitRatio { set; get; }
        public double Fees { set; get; }
        public double ProfitUsd { set; get; }
    }

    public class BittrexBalances
    {
        public class BittrexBalance
        {
            public string Currency { set; get; }
            public double Balance { set; get; }
            public double Available { set; get; }
            public double Pending { set; get; }
            public string CryptoAddress { set; get; }
            public bool Requested { set; get; }
        }
        public BittrexBalance[] Result { get; set; }
        public bool Success { set; get; }
        public string Message { set; get; }
    }

    public class BittrexError
    {
        public bool Success { set; get; }
        public string Message { set; get; }
    }

    public class BittrexOrderResponse : BittrexError
    {
        public class BittrexOrderResult
        {
            [JsonProperty("uuid")]
            public string OrderId { set; get; }
        }
        public BittrexOrderResult Result { set; get; }
    }
}