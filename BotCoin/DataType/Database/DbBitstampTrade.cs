using System;

namespace BotCoin.DataType.Database
{
    public class DbBitstampTrade
    {
        public string TradeType { set; get; }
        public double Amount { set; get; }
        public int Count { set; get; }
        
        public OrderSide Side
        {
            set
            {
                _side = (OrderSide)Enum.Parse(typeof(OrderSide), value.ToString(), true); 
            }
            get
            {
                return (OrderSide)Enum.Parse(typeof(OrderSide), TradeType, true); 
            }
        }
        OrderSide _side;
    }
}
