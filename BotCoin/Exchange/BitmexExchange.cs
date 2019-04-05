using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotCoin.Exchange
{
    public enum BtxSubscriptionItem
    {
        wallet, margin, funding, trade, instrument, orderBookL2_25, execution, order, position, liquidation
    }

    [Exchange(Name = "Bitmex", IsWebsocket = true)]
    public class BitmexExchange : BitmexWebSocketClient
    {
        BitmexOrderBookL2 _bookL2;
        readonly object _obj;

        public event EventHandler AuthPassed;
        public event EventHandler<BitmexEventArgs> InstrumentChanged;
        public event EventHandler<BitmexEventArgs> ExecutionChanged;
        public event EventHandler<BitmexEventArgs> TradeChanged;
        public event EventHandler<LiquidationEventArgs> LiquidationReceived;
        public event EventHandler<ExchangePricesEventArgs> XbtOrderBookChanged;
        public event EventHandler<ExchangePricesEventArgs> EthOrderBookChanged;
        public event EventHandler<ExchangePricesEventArgs> LtcOrderBookChanged;
        public event EventHandler<ExchangePricesEventArgs> EosOrderBookChanged;
        public event EventHandler<ExchangePricesEventArgs> XrpOrderBookChanged;
        public event EventHandler<ExchangePricesEventArgs> AdaOrderBookChanged;
        public event EventHandler<ExchangePricesEventArgs> TrxOrderBookChanged;
        public event EventHandler<ExchangePricesEventArgs> BchOrderBookChanged;
        public event EventHandler<ExchangePricesEventArgs> PositionChanged;
        public event EventHandler<ExchangePricesEventArgs> XbtPriceChanged;
        public event EventHandler<ExchangePricesEventArgs> EthPriceChanged;
        public event EventHandler<ExchangePricesEventArgs> LtcPriceChanged;
        public event EventHandler<ExchangePricesEventArgs> EosPriceChanged;
        public event EventHandler<ExchangePricesEventArgs> XrpPriceChanged;
        public event EventHandler<ExchangePricesEventArgs> AdaPriceChanged;
        public event EventHandler<ExchangePricesEventArgs> TrxPriceChanged;
        public event EventHandler<ExchangePricesEventArgs> BchPriceChanged;
        public event EventHandler<ExchangePricesEventArgs> OrderChanged;
        public event EventHandler<ExchangePricesEventArgs> MarginChanged;
        public event EventHandler<ExchangePricesEventArgs> WalletChanged;
        public event EventHandler<ExchangePricesEventArgs> FundingChanged;
        
        public BitmexExchange(ExchangeSettingsData setting) : base(setting)
        {
            _restExchange = new BitmexRestExchange(setting);
            _bookL2       = new BitmexOrderBookL2();
            _obj          = new object();

            XbtOrderBookChanged += (s, e) => { };
            EthOrderBookChanged += (s, e) => { };
            LiquidationReceived += (s, e) => { };
            XbtPriceChanged     += (s, e) => { };
            EthPriceChanged     += (s, e) => { };
            LtcPriceChanged     += (s, e) => { };
            EosPriceChanged     += (s, e) => { };
            XrpPriceChanged     += (s, e) => { };
            AdaPriceChanged     += (s, e) => { };
            TrxPriceChanged     += (s, e) => { };
            BchPriceChanged     += (s, e) => { };
            FundingChanged      += (s, e) => { };
        }

        public BitmexRestExchange Exchange
        {
            get { return (BitmexRestExchange)_restExchange; }
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.Bitmex;
        }

        public override UserAccount GetBalances()
        {
            return _restExchange.GetBalances();
        }

        public void SetInstruments(BitmexInstrumentSettings setting)
        {
            _bookL2.Initialize(setting);
        }

        private void ProcessOrderBookRequest(BitmexOrderL2Book obj)
        {
            double bid = 0, ask = 0;
            string symbol = null;

            lock (_obj) symbol = _bookL2.ProcessBook(obj, ref bid, ref ask);
            if (symbol == null) return;

            var args = new ExchangePricesEventArgs(bid, ask, symbol);
            args.CreatedAt = DateTime.UtcNow;

            Task.Run(() =>
            {
                if (args.BtcPrice != null)
                {
                    if (XbtOrderBookChanged != null)
                        XbtOrderBookChanged(this, args);
                }
                else if (args.EthPrice != null)
                {
                    if (EthOrderBookChanged != null)
                        EthOrderBookChanged(this, args);
                }
                else if (args.LtcPrice != null)
                {
                    if (LtcOrderBookChanged != null)
                        LtcOrderBookChanged(this, args);
                }
                else if (args.EosPrice != null)
                {
                    if (EosOrderBookChanged != null)
                        EosOrderBookChanged(this, args);
                }
                else if (args.XrpPrice != null)
                {
                    if (XrpOrderBookChanged != null)
                        XrpOrderBookChanged(this, args);
                }
                else if (args.TrxPrice != null)
                {
                    if (TrxOrderBookChanged != null)
                        TrxOrderBookChanged(this, args);
                }
                else if (args.AdaPrice != null)
                {
                    if (AdaOrderBookChanged != null)
                        AdaOrderBookChanged(this, args);
                }
                else if (args.BchPrice != null)
                {
                    if (BchOrderBookChanged != null)
                        BchOrderBookChanged(this, args);
                }
            });

            if (args.BtcPrice != null)      XbtPriceChanged(this, args);
            else if (args.EthPrice != null) EthPriceChanged(this, args);
            else if (args.LtcPrice != null) LtcPriceChanged(this, args);
            else if (args.EosPrice != null) EosPriceChanged(this, args);
            else if (args.XrpPrice != null) XrpPriceChanged(this, args);
            else if (args.AdaPrice != null) AdaPriceChanged(this, args);
            else if (args.TrxPrice != null) TrxPriceChanged(this, args);
            else if (args.BchPrice != null) BchPriceChanged(this, args);
        }

        private void ProcessAuthorizationRequest(string msg, Func<BitmexWsResponse, bool> error)
        {
            var obj = JsonConvert.DeserializeObject<BitmexWsResponse>(msg);
            if (!error(obj)) return;

            if (String.Compare(obj.Request.Command, "authKeyExpires", true) == 0)
                AuthPassed(obj.Success ? this : null, null);
        }

        private void ProcessFundingRequest(string msg, Func<BitmexWsResponse, bool> error)
        {
            var obj = JsonConvert.DeserializeObject<BitmexFunding>(msg);
            if (!error(obj)) return;

            if (obj.Data.Length > 0)
                FundingChanged(this, new ExchangePricesEventArgs(obj.Data));
        }

        private void ProcessMarginRequest(string msg, Func<BitmexWsResponse, bool> error)
        {
            var obj = JsonConvert.DeserializeObject<BitmexMargin>(msg);
            if (!error(obj)) return;

            if (obj.Data.Length == 1)
                MarginChanged(this, new ExchangePricesEventArgs(obj.Data[0]));
        }

        private void ProcessWalletRequest(string msg, Func<BitmexWsResponse, bool> error)
        {
            var obj = JsonConvert.DeserializeObject<BitmexWallet>(msg);
            if (!error(obj)) return;
            
            if (obj.Data.Length == 1)
                WalletChanged(this, new ExchangePricesEventArgs(obj.Data[0]));
        }

        private void ProcessLiquidationRequest(string msg, Func<BitmexWsResponse, bool> error)
        {
            var obj = JsonConvert.DeserializeObject<BitmexLiquidation>(msg);
            if (!error(obj)) return;
            
            if (obj.Data.Length > 0)
                LiquidationReceived(this, new LiquidationEventArgs(obj.Data));
        }

        private void ProcessOrderRequest(string msg, Func<BitmexWsResponse, bool> error)
        {
            var obj = JsonConvert.DeserializeObject<BitmexOrder>(msg);
            if (!error(obj)) return;
            
            if (obj.Data.Length > 0)
                OrderChanged(this, new ExchangePricesEventArgs(obj.Data));
        }

        private void ProcessInstrumentRequest(string msg, Func<BitmexWsResponse, bool> error)
        {
            var obj = JsonConvert.DeserializeObject<BitmexInstrument>(msg);
            if (!error(obj)) return;
            
            if (obj.Data.Length == 1)
                InstrumentChanged(this, new BitmexEventArgs(obj.Data[0]));
        }

        private void ProcessExecutionRequest(string msg, Func<BitmexWsResponse, bool> error)
        {
            var obj = JsonConvert.DeserializeObject<BitmexExecution>(msg);
            if (!error(obj)) return;
            
            if (obj.Data.Length == 1)
                ExecutionChanged(this, new BitmexEventArgs(obj.Data[0]));
        }

        private void ProcessTradeRequest(string msg, Func<BitmexWsResponse, bool> error)
        {
            var obj = JsonConvert.DeserializeObject<BitmexTrade>(msg);
            if (!error(obj)) return;
            
            if (obj.Data.Length == 1)
                TradeChanged(this, new BitmexEventArgs(obj.Data));
        }

        private void ProcessPositionRequest(string msg, Func<BitmexWsResponse, bool> error)
        {
            var obj = JsonConvert.DeserializeObject<BitmexPosition>(msg);
            if (!error(obj)) return;
            
            if (obj.Data.Length > 0)
                PositionChanged(this, new ExchangePricesEventArgs(obj.Data));
        }

        protected override void OnMessage(string msg)
        {
            Func<BitmexWsResponse, bool> error = obj =>
            {
                if (!obj.HasError) return true;
                Log.WriteError(obj.Error.Message, (int)ExchangeName.Bitmex);
                return false;
            };
            try
            {
                var obj = JsonConvert.DeserializeObject<BitmexOrderL2Book>(msg);
                if (obj == null)
                {
                    Log.WriteError(msg, (int)ExchangeName.Bitmex);
                    return;
                }
                if (obj.Table != null)
                {
                    if (!error(obj)) return;

                    if (obj.Table.StartsWith("orderBook"))
                    {
                        ProcessOrderBookRequest(obj);
                    }
                    else if (obj.Table.StartsWith("instrument"))
                    {
                        ProcessInstrumentRequest(msg, e => error(e));
                    }
                    else if (obj.Table.StartsWith("execution"))
                    {
                        ProcessExecutionRequest(msg, e => error(e));
                    }
                    else if (obj.Table.StartsWith("trade"))
                    {
                        ProcessTradeRequest(msg, e => error(e));
                    }
                    else if (obj.Table.StartsWith("order"))
                    {
                        ProcessOrderRequest(msg, e => error(e));
                    }
                    else if (obj.Table.StartsWith("position"))
                    {
                        ProcessPositionRequest(msg, e => error(e));
                    }
                    else if (obj.Table.StartsWith("funding"))
                    {
                        ProcessFundingRequest(msg, e => error(e));
                    }
                    else if (obj.Table.StartsWith("margin"))
                    {
                        ProcessMarginRequest(msg, e => error(e));
                    }
                    else if (obj.Table.StartsWith("wallet"))
                    {
                        ProcessWalletRequest(msg, e => error(e));
                    }
                    else if (obj.Table.StartsWith("liquidation"))
                    {
                        ProcessLiquidationRequest(msg, e => error(e));
                    }
                }
                else if (msg.Contains("\"op\":\"authKeyExpires\""))
                {
                    ProcessAuthorizationRequest(msg, e => error(e));
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(TaskCanceledException))
                    lock (_obj) Log.WriteError(ex.Message + ex.StackTrace, (int)ExchangeName.Bitmex);
            }
        }

        public void SubscriptionAuth(string args, string operation = "subscribe")
        {
            var json = ((BitmexRestExchange)_restExchange).Client.CreateAuthWebsocketRequest();
            SendMessage(json);
        }

        public void Subscription(bool subscribe, params string[] args)
        {
            var operation = subscribe ? "subscribe" : "unsubscribe";
            var json = JsonConvert.SerializeObject(new BitmexWsCommand
            {
                Command = operation,
                Agruments = args
            });
            SendMessage(json);
        }

        public void SubscribeTopics(bool subscribe, string symbol, params BtxSubscriptionItem[] items)
        {
            var topics = new List<string>();
            foreach (var item in items)
            {
                if (item == BtxSubscriptionItem.margin || item == BtxSubscriptionItem.wallet)
                    topics.Add(item.ToString());
                else
                    topics.Add(String.Format("{0}:{1}", item.ToString(), symbol));
            }
            Subscription(subscribe, topics.ToArray());
        }
    }
}