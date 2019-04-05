using BotCoin.DataType;
using BotCoin.DataType.Database;
using BotCoin.DataType.Exchange;
using BotCoin.DataType.WebApi;
using BotCoin.Exchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotCoin.Service
{
    public class DbRepositoryService : IDbRepository
    {
        readonly DbRepository _dbRepo;

        public string SessionId
        {
            set { _dbRepo.SetSession(value); }
        }

        public DbRepositoryService()
        {
            _dbRepo = new DbRepository();
        }

        public Task SaveBittrexArbitrageAsync(BittrexArbitrageData data)
        {
            return Task.Run(() => _dbRepo.SaveBittrexArbitrageAsync(data));
        }

        public Task SaveMatchingAsync(MatchingData data, IExchange ex1 = null, IExchange ex2 = null)
        {
            return Task.Run(() => _dbRepo.SaveMatching(data, ex1, ex2));
        }

        public UserAccount GetLastBalances(ExchangeName exchange)
        {
            return _dbRepo.GetLastBalances(exchange);
        }

        public bool CanUpdateBalances(IExchange ex)
        {
            return _dbRepo.CanUpdateBalances(ex.GetExchangeName());
        }

        public void UpdateBalances(List<UserAccount> accounts)
        {
            _dbRepo.UpdateBalances(accounts);
        }

        public void WriteServiceEvent(ServiceEventData data)
        {
            _dbRepo.WriteServiceEvent(null, data);
        }

        public void WriteServiceEvent(string sessionId, ServiceEventType type, ServiceName service, string msg, int? exchangeId = null)
        {
            WriteServiceEvent(new ServiceEventData
            {
                ServiceName = service,
                SessionId = sessionId,
                EventType = type,
                Message = msg,
                ExchangeId = exchangeId
            });
        }

        public DbExchange GetExchangeInfo(ExchangeName ex)
        {
            return _dbRepo.GetExchangeInfo(ex);
        }

        public double GetCurrencyRate(CurrencyName currency)
        {
            return _dbRepo.GetCurrencyRate(currency);
        }

        public void SaveOrderBook(ExchangePricesEventArgs args, OrderBook orders)
        {
            _dbRepo.SaveOrderBook(args, orders);
        }

        public void SaveTrade(TradeEventArgs args)
        {
            if (args.Exchange == ExchangeName.Bitmex)
            {
                _dbRepo.SaveBitmexTrade(args);
            }
            else
                _dbRepo.SaveTrade(args);
        }

        public void SaveTicker(TickerEventArgs args)
        {
            _dbRepo.SaveTicker(args);
        }

        public void SaveVwapIndicator(string exchange, List<Tuple<DateTime, DateTime>> dates, string periodName, List<string[]> instruments)
        {
            _dbRepo.SaveVwapIndicator(exchange, instruments, dates, periodName);
        }

        public void SaveVwapRatio(string exchange, List<string[]> pairList, DateTime[] dates, string periodName)
        {
            _dbRepo.SaveVwapRatio(pairList, exchange, dates, periodName);
        }

        public DateTime? GetLastVwapTimePeriod(DateTime time, string periodName, string exchange)
        {
            return _dbRepo.GetLastVwapTimePeriod(time, periodName, exchange);
        }

        public void SaveLiquidation(LiquidationEventArgs args)
        {
            _dbRepo.SaveLiquidation(args);
        }

        public bool CanInsertCurrencies()
        {
            return _dbRepo.CanInsertCurrencies();
        }

        public void SaveCurrencies(CurrencyRate[] rates)
        {
            _dbRepo.SaveCurrencies(rates);
        }

        public ExchangeSettingsData[] GetExchangeSettings()
        {
            return _dbRepo.GetExchangeSettings();
        }

        public void SaveSpread(DbSpread data)
        {
            _dbRepo.SaveSpread(data);
        }

        public void SaveSpread(DbInstrumentSpread data)
        {
            _dbRepo.SaveSpread(data);
        }

        public BitstampCandle GetBitstampCandle(DateTime dt, string instrument, int periodSeconds)
        {
            var data = _dbRepo.GetBitstampDailyCandles(dt, instrument, periodSeconds);
            double[] result = null;

            if (data.Length > 0)
                result = data.Select(c => c.Price).ToArray();
            else
                return new BitstampCandle();

            return new BitstampCandle { Open = result.First(), Close = result.Last(), High = result.Max(), Low = result.Min() };
        }

        public List<BitstampCandle> GetBitstampCandles(DateTime startDate, DateTime endDate, string instrument, int periodMinutes)
        {
            var result = new List<BitstampCandle>();
            var data = new List<BitstampOrderData>();
            var dtStart = startDate;
            int idx = 0;

            var elapsedSeconds = (int)(endDate - startDate).TotalSeconds;
            var candles = _dbRepo.GetBitstampDailyCandles(startDate, instrument, elapsedSeconds, true);

            while (dtStart < endDate)
            {
                var dt2 = dtStart.AddMinutes(periodMinutes);
                for (; idx < candles.Length; idx++)
                {
                    var c = candles[idx];
                    if (c.DateTime > dt2)
                        break;
                    data.Add(c);
                }
                if (data.Count == 0)
                {
                    result.Add(new BitstampCandle { Time = dt2 });
                }
                else
                    result.Add(new BitstampCandle
                    {
                        Time = dt2,
                        Open = data.First().Price,
                        Close = data.Last().Price,
                        High = data.Max(s => s.Price),
                        Low = data.Min(s => s.Price)
                    });

                dtStart = dtStart.AddMinutes(periodMinutes);
                data.Clear();
            }
            return result;
        }

        public List<BitstampCandle> GetBitstampDailyCandles(DateTime dt, string instrument, int periodMinutes, DateTime? dt1)
        {
            var result  = new List<BitstampCandle>();
            var date    = dt.Date;
            var endDate = dt1.HasValue ? dt1.Value.Date : DateTime.UtcNow.Date;

            while (date <= endDate)
            {
                var candles = _dbRepo.GetBitstampDailyCandles(date, instrument, 86400, true);  // 86400 секунд в сутках
                if (candles.Length == 0)
                {
                    date = date.AddDays(1);
                    continue;
                }
                int idx     = 0;
                var dtStart = date;
                var dtEnd   = date.AddDays(1);
                var data    = new List<BitstampOrderData>();
                //
                // группировка суток по интервалу
                //
                while (dtStart < dtEnd)
                {
                    var dt2 = dtStart.AddMinutes(periodMinutes);
                    for (; idx < candles.Length; idx++)
                    {
                        var c = candles[idx];
                        if (c.DateTime > dt2)
                            break;
                        data.Add(c);
                    }
                    if (data.Count == 0)
                    {
                        result.Add(new BitstampCandle { Time = dt2 });
                    }
                    else
                        result.Add(new BitstampCandle
                        {
                            Time  = dt2,
                            Open  = data.First().Price,
                            Close = data.Last().Price,
                            High  = data.Max(s => s.Price),
                            Low   = data.Min(s => s.Price)
                        });

                    dtStart = dtStart.AddMinutes(periodMinutes);
                    data.Clear();
                }
                date = date.AddDays(1);
            }
            return result;
        }

        public DateTime[] GetChartDates()
        {
            string[] names = _dbRepo.GetChartDates();
            if (names.Length == 0)
                return new DateTime[0];

            Func<string, DateTime> parsing = name =>
            {
                var s = name.Split('_')[0];
                return new DateTime(2000 + int.Parse(s.Substring(0, 2)), int.Parse(s.Substring(2, 2)), int.Parse(s.Substring(4, 2)));
            };
            var dates = new DateTime[2] { parsing(names.First()), parsing(names.Last()) };
            if (dates[1] > DateTime.UtcNow.Date)
                dates[1] = DateTime.UtcNow.Date;

            return dates;
        }

        public List<BitstampTicker> GetBitstampVwap(DateTime dt, int periodMinutes, DateTime? dt1)
        {
            var endDate = dt1.HasValue ? dt1.Value.Date : DateTime.UtcNow.Date;
            var date    = dt.Date;
            var result  = new List<BitstampTicker>();

            while (date <= endDate)
            {
                var d1 = date;
                var d2 = date.AddDays(1);
                var dailyTickers = _dbRepo.GetBitstampTicker(d1, d2);  // за сутки

                if (dailyTickers.Length == 0)
                {
                    date = date.AddDays(1);
                    continue;
                }
                while (d1 < d2)
                {
                    var end = d1.AddMinutes(periodMinutes);
                    var tickers = dailyTickers.Where(t => t.Time.Value >= d1 && t.Time.Value <= end).ToArray();

                    if (tickers.Length == 0)
                        result.Add(new BitstampTicker { Time = d1 });
                    else
                        result.Add(tickers[0]);

                    d1 = d1.AddMinutes(periodMinutes);
                }
                date = date.AddDays(1);
            }
            return result;
        }

        public DbBitstampTrade[] GetBitstampTrades(int secs)
        {
            var result   = new DbBitstampTrade[2];
            var dt       = DateTime.UtcNow;
            var dbTrades = _dbRepo.GetBitstampTrades(dt, secs);
            var trades   = dbTrades.Where(t => t.Side == OrderSide.BID).ToArray();

            if (trades.Length > 0)
            {
                result[0] = new DbBitstampTrade();
                result[0].Amount = trades.Sum(t => t.Amount);
                result[0].Count = trades.Length;
                result[0].Side = OrderSide.BID;
            }
            trades = dbTrades.Where(t => t.Side == OrderSide.ASK).ToArray();
            if (trades.Length > 0)
            {
                result[1] = new DbBitstampTrade();
                result[1].Amount = trades.Sum(t => t.Amount);
                result[1].Count = trades.Length;
                result[1].Side = OrderSide.ASK;
            }
            return result;
        }

        public void NewDatePriceLevel(string levelId, DateTime date)
        {
            _dbRepo.NewDatePriceLevel(levelId, date);
        }

        public void RestorePriceLevel(string levelId)
        {
            _dbRepo.RestorePriceLevel(levelId);
        }

        public void AddBreakDown(string levelId, bool isFalseBreakdown, DateTime dt)
        {
            _dbRepo.AddBreakDown(levelId, isFalseBreakdown, dt);
        }

        public DbPriceLevel AddPriceLevel(double price, bool isLevelUp, string timeframe, DateTime dt1, DateTime dt2)
        {
            return _dbRepo.AddPriceLevel(price, isLevelUp, timeframe, dt1, dt2);
        }

        public DbPriceLevel GetPriceLevelById(string id)
        {
            DbPriceLevel level = null;
            var res = _dbRepo.GetPriceLevelById(id);

            if (res.Length > 0)
            {
                level = new DbPriceLevel { Id = id, Price = res[0].Price };
                level.ConfirmedDates.AddRange(res.Select(x => x.Date2.Value));
            }
            return level;
        }

        public DbPriceLevel[] GetPriceLevels(bool onlyActive)
        {
            var levels = new Dictionary<string, DbPriceLevel>();
            foreach (var x in _dbRepo.GetPriceLevels(onlyActive))
            {
                if (!levels.ContainsKey(x.Id))
                    levels[x.Id] = new DbPriceLevel { Id = x.Id, Price = x.Price, Date2 = x.Date2 };

                var level = levels[x.Id];
                level.ConfirmedDates.Add(x.LevelDate.Value);
                level.IsActual = x.IsActual;
            }
            return levels.Values.ToArray();
        }

        public void RemovePriceLevel(string id, bool removeFromDb)
        {
            _dbRepo.RemovePriceLevel(id, removeFromDb);
        }

        public DbMessage SaveBitmexPosition(PositionRequest request)
        {
            return _dbRepo.SaveBitmexPosition(request);
        }

        public void SaveBitmexPositionState(PositionRequest request)
        {
            _dbRepo.SaveBitmexPositionState(request);
        }

        public DbPositionState GetBitmexPositionState(string account, string hostName, string instrument)
        {
            return _dbRepo.GetBitmexPositionState(account, hostName, instrument);
        }

        public void SaveBitmexTrade(TradeRequest request)
        {
            _dbRepo.SaveBitmexTrade(request);
        }

        public DbIndicatorVwapLite[] GetVwapGains(DateTime date, string exchange)
        {
            return _dbRepo.GetVwapGains(date, exchange);
        }

        public DbMessage GetTrades(string account, string instrument, DateTime startDate, DateTime endDate, int? count = null)
        {
            return _dbRepo.GetTrades(account, instrument, startDate, endDate, count);
        }

        public void SaveBitmexInstruments(BitmexInstrumentSettings[] comm, string accounId)
        {
            _dbRepo.SaveBitmexInstruments(comm, accounId);
        }

        public BitmexInstrumentSettings[] GetBitmexInstruments(string accountId)
        {
            return _dbRepo.GetBitmexInstruments(accountId);
        }

        public void SaveBitmexOrder(OrderRequest request)
        {
            _dbRepo.SaveBitmexOrder(request);
        }

        public void SaveWallet(WalletRequest request)
        {
            _dbRepo.SaveWallet(request);
        }

        public void SaveMargin(MarginRequest request)
        {
            _dbRepo.SaveMargin(request);
        }

        public void LogScalperEvent(string sessionId, DateTime time, string eventType, string message)
        {
            _dbRepo.LogScalperEvent(sessionId, time, eventType, message);
        }

        public ExchangeName ShouldRestartTradeDataBot(int limitMinutes)
        {
            return _dbRepo.ShouldRestartTradeDataBot(limitMinutes);
        }
    }
}
