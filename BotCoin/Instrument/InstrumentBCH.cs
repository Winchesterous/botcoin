using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Exchange;
using System;
using System.Collections.Generic;

namespace BotCoin.Instruments
{
    public class InstrumentBCH : Instrument
    {
        public InstrumentBCH() : base()
        {
            BalanceStep  = 0.08;
            //AvgAmount    = TradingStrategy.AvgBchAmount;
        }

        public override double GetCryptoBalance(IExchange ex)
        {
            return ex.BchBalance;
        }

        public override CurrencyName GetInstrument()
        {
            return CurrencyName.BCH;
        }

        public override void SubscribeOrderBook(IExchange ex, Action<ExchangeName, OrderBookEventArgs> handler)
        {
            ((IExchangeEvents)ex).OnBchOrderBook += OnOrderBook;
            _orderBookHandler = handler;
        }

        public override void UnsubscribeOrderBook(IExchange ex)
        {
            ((IExchangeEvents)ex).OnBchOrderBook -= OnOrderBook;
        }

        private void OnOrderBook(object sender, OrderBookEventArgs e)
        {
            OnOrderBook(sender, e, CurrencyName.BCH);
        }

        private void OnTrade(object sender, TradeEventArgs e)
        {
            OnTrade(sender, e, CurrencyName.BCH);
        }

        public override void InitExchangePrice(BaseExchange ex, ExchangePricesEventArgs args)
        {
            args.BchPrice = new double[] { GetBidOrder(ex).Price, GetAskOrder(ex).Price };
            args.BchAmount = new double[2][] { GetBidOrder(ex).Amount, GetAskOrder(ex).Amount };
        }

        public override OrderInfo CreateBidOrder(IExchange ex, out double expectedAmount)
        {
            expectedAmount = GetBalanceStep();
            ex.BchBidOrder = new ExchangeBchOrder(expectedAmount);
            GetBidOrder(ex).Amount = new double[2];
            return new OrderInfo();
        }

        public override OrderInfo CreateAskOrder(IExchange ex, out double expectedAmount)
        {
            expectedAmount = GetBalanceStep();
            ex.BchAskOrder = new ExchangeBchOrder(expectedAmount);
            GetAskOrder(ex).Amount = new double[2];
            return new OrderInfo();
        }

        public override ExchangeOrder GetBidOrder(IExchange ex)
        {
            return ex.BchBidOrder;
        }

        protected override ExchangeOrder GetAskOrder(IExchange ex)
        {
            return ex.BchAskOrder;
        }

        public override OrderSide GetOrderType(ExchangePricesEventArgs args)
        {
            Utils.ThrowIf(args.BchPrice == null, "BchPrice null");
            Utils.ThrowIf(args.BchPrice[0] == 0 && args.BchPrice[1] == 0, "BchPrice zeros");

            if (args.BchPrice[0] != 0 && args.BchPrice[1] != 0)
                return OrderSide.Both;

            if (args.BchPrice[0] != 0)
                return OrderSide.BID;

            return OrderSide.ASK;
        }

        public override double GetPrice(ExchangePricesEventArgs args, int index)
        {
            return args.BchPrice[index];
        }

        public override double[] GetAmount(ExchangePricesEventArgs args, int index)
        {
            return args.BchAmount[index];
        }

        public override void CreateOrder(IExchange ex, OrderSide type, ExchangePricesEventArgs args)
        {
            if (type == OrderSide.BID)
                ex.BchBidOrder = new ExchangeBchOrder(args, this, type);
            else if (type == OrderSide.ASK)
                ex.BchAskOrder = new ExchangeBchOrder(args, this, type);
            else
                Utils.ThrowIf(true, "BOTH order type");
        }

        public override Dictionary<string, double> InitTempOrderBook(ExchangePricesEventArgs data)
        {
            return new Dictionary<string, double>
            {
                { "@BidPrice", data.BchPrice[0] },
                { "@AskPrice", data.BchPrice[1] },
                { "@OrderBidAmount", data.BchAmount[0][1] },
                { "@OrderAskAmount", data.BchAmount[1][1] }
            };
        }
    }
}
