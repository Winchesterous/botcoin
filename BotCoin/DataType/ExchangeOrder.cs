using BotCoin.Instruments;
using System;

namespace BotCoin.DataType
{
    public class ExchangeOrder
    {
        public readonly string Id;

        public ExchangeOrder(double defaultAmount)
        {
            DefaultAmount = defaultAmount;
        }

        public ExchangeOrder(ExchangePricesEventArgs args, Instrument ins, OrderSide type)
        {
            Id = args.OrderId;
            Instrument = args.Instrument1;
            ins.InitExchangePrice(this, args, type);
        }

        public double DefaultAmount { private set; get; }
        public CurrencyName Instrument { set; get; }
        public double Price { set; get; }
        public double[] Amount { set; get; }
    }
}
