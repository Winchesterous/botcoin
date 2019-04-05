using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Exchange;
using System;
using System.Collections.Generic;

namespace BotCoin.Instruments
{
    public class InstrumentLTC : Instrument
    {
        public InstrumentLTC() : base()
        {
            BalanceStep  = 0.5;
            //AvgAmount    = TradingStrategy.AvgLtcAmount;
        }

        public override double GetCryptoBalance(IExchange ex)
        {
            return ex.LtcBalance;
        }

        public override CurrencyName GetInstrument()
        {
            return CurrencyName.LTC;
        }

        public override void SubscribeOrderBook(IExchange ex, Action<ExchangeName, OrderBookEventArgs> handler)
        {
            ((IExchangeEvents)ex).OnLtcOrderBook += OnOrderBook;
            _orderBookHandler = handler;
        }

        public override void UnsubscribeOrderBook(IExchange ex)
        {
            ((IExchangeEvents)ex).OnLtcOrderBook -= OnOrderBook;
        }

        private void OnOrderBook(object sender, OrderBookEventArgs e)
        {
            OnOrderBook(sender, e, CurrencyName.LTC);
        }

        private void OnTrade(object sender, TradeEventArgs e)
        {
            OnTrade(sender, e, CurrencyName.LTC);
        }

        public override void InitExchangePrice(BaseExchange ex, ExchangePricesEventArgs args)
        {
            args.LtcPrice = new double[] { GetBidOrder(ex).Price, GetAskOrder(ex).Price };
            args.LtcAmount = new double[2][] { GetBidOrder(ex).Amount, GetAskOrder(ex).Amount };
        }

        public override OrderInfo CreateBidOrder(IExchange ex, out double expectedAmount)
        {
            expectedAmount = GetBalanceStep();
            ex.LtcBidOrder = new ExchangeLtcOrder(expectedAmount);
            GetBidOrder(ex).Amount = new double[2];
            return new OrderInfo();
        }

        public override OrderInfo CreateAskOrder(IExchange ex, out double expectedAmount)
        {
            expectedAmount = GetBalanceStep();
            ex.LtcAskOrder = new ExchangeLtcOrder(expectedAmount);
            GetAskOrder(ex).Amount = new double[2];
            return new OrderInfo();
        }

        public override ExchangeOrder GetBidOrder(IExchange ex)
        {
            return ex.LtcBidOrder;
        }

        protected override ExchangeOrder GetAskOrder(IExchange ex)
        {
            return ex.LtcAskOrder;
        }

        public override OrderSide GetOrderType(ExchangePricesEventArgs args)
        {
            Utils.ThrowIf(args.LtcPrice == null, "LtcPrice null");
            Utils.ThrowIf(args.LtcPrice[0] == 0 && args.LtcPrice[1] == 0, "LtcPrice zeros");

            if (args.LtcPrice[0] != 0 && args.LtcPrice[1] != 0)
                return OrderSide.Both;

            if (args.LtcPrice[0] != 0)
                return OrderSide.BID;

            return OrderSide.ASK;
        }

        public override double GetPrice(ExchangePricesEventArgs args, int index)
        {
            return args.LtcPrice[index];
        }

        public override double[] GetAmount(ExchangePricesEventArgs args, int index)
        {
            return args.LtcAmount[index];
        }

        public override void CreateOrder(IExchange ex, OrderSide type, ExchangePricesEventArgs args)
        {
            if (type == OrderSide.BID)
                ex.LtcBidOrder = new ExchangeLtcOrder(args, this, type);
            else if (type == OrderSide.ASK)
                ex.LtcAskOrder = new ExchangeLtcOrder(args, this, type);
            else
                Utils.ThrowIf(true, "BOTH order type");
        }

        public override Dictionary<string, double> InitTempOrderBook(ExchangePricesEventArgs data)
        {
            return new Dictionary<string, double>
            {
                { "@BidPrice", data.LtcPrice[0] },
                { "@AskPrice", data.LtcPrice[1] },
                { "@OrderBidAmount", data.LtcAmount[0][1] },
                { "@OrderAskAmount", data.LtcAmount[1][1] }
            };
        }
    }
}
