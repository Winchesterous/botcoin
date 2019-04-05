using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Exchange;
using System;
using System.Collections.Generic;

namespace BotCoin.Instruments
{
    public class InstrumentXRP : Instrument
    {
        public InstrumentXRP() : base()
        {
            BalanceStep  = 12;
            //AvgAmount    = TradingStrategy.AvgXrpAmount;
        }

        public override double GetCryptoBalance(IExchange ex)
        {
            return ex.XrpBalance;
        }

        public override CurrencyName GetInstrument()
        {
            return CurrencyName.XRP;
        }

        public override void SubscribeOrderBook(IExchange ex, Action<ExchangeName, OrderBookEventArgs> handler)
        {
            ((IExchangeEvents)ex).OnXrpOrderBook += OnOrderBook;
            _orderBookHandler = handler;
        }

        public override void SubscribeTrade(IExchange ex, Action<ExchangeName, TradeEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        public override void UnsubscribeOrderBook(IExchange ex)
        {
            ((IExchangeEvents)ex).OnXrpOrderBook -= OnOrderBook;
        }

        public override void UnsubscribeTrade(IExchange ex)
        {
            throw new NotImplementedException();
        }

        private void OnOrderBook(object sender, OrderBookEventArgs e)
        {
            OnOrderBook(sender, e, CurrencyName.XRP);
        }

        private void OnTrade(object sender, TradeEventArgs e)
        {
            OnTrade(sender, e, CurrencyName.XRP);
        }

        public override void InitExchangePrice(BaseExchange ex, ExchangePricesEventArgs args)
        {
            args.XrpPrice = new double[] { GetBidOrder(ex).Price, GetAskOrder(ex).Price };
            args.XrpAmount = new double[2][] { GetBidOrder(ex).Amount, GetAskOrder(ex).Amount };
        }

        public override OrderInfo CreateBidOrder(IExchange ex, out double expectedAmount)
        {
            expectedAmount = GetBalanceStep();
            ex.XrpBidOrder = new ExchangeXrpOrder(expectedAmount);
            GetBidOrder(ex).Amount = new double[2];
            return new OrderInfo();
        }

        public override OrderInfo CreateAskOrder(IExchange ex, out double expectedAmount)
        {
            expectedAmount = GetBalanceStep();
            ex.XrpAskOrder = new ExchangeXrpOrder(expectedAmount);
            GetAskOrder(ex).Amount = new double[2];
            return new OrderInfo();
        }

        public override ExchangeOrder GetBidOrder(IExchange ex)
        {
            return ex.XrpBidOrder;
        }

        protected override ExchangeOrder GetAskOrder(IExchange ex)
        {
            return ex.XrpAskOrder;
        }

        public override OrderSide GetOrderType(ExchangePricesEventArgs args)
        {
            Utils.ThrowIf(args.XrpPrice == null, "XrpPrice null");
            Utils.ThrowIf(args.XrpPrice[0] == 0 && args.XrpPrice[1] == 0, "XrpPrice zeros");

            if (args.XrpPrice[0] != 0 && args.XrpPrice[1] != 0)
                return OrderSide.Both;

            if (args.XrpPrice[0] != 0)
                return OrderSide.BID;

            return OrderSide.ASK;
        }

        public override double GetPrice(ExchangePricesEventArgs args, int index)
        {
            return args.XrpPrice[index];
        }

        public override double[] GetAmount(ExchangePricesEventArgs args, int index)
        {
            return args.XrpAmount[index];
        }

        public override void CreateOrder(IExchange ex, OrderSide type, ExchangePricesEventArgs args)
        {
            if (type == OrderSide.BID)
                ex.XrpBidOrder = new ExchangeXrpOrder(args, this, type);
            else if (type == OrderSide.ASK)
                ex.XrpAskOrder = new ExchangeXrpOrder(args, this, type);
            else
                Utils.ThrowIf(true, "BOTH order type");
        }

        public override Dictionary<string, double> InitTempOrderBook(ExchangePricesEventArgs data)
        {
            return new Dictionary<string, double>
            {
                { "@BidPrice", data.XrpPrice[0] },
                { "@AskPrice", data.XrpPrice[1] },
                { "@OrderBidAmount", data.XrpAmount[0][1] },
                { "@OrderAskAmount", data.XrpAmount[1][1] }
            };
        }
    }
}
