namespace BotCoin.DataType.Exchange
{
    public class KrakenError
    {
        public string[] Error { set; get; }
    }

    public class KrakenBalance : KrakenError
    {
        //public string Asset
        //public double Amount
    }

    public class KrakenOrderResponse : KrakenError
    {
    }
}
