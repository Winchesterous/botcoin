using BotCoin.Instruments;

namespace BotCoin.DataType
{
    public class ExchangeBchOrder : ExchangeOrder
    {
        public ExchangeBchOrder(double defaultAmount) 
            : base(defaultAmount)
        {
        }

        public ExchangeBchOrder(ExchangePricesEventArgs arg, Instrument ins, OrderSide type)
            : base(arg, ins, type)
        {
        }        
    }
}
