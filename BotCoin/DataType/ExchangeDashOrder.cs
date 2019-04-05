using BotCoin.Instruments;

namespace BotCoin.DataType
{
    public class ExchangeDashOrder : ExchangeOrder
    {
        public ExchangeDashOrder(double defaultAmount) 
            : base(defaultAmount)
        {
        }

        public ExchangeDashOrder(ExchangePricesEventArgs arg, Instrument ins, OrderSide type)
            : base(arg, ins, type)
        {
        }        
    }
}
