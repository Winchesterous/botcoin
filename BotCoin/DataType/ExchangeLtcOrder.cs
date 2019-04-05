using BotCoin.Instruments;

namespace BotCoin.DataType
{
    public class ExchangeLtcOrder : ExchangeOrder
    {
        public ExchangeLtcOrder(double defaultAmount) 
            : base(defaultAmount)
        {
        }

        public ExchangeLtcOrder(ExchangePricesEventArgs arg, Instrument ins, OrderSide type)
            : base(arg, ins, type)
        {
        }        
    }
}
