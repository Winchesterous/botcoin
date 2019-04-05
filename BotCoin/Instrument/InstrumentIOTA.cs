using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Exchange;
using System;
using System.Collections.Generic;

namespace BotCoin.Instruments
{
    public class InstrumentIOTA : Instrument
    {
        public InstrumentIOTA() : base()
        {
            BalanceStep = 45.11;
        }

        public override double GetCryptoBalance(IExchange ex)
        {
            return ex.IotaBalance;
        }

        public override CurrencyName GetInstrument()
        {
            return CurrencyName.IOTA;
        }

        public override void SubscribeOrderBook(IExchange exchange, Action<ExchangeName, OrderBookEventArgs> handler)
        {
            var ex = exchange as IExchangeAltcoinEvents;
            if (ex != null)
            {
                ((IExchangeAltcoinEvents)ex).OnIotaOrderBook += OnOrderBook;
                _orderBookHandler = handler;
            }
        }

        public override void SubscribeTrade(IExchange ex, Action<ExchangeName, TradeEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        public override void UnsubscribeOrderBook(IExchange ex)
        {
            ((IExchangeAltcoinEvents)ex).OnIotaOrderBook -= OnOrderBook;
        }

        public override void UnsubscribeTrade(IExchange ex)
        {
            throw new NotImplementedException();
        }

        private void OnOrderBook(object sender, OrderBookEventArgs e)
        {
            OnOrderBook(sender, e, CurrencyName.IOTA);
        }

        private void OnTrade(object sender, TradeEventArgs e)
        {
            OnTrade(sender, e, CurrencyName.IOTA);
        }

        public override void InitExchangePrice(BaseExchange ex, ExchangePricesEventArgs args)
        {
            args.IotaPrice = new double[] { GetBidOrder(ex).Price, GetAskOrder(ex).Price };
            args.IotaAmount = new double[2][] { GetBidOrder(ex).Amount, GetAskOrder(ex).Amount };
        }

        public override OrderInfo CreateBidOrder(IExchange ex, out double expectedAmount)
        {
            expectedAmount = GetBalanceStep();
            ex.IotaBidOrder = new ExchangeIotaOrder(expectedAmount);
            GetBidOrder(ex).Amount = new double[2];
            return new OrderInfo();
        }

        public override OrderInfo CreateAskOrder(IExchange ex, out double expectedAmount)
        {
            expectedAmount = GetBalanceStep();
            ex.IotaAskOrder = new ExchangeIotaOrder(expectedAmount);
            GetAskOrder(ex).Amount = new double[2];
            return new OrderInfo();
        }

        public override ExchangeOrder GetBidOrder(IExchange ex)
        {
            return ex.IotaBidOrder;
        }

        protected override ExchangeOrder GetAskOrder(IExchange ex)
        {
            return ex.IotaAskOrder;
        }

        public override OrderSide GetOrderType(ExchangePricesEventArgs args)
        {
            Utils.ThrowIf(args.IotaPrice == null, "IotaPrice null");
            Utils.ThrowIf(args.IotaPrice[0] == 0 && args.IotaPrice[1] == 0, "IotaPrice zeros");

            if (args.IotaPrice[0] != 0 && args.IotaPrice[1] != 0)
                return OrderSide.Both;

            if (args.IotaPrice[0] != 0)
                return OrderSide.BID;

            return OrderSide.ASK;
        }

        public override double GetPrice(ExchangePricesEventArgs args, int index)
        {
            return args.IotaPrice[index];
        }

        public override double[] GetAmount(ExchangePricesEventArgs args, int index)
        {
            return args.IotaAmount[index];
        }

        public override void CreateOrder(IExchange ex, OrderSide type, ExchangePricesEventArgs args)
        {
            if (type == OrderSide.BID)
                ex.IotaBidOrder = new ExchangeIotaOrder(args, this, type);
            else if (type == OrderSide.ASK)
                ex.IotaAskOrder = new ExchangeIotaOrder(args, this, type);
            else
                Utils.ThrowIf(true, "BOTH order type");
        }

        public override Dictionary<string, double> InitTempOrderBook(ExchangePricesEventArgs data)
        {
            return new Dictionary<string, double>
            {
                { "@BidPrice", data.IotaPrice[0] },
                { "@AskPrice", data.IotaPrice[1] },
                { "@OrderBidAmount", data.IotaAmount[0][1] },
                { "@OrderAskAmount", data.IotaAmount[1][1] }
            };
        }
    }
}
