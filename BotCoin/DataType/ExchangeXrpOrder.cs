using BotCoin.Instruments;

namespace BotCoin.DataType
{
    public class ExchangeXrpOrder : ExchangeOrder
    {
        public ExchangeXrpOrder(double defaultAmount) 
            : base(defaultAmount)
        {
        }

        public ExchangeXrpOrder(ExchangePricesEventArgs arg, Instrument ins, OrderSide type)
            : base(arg, ins, type)
        {
        }        
    }
}
