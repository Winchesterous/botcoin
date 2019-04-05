using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Exchange;
using System;
using System.Collections.Generic;

namespace BotCoin.Instruments
{
    public class InstrumentBTC : Instrument
    {
        public InstrumentBTC() : base()
        {
            BalanceStep  = 0.01;
            //AvgAmount    = TradingStrategy.AvgBtcAmount;
        }

        public override double GetCryptoBalance(IExchange ex)
        {
            return ex.BtcBalance;
        }

        public override CurrencyName GetInstrument()
        {
            return CurrencyName.BTC;
        }

        public override void SubscribeOrderBook(IExchange ex, Action<ExchangeName, OrderBookEventArgs> handler)
        {
            ((IExchangeEvents)ex).OnBtcOrderBook += OnOrderBook;
            _orderBookHandler = handler;
        }

        public override void UnsubscribeOrderBook(IExchange ex)
        {
            ((IExchangeEvents)ex).OnBtcOrderBook -= OnOrderBook;
        }

        private void OnOrderBook(object sender, OrderBookEventArgs e)
        {
            OnOrderBook(sender, e, CurrencyName.BTC);
        }

        private void OnTrade(object sender, TradeEventArgs e)
        {
            OnTrade(sender, e, CurrencyName.BTC);
        }

        public override void InitExchangePrice(BaseExchange ex, ExchangePricesEventArgs args)
        {
            args.BtcPrice = new double[] { GetBidOrder(ex).Price, GetAskOrder(ex).Price };
            args.BtcAmount = new double[2][] { GetBidOrder(ex).Amount, GetAskOrder(ex).Amount };
        }

        public override OrderInfo CreateBidOrder(IExchange ex, out double expectedAmount)
        {
            expectedAmount = GetBalanceStep();
            ex.BtcBidOrder = new ExchangeBtcOrder(expectedAmount);
            GetBidOrder(ex).Amount = new double[2];
            return new OrderInfo();
        }

        public override OrderInfo CreateAskOrder(IExchange ex, out double expectedAmount)
        {
            expectedAmount = GetBalanceStep();
            ex.BtcAskOrder = new ExchangeBtcOrder(expectedAmount);
            GetAskOrder(ex).Amount = new double[2];
            return new OrderInfo();
        }

        public override ExchangeOrder GetBidOrder(IExchange ex)
        {
            return ex.BtcBidOrder;
        }

        protected override ExchangeOrder GetAskOrder(IExchange ex)
        {
            return ex.BtcAskOrder;
        }

        public override void CreateOrder(IExchange ex, OrderSide type, ExchangePricesEventArgs args)
        {
            if (type == OrderSide.BID)
                ex.BtcBidOrder = new ExchangeBtcOrder(args, this, type);
            else if (type == OrderSide.ASK)
                ex.BtcAskOrder = new ExchangeBtcOrder(args, this, type);
            else
                Utils.ThrowIf(true, "BOTH order type");
        }

        public override OrderSide GetOrderType(ExchangePricesEventArgs args)
        {
            Utils.ThrowIf(args.BtcPrice == null, "BtcPrice null");
            Utils.ThrowIf(args.BtcPrice[0] == 0 && args.BtcPrice[1] == 0, "BtcPrice zeros");

            if (args.BtcPrice[0] != 0 && args.BtcPrice[1] != 0)
                return OrderSide.Both;

            if (args.BtcPrice[0] != 0)
                return OrderSide.BID;

            return OrderSide.ASK;
        }

        public override double GetPrice(ExchangePricesEventArgs args, int index)
        {
            return args.BtcPrice[index];
        }

        public override double[] GetAmount(ExchangePricesEventArgs args, int index)
        {
            return args.BtcAmount[index];
        }

        public override Dictionary<string, double> InitTempOrderBook(ExchangePricesEventArgs data)
        {
            return new Dictionary<string, double>
            {
                { "@BidPrice", data.BtcPrice[0] },
                { "@AskPrice", data.BtcPrice[1] },
                { "@OrderBidAmount", data.BtcAmount[0][1] },
                { "@OrderAskAmount", data.BtcAmount[1][1] }
            };
        }
    }
}
