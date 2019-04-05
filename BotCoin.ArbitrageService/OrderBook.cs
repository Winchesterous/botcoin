using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Exchange;
using BotCoin.Instruments;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BotCoin.ArbitrageService
{
    internal class OrderBook
    {
        Dictionary<ExchangeName, string[]> _removedOrders = new Dictionary<ExchangeName, string[]>();

        public void InitOrder(ExchangeName exName)
        {
            _removedOrders[exName] = new string[] { String.Empty, String.Empty };
        }

        private bool WebsocketExchanges(ExchangeName ex)
        {
            return false;// ex == ExchangeName.Bitstamp || ex == ExchangeName.Luno;
        }

        private bool RestExchanges(ExchangeName ex)
        {
            return ex == ExchangeName.Kuna || ex == ExchangeName.Quadriga || ex == ExchangeName.Bitbay || ex == ExchangeName.Kraken;
        }

        private bool SignAsRemoved(ExchangeName ex, OrderSide type, ExchangePricesEventArgs args)
        {
            if (WebsocketExchanges(ex))
            {
                Utils.ThrowIf(String.IsNullOrEmpty(args.OrderId), "[{0}] orderId is undefined", ex.ToString());
                Utils.ThrowIf(type == OrderSide.Both, "Order type");

                if (args.IsOrderDeleted)
                {
                    var orders = _removedOrders[ex];
                    lock (orders)
                    {
                        if (String.Compare(orders[type == OrderSide.BID ? 0 : 1], args.OrderId, true) == 0)
                        {
                            SaveOrder(ex, type);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void SaveOrder(ExchangeName ex, OrderSide type, string orderId = "")
        {
            if (WebsocketExchanges(ex))
                _removedOrders[ex][type == OrderSide.BID ? 0 : 1] = orderId;
        }

        private void Lock(IExchange exchange, Action action)
        {
            if (RestExchanges(exchange.GetExchangeName()))
            {
                Monitor.Enter(exchange);
            }
            else
            {
                if (!Monitor.TryEnter(exchange, 10))
                    return;
            }
            action();
            Monitor.Exit(exchange);
        }

        public void HandlingOrder(Action<IExchange> createOrder, 
                                  IExchange exchange, 
                                  AutoResetEvent evt, 
                                  Instrument ins, 
                                  ExchangePricesEventArgs args)
        {
            var type = ins.GetOrderType(args);
            var ex = exchange.GetExchangeName();

            if (SignAsRemoved(ex, type, args))
            {
                Lock(exchange, () =>
                {
                    SaveOrder(ex, type);
                    if (!args.IsOrderDeleted)
                    {
                        SaveOrder(ex, type, args.OrderId);
                        createOrder(exchange);

                        if (ins.ContainsOrders(exchange))
                            evt.Set();
                    }
                });
            }
        }

        public bool TryExecuteOrders(IExchange exchange1, IExchange exchange2)
        {
            ExchangeName ex1 = exchange1.GetExchangeName();
            if (WebsocketExchanges(ex1))
            {
                if (_removedOrders[ex1][0].Length == 0)
                    return false;
            }
            ExchangeName ex2 = exchange2.GetExchangeName();
            if (WebsocketExchanges(ex2))
            {
                if (_removedOrders[ex2][1].Length == 0)
                    return false;
            }
            return true;
        }
    }
}
