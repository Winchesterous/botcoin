using BotCoin.Instruments;

namespace BotCoin.DataType
{
    public class ExchangeBtcOrder : ExchangeOrder
    {
        public ExchangeBtcOrder(double defaultAmount) 
            : base(defaultAmount)
        {
        }

        public ExchangeBtcOrder(ExchangePricesEventArgs arg, Instrument ins, OrderSide type)
            : base(arg, ins, type)
        {
        }        
    }
}
