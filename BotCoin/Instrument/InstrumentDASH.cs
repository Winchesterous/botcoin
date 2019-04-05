using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Exchange;
using System;
using System.Collections.Generic;

namespace BotCoin.Instruments
{
    public class InstrumentDASH : Instrument
    {
        public InstrumentDASH() : base()
        {
            BalanceStep  = 0.15;
            //AvgAmount    = TradingStrategy.AvgDashAmount;
        }

        public override double GetCryptoBalance(IExchange ex)
        {
            return ex.DashBalance;
        }

        public override CurrencyName GetInstrument()
        {
            return CurrencyName.DSH;
        }

        public override void SubscribeOrderBook(IExchange ex, Action<ExchangeName, OrderBookEventArgs> handler)
        {
            ((IExchangeEvents)ex).OnDashOrderBook += OnOrderBook;
            _orderBookHandler = handler;
        }

        public override void UnsubscribeOrderBook(IExchange ex)
        {
            ((IExchangeEvents)ex).OnDashOrderBook -= OnOrderBook;
        }

        private void OnOrderBook(object sender, OrderBookEventArgs e)
        {
            OnOrderBook(sender, e, CurrencyName.DSH);
        }

        private void OnTrade(object sender, TradeEventArgs e)
        {
            OnTrade(sender, e, CurrencyName.DSH);
        }

        public override void InitExchangePrice(BaseExchange ex, ExchangePricesEventArgs args)
        {
            args.DashPrice = new double[] { GetBidOrder(ex).Price, GetAskOrder(ex).Price };
            args.DashAmount = new double[2][] { GetBidOrder(ex).Amount, GetAskOrder(ex).Amount };
        }

        public override OrderInfo CreateBidOrder(IExchange ex, out double expectedAmount)
        {
            expectedAmount = GetBalanceStep();
            ex.DashBidOrder = new ExchangeDashOrder(expectedAmount);
            GetBidOrder(ex).Amount = new double[2];
            return new OrderInfo();
        }

        public override OrderInfo CreateAskOrder(IExchange ex, out double expectedAmount)
        {
            expectedAmount = GetBalanceStep();
            ex.DashAskOrder = new ExchangeDashOrder(expectedAmount);
            GetAskOrder(ex).Amount = new double[2];
            return new OrderInfo();
        }

        public override ExchangeOrder GetBidOrder(IExchange ex)
        {
            return ex.DashBidOrder;
        }

        protected override ExchangeOrder GetAskOrder(IExchange ex)
        {
            return ex.DashAskOrder;
        }

        public override OrderSide GetOrderType(ExchangePricesEventArgs args)
        {
            Utils.ThrowIf(args.DashPrice == null, "DashPrice null");
            Utils.ThrowIf(args.DashPrice[0] == 0 && args.DashPrice[1] == 0, "DashPrice zeros");

            if (args.DashPrice[0] != 0 && args.DashPrice[1] != 0)
                return OrderSide.Both;

            if (args.DashPrice[0] != 0)
                return OrderSide.BID;

            return OrderSide.ASK;
        }

        public override double GetPrice(ExchangePricesEventArgs args, int index)
        {
            return args.DashPrice[index];
        }

        public override double[] GetAmount(ExchangePricesEventArgs args, int index)
        {
            return args.DashAmount[index];
        }

        public override void CreateOrder(IExchange ex, OrderSide type, ExchangePricesEventArgs args)
        {
            if (type == OrderSide.BID)
                ex.DashBidOrder = new ExchangeDashOrder(args, this, type);
            else if (type == OrderSide.ASK)
                ex.DashAskOrder = new ExchangeDashOrder(args, this, type);
            else
                Utils.ThrowIf(true, "BOTH order type");
        }

        public override Dictionary<string, double> InitTempOrderBook(ExchangePricesEventArgs data)
        {
            return new Dictionary<string, double>
            {
                { "@BidPrice", data.DashPrice[0] },
                { "@AskPrice", data.DashPrice[1] },
                { "@OrderBidAmount", data.DashAmount[0][1] },
                { "@OrderAskAmount", data.DashAmount[1][1] }
            };
        }
    }
}
