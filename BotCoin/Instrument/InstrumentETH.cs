using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Exchange;
using System;
using System.Collections.Generic;

namespace BotCoin.Instruments
{
    public class InstrumentETH : Instrument
    {
        public InstrumentETH() : base()
        {
            BalanceStep  = 0.1;
            //AvgAmount    = TradingStrategy.AvgEthAmount;
        }

        public override double GetCryptoBalance(IExchange ex)
        {
            return ex.EthBalance;
        }

        public override CurrencyName GetInstrument()
        {
            return CurrencyName.ETH;
        }

        public override void SubscribeOrderBook(IExchange ex, Action<ExchangeName, OrderBookEventArgs> handler)
        {
            ((IExchangeEvents)ex).OnEthOrderBook += OnOrderBook;
            _orderBookHandler = handler;
        }

        public override void UnsubscribeOrderBook(IExchange ex)
        {
            ((IExchangeEvents)ex).OnEthOrderBook -= OnOrderBook;
        }

        private void OnOrderBook(object sender, OrderBookEventArgs e)
        {
            OnOrderBook(sender, e, CurrencyName.ETH);
        }

        private void OnTrade(object sender, TradeEventArgs e)
        {
            OnTrade(sender, e, CurrencyName.ETH);
        }

        public override void InitExchangePrice(BaseExchange ex, ExchangePricesEventArgs args)
        {
            args.EthPrice = new double[] { GetBidOrder(ex).Price, GetAskOrder(ex).Price };
            args.EthAmount = new double[2][] { GetBidOrder(ex).Amount, GetAskOrder(ex).Amount };
        }

        public override OrderInfo CreateBidOrder(IExchange ex, out double expectedAmount)
        {
            expectedAmount = GetBalanceStep();
            ex.EthBidOrder = new ExchangeEthOrder(expectedAmount);
            GetBidOrder(ex).Amount = new double[2];
            return new OrderInfo();
        }

        public override OrderInfo CreateAskOrder(IExchange ex, out double expectedAmount)
        {
            expectedAmount = GetBalanceStep();
            ex.EthAskOrder = new ExchangeEthOrder(expectedAmount);
            GetAskOrder(ex).Amount = new double[2];
            return new OrderInfo();
        }

        public override ExchangeOrder GetBidOrder(IExchange ex)
        {
            return ex.EthBidOrder;
        }

        protected override ExchangeOrder GetAskOrder(IExchange ex)
        {
            return ex.EthAskOrder;
        }

        public override OrderSide GetOrderType(ExchangePricesEventArgs args)
        {
            Utils.ThrowIf(args.EthPrice == null, "EthPrice null");
            Utils.ThrowIf(args.EthPrice[0] == 0 && args.EthPrice[1] == 0, "EthPrice zeros");

            if (args.EthPrice[0] != 0 && args.EthPrice[1] != 0)
                return OrderSide.Both;

            if (args.EthPrice[0] != 0)
                return OrderSide.BID;

            return OrderSide.ASK;
        }

        public override double GetPrice(ExchangePricesEventArgs args, int index)
        {
            return args.EthPrice[index];
        }

        public override double[] GetAmount(ExchangePricesEventArgs args, int index)
        {
            return args.EthAmount[index];
        }

        public override void CreateOrder(IExchange ex, OrderSide type, ExchangePricesEventArgs args)
        {
            if (type == OrderSide.BID)
                ex.EthBidOrder = new ExchangeEthOrder(args, this, type);
            else if (type == OrderSide.ASK)
                ex.EthAskOrder = new ExchangeEthOrder(args, this, type);
            else
                Utils.ThrowIf(true, "BOTH order type");
        }

        public override Dictionary<string, double> InitTempOrderBook(ExchangePricesEventArgs data)
        {
            return new Dictionary<string, double>
            {
                { "@BidPrice", data.EthPrice[0] },
                { "@AskPrice", data.EthPrice[1] },
                { "@OrderBidAmount", data.EthAmount[0][1] },
                { "@OrderAskAmount", data.EthAmount[1][1] }
            };
        }
    }
}
