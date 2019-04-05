using BotCoin.ApiClient;
using BotCoin.BitmexScalper.Domain;
using BotCoin.BitmexScalper.Models;
using BotCoin.BitmexScalper.Properties;
using BotCoin.DataType;
using BotCoin.DataType.Database;
using BotCoin.DataType.Exchange;
using BotCoin.DataType.WebApi;
using BotCoin.Exchange;
using BotCoin.Instruments;
using BotCoin.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotCoin.BitmexScalper.Controllers
{
    internal class MainWindowController : IDisposable
    {
        public readonly Dictionary<string, ICryptoIntrument> Instruments;
        readonly BitmexBotClient _btxBotClient;
        readonly RestApiClient2 _apiClient;
        readonly MainWindow _mainWnd;

        BitmexExchange _bitmex;
        BtxLiquidation _btxLiq;

        Func<PositionRequest> createPosRequest;
        Func<TradeRequest> createTradeRequest;
        Func<IndicatorRequest> createIndicatorRequest;
        Func<SettingRequest> createSettingRequest;
        Func<LogRequest> createLogRequest;

        public MainWindowController(MainWindow mainWnd)
        {
            var config = MainWindow.Config;

            var setting   = new ExchangeSettingsData { WebsocketUrl = config.Connections.GetElement("BitmexBot").Url };
            _btxBotClient = new BitmexBotClient(mainWnd, setting, config.Connections.GetElement("BitmexBot").TimoutMinute);
            Instruments  = new Dictionary<string, ICryptoIntrument>();

            Instruments[Settings.Default.LtcContract] = new LtcInstrument(Settings.Default.LtcContract);
            Instruments[Settings.Default.EosContract] = new EosInstrument(Settings.Default.EosContract);
            Instruments[Settings.Default.XrpContract] = new XrpInstrument(Settings.Default.XrpContract);
            Instruments[Settings.Default.AdaContract] = new AdaInstrument(Settings.Default.AdaContract);
            Instruments[Settings.Default.TrxContract] = new TrxInstrument(Settings.Default.TrxContract);
            Instruments[Settings.Default.BchContract] = new BchInstrument(Settings.Default.BchContract);
            Instruments[Settings.Default.XbtSwap]     = new XbtInstrument(Settings.Default.XbtSwap);
            Instruments[Settings.Default.EthSwap]     = new EthInstrument(Settings.Default.EthSwap);

            _apiClient = new RestApiClient2(config.Connections.GetElement("WebApi").Url);
            _mainWnd   = mainWnd;
        }

        public BitmexBotClient BotClient
        {
            get { return _btxBotClient; }
        }

        public BitmexClient Client
        {
            get { return ((BitmexRestExchange)_bitmex.Exchange).Client; }
        }

        public BitmexExchange Exchange
        {
            get { return _bitmex; }
        }

        public void LogEvent(EventModel model)
        {
            Task.Run(() =>
            {
                createLogRequest().WriteScalperLog(((RestServiceEventLogger)_bitmex.Log).SessionId, model.Time, model.EventType, model.Message);
            });
        }

        public void CreateBitmexTrade(DbPosition position, double stopValue, double riskPercent, double stopPrice, double startWatchPrice)
        {
            createTradeRequest().CreateBitmexTrade(position, stopValue, riskPercent, stopPrice, startWatchPrice);
        }

        public double GetBuyLiqPrice(double leverage, double price, double rate)
        {
            return _btxLiq.GetBuyLiqPrice(leverage, price, rate);
        }

        public double GetSellLiqPrice(double leverage, double price, double rate)
        {
            return _btxLiq.GetSellLiqPrice(leverage, price, rate);
        }

        public BitmexUser GetAccountName()
        {
            return _bitmex.Exchange.Client.GetAccount();
        }

        public void SetBitmexSettings(string account)
        {
            var settings = createSettingRequest().GetBitmexInstruments(account);
            if (settings == null || settings.Length == 0)
                throw new InvalidOperationException("Bitmex instruments is undefined");

            var instruments = new string[] 
            {
                Settings.Default.XbtSwap,
                Settings.Default.EthSwap,
                Settings.Default.LtcContract,
                Settings.Default.EosContract,
                Settings.Default.XrpContract,
                Settings.Default.AdaContract,
                Settings.Default.TrxContract,
                Settings.Default.BchContract
            };
            foreach (var symbol in instruments)
            {
                var setting = settings.Where(c => c.Symbol == symbol).SingleOrDefault();

                Instruments[symbol].SetCommission(setting);
                _bitmex.SetInstruments(setting);
            }
        }

        public void Logon()
        {
            var btxName = MainWindow.Config.BitmexScalper.AccountMode;
            var setting = SettingRequest.Get(_apiClient, btxName);

            _btxLiq = new BtxLiquidation(setting.TradeFee);
            _bitmex = new BitmexExchange(setting);
            _bitmex.Log       = new RestServiceEventLogger(_apiClient, DataType.ServiceName.Desktop);
            _btxBotClient.Log = _bitmex.Log;

            createIndicatorRequest = () => new IndicatorRequest(_apiClient);
            createTradeRequest     = () => new TradeRequest(_apiClient);
            createPosRequest       = () => new PositionRequest(_apiClient);
            createLogRequest       = () => new LogRequest(_apiClient);
            createSettingRequest   = () => new SettingRequest(_apiClient);

            try
            {
                _bitmex.Logon();
                _btxBotClient.Logon();
            }
            catch (Exception ex)
            {
                _bitmex.Log.WriteError(ex);
                throw ex;
            }
        }

        public void Logout()
        {
            if (_bitmex == null) return;
            try
            {
                _bitmex.Logout();
                _btxBotClient.Logout();
            }
            catch (Exception ex)
            {
                _bitmex.Log.WriteError(ex);
            }
        }

        public void SavePositionState(BitmexUser account, List<DbPositionState> states)
        {
            createPosRequest().SavePositionState(account, states);
        }

        public DbPositionState GetPositionState(BitmexUser account, string instrument)
        {
            return createPosRequest().GetPositionState(account, instrument);
        }

        public DbMessage GetDbTrades(BitmexUser user, string instrument, int count = 20)
        {
            return createTradeRequest().GetDbTrades(user, instrument, count);
        }

        public DbMessage GetDbTrades(BitmexUser user, DateTime startTime, DateTime endTime, string instrument)
        {
            return createTradeRequest().GetDbTrades(user, startTime, endTime, instrument);
        }

        public DbIndicatorVwapLite[] GetVwapGains(DateTime date, ExchangeName exchange)
        {
            return createIndicatorRequest().GetVwapGains(date, exchange.ToString());
        }

        public string CancelOrders(params string[] ids)
        {
            return _bitmex.Exchange.CancelOrders(ids);
        }

        public void CancelAllOrders(string symbol)
        {
            _bitmex.Exchange.Client.CancelAllOrders(symbol);
        }
                
        public void SetLeverage(double value, string symbol)
        {
            Client.SetLeverage(value, symbol);
        }

        private void ThrowIfError(BitmexOrderData result, string actionName)
        {
            if (result.HasError)
                throw new InvalidOperationException(String.Format("BitMEX '{0}' when {1}: {2}", result.Text, actionName, result.Error));
        }

        public BitmexOrderData CreateLimitOrder(string symbol, string side, long qty, double price, bool reduceOnly, string text)
        {
            var result = Client.CreateLimitOrder(symbol, side, qty, price, reduceOnly, text);
            ThrowIfError(result, "CREATE LIMIT");

            return result;
        }

        public BitmexOrderData CreateMarketOrder(string symbol, string side, long qty, string text)
        {
            var result = Client.CreateMarketOrder(symbol, side, qty, text);
            ThrowIfError(result, "CREATE MARKET");

            return result;
        }

        public BitmexOrderData CreateCloseMarketOrder(string symbol, string side, long? qty = null)
        {
            var result = Client.CreateCloseMarketOrder(symbol, side, qty);
            ThrowIfError(result, "CREATE CLOSEMARKET");

            return result;
        }

        public BitmexOrderData CreateStopLimitOrder(string symbol, string side, int qty, double price, double stopPrice, bool closeOnTrigger = true, string text = null)
        {
            var result = Client.CreateStopLimitOrder(symbol, side, qty, price, stopPrice, closeOnTrigger, text);
            ThrowIfError(result, "CREATE STOPLIMIT");

            return result;
        }

        public BitmexOrderData CreateStopMarketOrder(string symbol, string side, long qty, double stopPrice, string text = null)
        {
            var result = Client.CreateStopMarketOrder(symbol, side, qty, stopPrice, true, text);
            ThrowIfError(result, "CREATE STOPMARKET");

            return result;
        }

        public BitmexOrderData UpdateStopOrder(string orderId, long? qty = null, double? stopPx = null)
        {
            var result = Client.UpdateLimitOrder(orderId, qty, null, stopPx);
            ThrowIfError(result, "UPDATE STOP");

            return result;
        }

        public bool UpdateLimitOrder(string orderId, int? qty = null, double? price = null, double? stopPrice = null)
        {
            if (!qty.HasValue && !price.HasValue && !stopPrice.HasValue)
                return false;

            var result = Client.UpdateLimitOrder(orderId, qty, price, stopPrice);
            ThrowIfError(result, "UPDATE LIMIT");

            return true;
        }

        public void Dispose()
        {
            Logout();
            _apiClient.Dispose();
        }

        public void CreateTakeProfit(string symbol, string ordSide, params Tuple<double, long>[] profits)
        {
            ordSide = ordSide == "Buy" ? "Sell" : "Buy";
            foreach (var profit in profits)
            {
                if (profit.Item2 > 0)
                {
                    CreateLimitOrder(symbol, ordSide, profit.Item2, profit.Item1, true, null);
                    Thread.Sleep(200);
                }
            }
        }

        public void SubscribeTopics()
        {
            var ex = Exchange;
            ex.Subscription(true, BtxSubscriptionItem.position.ToString());
            ex.Subscription(true, BtxSubscriptionItem.funding.ToString());
            ex.Subscription(true, BtxSubscriptionItem.order.ToString());
            ex.Subscription(true, BtxSubscriptionItem.margin.ToString());
            ex.Subscription(true, String.Format("{0}:{1}", BtxSubscriptionItem.orderBookL2_25.ToString(), Settings.Default.XbtSwap));
            ex.Subscription(true, String.Format("{0}:{1}", BtxSubscriptionItem.orderBookL2_25.ToString(), Settings.Default.EthSwap));
            ex.Subscription(true, String.Format("{0}:{1}", BtxSubscriptionItem.orderBookL2_25.ToString(), Settings.Default.LtcContract));
            ex.Subscription(true, String.Format("{0}:{1}", BtxSubscriptionItem.orderBookL2_25.ToString(), Settings.Default.EosContract));
            ex.Subscription(true, String.Format("{0}:{1}", BtxSubscriptionItem.orderBookL2_25.ToString(), Settings.Default.XrpContract));
            ex.Subscription(true, String.Format("{0}:{1}", BtxSubscriptionItem.orderBookL2_25.ToString(), Settings.Default.AdaContract));
            ex.Subscription(true, String.Format("{0}:{1}", BtxSubscriptionItem.orderBookL2_25.ToString(), Settings.Default.TrxContract));
            ex.Subscription(true, String.Format("{0}:{1}", BtxSubscriptionItem.orderBookL2_25.ToString(), Settings.Default.BchContract));
        }

        public double SetBidPrice(ExchangePricesEventArgs args)
        {
            return Instruments[args.Symbol].SetBidPrice(args);
        }

        public double SetAskPrice(ExchangePricesEventArgs args)
        {
            return Instruments[args.Symbol].SetAskPrice(args);
        }

        public string ToStringPrice(double price, string symbol)
        {
            return Instruments[symbol].ToStringPrice(price);
        }

        public double GetBidPrice(string symbol)
        {
            return Instruments[symbol].GetBidPrice();
        }

        public double GetAskPrice(string symbol)
        {
            return Instruments[symbol].GetAskPrice();
        }

        public string GetInstrumentTickSize(string symbol)
        {
            var inst = Instruments[symbol];
            return inst.ToStringPrice(inst.TickSize);
        }

        public void SetProfit(double price, long size, double openPosFee, Action<double, double> setProfitUsd, PositionModel model)
        {
            var instrument    = Instruments[model.Symbol];
            var pnl           = instrument.GetPnl(size, model.AvgEntryPrice, price);
            var openValue     = instrument.ConvertToPrice(size, model.AvgEntryPrice);
            var feeOpen       = openValue * openPosFee;
            var feeCloseTaker = (openValue + pnl) * instrument.GetTakerFee();
            var feeCloseMaker = (openValue + pnl) * instrument.GetMakerFee();

            var profitMaker = Math.Round(pnl - feeOpen - feeCloseMaker, 5);
            var profitTaker = Math.Round(pnl - feeOpen - feeCloseTaker, 5);

            model.TradeProfitXbt = profitTaker.ToString("0.00000") + " XBT";
            model.ProfitXbt      = profitMaker.ToString("0.00000");

            setProfitUsd(profitMaker, profitTaker);
        }

        public int GetPosition(double price, double stopValue, double risk, double balance, string symbol)
        {
            var a1  = risk * balance;
            var a2  = Instruments[symbol].GetPnl(100, price, price - stopValue);
            var res = Math.Round(a1 / a2, 1);

            return (int)res;
        }

        public double GetRealRisk(long size, double price, double stopValue, double risk, double balance, string symbol)
        {
            var instrument = Instruments[symbol];
            var posProfit  = instrument.GetPnl(size, price, price - stopValue);
            var openPos    = instrument.ConvertToPrice(size, price);
            var closePos   = openPos + posProfit;
            var fee1       = -1 * openPos * instrument.GetMakerFee();
            var fee2       = -1 * closePos * instrument.GetTakerFee();
            var profit     = posProfit + fee1 + fee2;

            return -1 * Math.Round(profit / balance * 100, 3);
        }

        public double GetZeroProfitPriceStep(long size, double entryPrice, string symbol)
        {
            double price = Instruments[symbol].ConvertToPrice(size, entryPrice);
            double fee = price * Instruments[symbol].GetTakerFee() * 2; // 2 <=> fee1 + fee2

            return Math.Round(Math.Abs(fee * entryPrice) + 0.5);
        }

        public string GetOrderValue(long qty, double price, string symbol)
        {
            return Instruments[symbol].FormatOrderValue(qty, price);
        }
    }
}