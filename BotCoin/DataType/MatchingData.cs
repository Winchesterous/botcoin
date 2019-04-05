using BotCoin.Exchange;
using MessagePack;
using System;

namespace BotCoin.DataType
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class MatchingData
    {
        readonly int CryptoCurrencyDecimal = 8;

        public MatchingData()
        {
            Balance = new double[2];
        }

        public double Amount
        {
            set { _amount = Math.Round(value, CryptoCurrencyDecimal); }
            get { return _amount; }
        }
        double _amount;
        public DateTime CreatedAt { set; get; }
        public ExchangeName Exchange1 { set; get; }
        public ExchangeName Exchange2 { set; get; }
        public double ProfitRatio { set; get; }
        public double Profit { set; get; }
        public double Fees { set; get; }        
        public double AskAmount { set; get; }
        public double BidAmount { set; get; }
        public double[] Balance { set; get; }
        public double[] Btc { set; get; }
        public double[] Bch { set; get; }
        public double[] Eth { set; get; }
        public double[] Ltc { set; get; }
        public double[] BtcBalance { set; get; }
        public double[] BchBalance { set; get; }
        public double[] EthBalance { set; get; }
        public double[] LtcBalance { set; get; }        
        public double BidPrice2 { set; get; }
        public double AskPrice1 { set; get; }
        public double CurrencyRate1 { set; get; }
        public double CurrencyRate2 { set; get; }
        public CurrencyName Instrument { set; get; }
        public TradingState TransactionState { set; get; }
        public string Order1 { set; get; }
        public string Order2 { set; get; }
        public string FailReason1 { set; get; }
        public string FailReason2 { set; get; }
        public double BuyUsdAmount { set; get; }
        public double SellUsdAmount { set; get; }

        [IgnoreMember]
        public string TransCode
        {
            get { return TransactionState.ToString(); }
        }

        [IgnoreMember]
        public double BuyAmount
        {
            get { return BuyUsdAmount * CurrencyRate1; }
        }

        [IgnoreMember]
        public double SellAmount
        {
            get { return SellUsdAmount * CurrencyRate2; }
        }

        [IgnoreMember]
        public bool HasRejectedState
        {
            get
            {
                return //TransactionState == TransactionState.Reject ||
                       TransactionState == TradingState.NoUsd ||
                       TransactionState == TradingState.NoBalance ||
                       TransactionState == TradingState.NoCrypt ||
                       //TransactionState == TransactionState.RejectPrice ||
                       TransactionState == TradingState.NoProfit;
            }
        }

        public void SetExchange1(IExchange ex1)
        {
            CurrencyRate1 = ((BaseExchange)ex1).CurrencyRate;
            Exchange1     = ex1.GetExchangeName();
        }

        public void SetExchange2(IExchange ex2)
        {
            CurrencyRate2 = ((BaseExchange)ex2).CurrencyRate;
            Exchange2     = ex2.GetExchangeName();
        }
    }
}
