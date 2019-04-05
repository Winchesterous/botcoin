using BotCoin.Instruments;

namespace BotCoin.DataType
{
    public class ExchangeEthOrder : ExchangeOrder
    {
        public ExchangeEthOrder(double defaultAmount) 
            : base(defaultAmount)
        {
        }

        public ExchangeEthOrder(ExchangePricesEventArgs arg, Instrument ins, OrderSide type)
            : base(arg, ins, type)
        {
        }        
    }
}
