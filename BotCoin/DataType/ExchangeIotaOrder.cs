using BotCoin.Instruments;

namespace BotCoin.DataType
{
    public class ExchangeIotaOrder : ExchangeOrder
    {
        public ExchangeIotaOrder(double defaultAmount) 
            : base(defaultAmount)
        {
        }

        public ExchangeIotaOrder(ExchangePricesEventArgs arg, Instrument ins, OrderSide type)
            : base(arg, ins, type)
        {
        }        
    }
}
