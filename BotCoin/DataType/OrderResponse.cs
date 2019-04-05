using BotCoin.DataType.Exchange;
using System;

namespace BotCoin.DataType
{
    public class OrderResponse
    {
        public OrderResponse()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public OrderResponse(string orderId) : this()
        {
            OrderId = orderId;
        }

        public OrderResponse(DateTime transactTime, string orderId) : this(orderId)
        {
            CreatedAt = transactTime;
        }

        public OrderResponse(BitstampOrderData order) : this(order.OrderId)
        {
            CreatedAt = order.DateTime;
            Price     = order.Price;
            Amount    = order.Amount;
            //OrderType = (OrderSide)Enum.Parse(typeof(OrderSide), order.OrderType);
        }

        public OrderResponse(QuadrigaOrderResponse order) : this(order.OrderId)
        {
            CreatedAt = order.Datetime;
            Price     = order.Price;
            Amount    = order.Amount;
        }

        public OrderResponse(KunaOrder order) : this(order.Id)
        {
            CreatedAt = order.CreatedAt;
            Price     = order.Price;
            Amount    = order.Volume;
        }

        public string OrderId { private set; get; }
        public DateTime CreatedAt { private set; get; }
        public double Price { set; get; }
        public double Amount { set; get; }
        public OrderSide OrderType { set; get; }
        public string ErrorReason { set; get; }
        public int? ErrorCode { set; get; }

        public bool Success
        {
            get { return String.IsNullOrEmpty(ErrorReason); }
        }
    }
}
