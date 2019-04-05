using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Database;
using BotCoin.DataType.Exchange;
using BotCoin.DataType.WebApi;
using BotCoin.Exchange;
using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BotCoin.Service
{
    using System.Text;
    using Interval = Tuple<DateTime, DateTime>;

    internal class DbRepository
    {
        readonly object _obj;
        readonly Func<IDbConnection> BotcoinConnection;
        readonly Func<IDbConnection> TradeDataConnection;
        private string _sessionId;

        public DbRepository()
        {
            _obj = new object();

            var conn1 = ConfigurationManager.ConnectionStrings["TradeData"];
            if (conn1 != null)
            {
                TradeDataConnection = () => new SqlConnection(conn1.ConnectionString);
            }
            var conn2 = ConfigurationManager.ConnectionStrings["Botcoin"];
            if (conn2 != null)
            {
                BotcoinConnection = () => new SqlConnection(conn2.ConnectionString);
            }
        }

        public void SetSession(string id)
        {
            _sessionId = id;
        }

        private void TransactionAction(IDbConnection db, Action<SqlTransaction> action, Action<Exception> rollbackAction = null)
        {
            IDbTransaction trans = null;
            db.Open();
            try
            {
                trans = db.BeginTransaction();
                action((SqlTransaction)trans);
                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                if (rollbackAction == null)
                    WriteServiceExceptionEvent(db, ex, ServiceName.WebApi);
                else
                    rollbackAction(ex);
            }
        }

        private void BulkInsert(SqlConnection db, SqlTransaction trans, string tableName, Func<DataTable> action)
        {
            var bulkCopy = new SqlBulkCopy(db, SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.FireTriggers, trans);
            bulkCopy.DestinationTableName = tableName;
            bulkCopy.WriteToServer(action());
        }

        internal void SaveBittrexArbitrageAsync(BittrexArbitrageData data)
        {
            using (var db = BotcoinConnection())
            {
                var args = SaveBittrexProcParams(data);
                db.Execute(DbQueryStr.CreateBittrexArbitrage, args);
            }
        }

        private DynamicParameters SaveBittrexProcParams(BittrexArbitrageData data)
        {
            var args = new DynamicParameters();

            args.Add("@CreatedAt", DateTime.UtcNow);
            args.Add("@Exchange1", (int)data.Exchange1);
            args.Add("@Exchange2", (int)data.Exchange2);
            args.Add("@Instrument1", (int)data.Instrument1);
            args.Add("@Instrument2", (int)data.Instrument2);
            args.Add("@SellPrice", data.SellPrice);
            args.Add("@BuyPrice", data.BuyPrice);
            args.Add("@BittrexRatio", data.BittrexRatio);
            args.Add("@ProfitRatio", data.ProfitRatio);
            args.Add("@ProfitUsd", data.ProfitUsd);
            args.Add("@Ratio", data.Ratio);
            args.Add("@Fees", data.Fees);

            return args;
        }

        internal void SaveMatching(MatchingData data, IExchange ex1 = null, IExchange ex2 = null)
        {
            lock (_obj)
            {
                using (var db = BotcoinConnection())
                {
                    try
                    {
                        var args = SaveMatchingProcParams(data);
                        db.Execute("SaveMatching", args, commandType: CommandType.StoredProcedure);

                        if (ex1 != null && ex2 != null)
                        {
                            SyncBalance(ex1, data.Instrument, GetLastBalances(db, ex1.GetExchangeName()));
                            SyncBalance(ex2, data.Instrument, GetLastBalances(db, ex2.GetExchangeName()));
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteServiceExceptionEvent(db, ex, ServiceName.Arbitrage);
                    }
                }
            }
        }

        private void SyncBalance(IExchange ex, CurrencyName instrument, UserAccount account)
        {
            ex.UsdBalance = Utils.Round(account.UsdBalance, ex, true);
            switch (instrument)
            {
            case CurrencyName.BTC: ex.BtcBalance = Utils.Round(account.BtcBalance, ex, false); break;
            case CurrencyName.BCH: ex.BchBalance = Utils.Round(account.BchBalance.Value, ex, false); break;
            case CurrencyName.LTC: ex.LtcBalance = Utils.Round(account.LtcBalance.Value, ex, false); break;
            case CurrencyName.ETH: ex.EthBalance = Utils.Round(account.EthBalance.Value, ex, false); break;
            }
            Utils.ThrowIf(ex.UsdBalance < 0, "[{0}] USD balance {1}", ex.GetExchangeName(), ex.UsdBalance);
            Utils.ThrowIf(ex.BtcBalance < 0, "[{0}] BTC balance {1}", ex.GetExchangeName(), ex.BtcBalance);
            Utils.ThrowIf(ex.BchBalance < 0, "[{0}] BCH balance {1}", ex.GetExchangeName(), ex.BchBalance);
            Utils.ThrowIf(ex.LtcBalance < 0, "[{0}] LTC balance {1}", ex.GetExchangeName(), ex.LtcBalance);
            Utils.ThrowIf(ex.EthBalance < 0, "[{0}] ETH balance {1}", ex.GetExchangeName(), ex.EthBalance);
        }

        private DynamicParameters SaveMatchingProcParams(MatchingData data)
        {
            var args = new DynamicParameters();

            args.Add("@createdAt", data.CreatedAt);
            args.Add("@exchange1", (int)data.Exchange1);
            args.Add("@exchange2", (int)data.Exchange2);
            args.Add("@profitRatio", Double.IsNaN(data.ProfitRatio) ? -100 : data.ProfitRatio);
            args.Add("@profit", data.Profit);
            args.Add("@dataAmount", data.Amount);
            args.Add("@askAmount", data.AskAmount);
            args.Add("@bidAmount", data.BidAmount);
            args.Add("@askPrice1", data.AskPrice1);
            args.Add("@bidPrice2", data.BidPrice2);
            args.Add("@fees", data.Fees);
            args.Add("@buyUsdAmount", data.BuyUsdAmount);
            args.Add("@sellUsdAmount", data.SellUsdAmount);
            args.Add("@currencyRate1", data.CurrencyRate1);
            args.Add("@currencyRate2", data.CurrencyRate2);
            args.Add("@instrument1", (int)data.Instrument);
            args.Add("@instrument2", (int)data.Instrument);
            args.Add("@transCode", data.TransactionState.ToString());
            args.Add("@order1", data.Order1);
            args.Add("@order2", data.Order2);
            args.Add("@failReason1", data.FailReason1);
            args.Add("@failReason2", data.FailReason2);

            return args;
        }

        private DynamicParameters SaveTradeParams(TradeEventArgs data)
        {
            var args = new DynamicParameters();

            args.Add("@CreatedAt",   data.CreatedAt);
            args.Add("@TradeAt",     data.Timestamp);
            args.Add("@Instrument1", data.Instrument1.ToString());
            args.Add("@Instrument2", data.Instrument2.ToString());            
            args.Add("@TradeId",     data.TradeId);
            args.Add("@Price",       data.Price);
            args.Add("@Quantity",    data.Quantity);
            args.Add("@Volume",      data.Quantity * data.Price);

            if (data.Exchange == ExchangeName.Binance)
            {
                args.Add("@BuyerOrderId", data.BuyerOrderId);
                args.Add("@SellerOrderId", data.SellerOrderId);
                args.Add("@IsBuyerMMaker", data.IsBuyerMarketMaker.Value);
            }
            else
            {
                args.Add("@TradeType", data.TradeType);
            }
            return args;
        }

        private DynamicParameters SaveBinanceTickerParams(TickerEventArgs data)
        {
            var args = new DynamicParameters();
            var t = data.BinanceTicker;

            args.Add("@CreatedAt", data.CreatedAt);
            args.Add("@Instrument1", data.Instrument1.ToString());
            args.Add("@Instrument2", data.Instrument2.ToString());
            args.Add("@Vwap", t.Vwap);
            args.Add("@HighPrice", t.HighPrice);
            args.Add("@OpenPrice", t.OpenPrice);
            args.Add("@LowPrice", t.LowPrice);
            args.Add("@BestBidPrice", t.BestBidPrice);
            args.Add("@BestAskPrice", t.BestAskPrice);
            args.Add("@QtyClose", t.QtyClose);
            args.Add("@BestBidQty", t.BestBidQty);
            args.Add("@BestAskQty", t.BestAskQty);
            args.Add("@PriceChange", t.PriceChange);
            args.Add("@PriceChangePcnt", t.PriceChangePercent);
            args.Add("@ClosePricePrevDay", t.ClosePricePrevDay);
            args.Add("@ClosePriceCurrDay", t.ClosePriceCurrDay);
            args.Add("@TotalBaseVolume", t.TotalTradedBaseVolume);
            args.Add("@TotalQuoteVolume", t.TotalTradeQuoteVolume);
            args.Add("@OpenTimeStats", data.OpenTime);
            args.Add("@CloseTimeStats", data.CloseTime);

            return args;
        }

        private DynamicParameters SaveLiquidationParams(BitmexLiquidationData data, DateTime ts)
        {
            var args = new DynamicParameters();

            args.Add("@Timestamp", ts);
            args.Add("@OrderId", data.OrderId);
            args.Add("@Symbol", data.Symbol);
            args.Add("@Side", data.Side);
            args.Add("@Price", data.Price);
            args.Add("@LeavesQty", data.LeavesQty);

            return args;
        }

        private DynamicParameters SaveBitmexTickerParams(TickerEventArgs data)
        {
            var args = new DynamicParameters();
            var t = data.BitmexTicker;

            if (t.LastPrice.HasValue)
                args.Add("@Price", t.LastPrice.Value);

            if (!String.IsNullOrEmpty(t.LastTickDirection))
                args.Add("@TickDirection", ConvertTickDirection(t.LastTickDirection));

            return args;
        }

        public string ConvertTickDirection(string tick)
        {
            if (tick.StartsWith("Minus"))
                return "-";
            else if (tick.StartsWith("Plus"))
                return "+";
            else if (tick.StartsWith("ZeroMinus"))
                return "0-";
            else if (tick.StartsWith("ZeroPlus"))
                return "0+";
            else
                throw new InvalidOperationException("ConvertTickDirection()");
        }

        private DynamicParameters SaveBitmexInstrumentParams(TickerEventArgs data)
        {
            var args = new DynamicParameters();
            var t = data.BitmexTicker;

            if (t.OpenInterest.HasValue)
                args.Add("@OpenInterest", t.OpenInterest.Value);
            if (t.OpenValue.HasValue)
                args.Add("@OpenValue", t.OpenValue.Value);
            if (t.TotalVolume.HasValue)
                args.Add("@TotalVolume", t.TotalVolume.Value);
            if (t.Volume.HasValue)
                args.Add("@Volume", t.Volume.Value);
            if (t.Volume24h.HasValue)
                args.Add("@Volume24h", t.Volume24h.Value);
            if (t.TotalTurnover.HasValue)
                args.Add("@TotalTurnover", t.TotalTurnover.Value);
            if (t.TurnOver.HasValue)
                args.Add("@TurnOver", t.TurnOver.Value);
            if (t.TurnOver24h.HasValue)
                args.Add("@TurnOver24h", t.TurnOver24h.Value);
            if (!String.IsNullOrEmpty(t.HomeNotional24h))
                args.Add("@HomeNotional24h", t.HomeNotional24h);
            if (t.ForeignNotional24h.HasValue)
                args.Add("@ForeignNotional24h", t.ForeignNotional24h.Value);

            return args;
        }

        private DynamicParameters SaveBitmexFundingParams(TickerEventArgs data)
        {
            var args = new DynamicParameters();
            var t = data.BitmexTicker;

            if (t.Vwap.HasValue)
                args.Add("@Vwap", t.Vwap.Value);
            if (t.HighPrice.HasValue)
                args.Add("@HighPrice", t.HighPrice.Value);
            if (t.LowPrice.HasValue)
                args.Add("@LowPrice", t.LowPrice.Value);
            if (t.FundingRate.HasValue)
                args.Add("@FundingRate", t.FundingRate.Value);
            if (t.IndicativeFundingRate.HasValue)
                args.Add("@IndicativeFundingRate", t.IndicativeFundingRate.Value);
            if (t.FundingTimestamp.HasValue)
                args.Add("@FundingTimestamp", t.FundingTimestamp.Value);

            return args;
        }

        private DynamicParameters SaveBitstampTickerParams(TickerEventArgs data)
        {
            var args = new DynamicParameters();
            var t = data.BitstampTicker;

            args.Add("@CreatedAt", data.CreatedAt);
            args.Add("@Instrument1", data.Instrument1.ToString());
            args.Add("@Instrument2", data.Instrument2.ToString());
            args.Add("@Vwap", t.Vwap);
            args.Add("@HighPrice", t.High);
            args.Add("@OpenPrice", t.Open);
            args.Add("@LowPrice", t.Low);
            args.Add("@BestBidPrice", t.Bid);
            args.Add("@BestAskPrice", t.Ask);
            args.Add("@Volume", t.Volume);

            return args;
        }

        private DynamicParameters SaveGdaxTickerParams(TickerEventArgs data)
        {
            var args = new DynamicParameters();
            var t = data.GdaxTicker;

            args.Add("@CreatedAt", data.CreatedAt);
            args.Add("@Timestamp", t.Time.Value);
            args.Add("@Sequence", t.Sequence);
            args.Add("@Instrument1", data.Instrument1.ToString());
            args.Add("@Instrument2", data.Instrument2.ToString());
            args.Add("@OrderSide", t.Side == "buy" ? "Buy" : "Sell");
            
            if (t.BestBid.HasValue)     args.Add("@BestBidPrice", t.BestBid.Value);
            if (t.BestAsk.HasValue)     args.Add("@BestAskPrice", t.BestAsk.Value);
            if (t.LastSize.HasValue)    args.Add("@LastQty", t.LastSize.Value);
            if (t.Price.HasValue)       args.Add("@Price", t.Price.Value);
            
            return args;
        }

        internal UserAccount GetLastBalances(IDbConnection db, ExchangeName ex)
        {
            using (var result = db.QueryMultiple(DbQueryStr.GetLastBalances, new { Exchange = (int)ex }))
            {
                var query1 = result.Read<UserAccount>().Single();
                var query2 = result.Read<UserAccount>().SingleOrDefault();
                var query3 = result.Read<UserAccount>().SingleOrDefault();
                var query4 = result.Read<UserAccount>().SingleOrDefault();
                var query5 = result.Read<UserAccount>().SingleOrDefault();
                var query6 = result.Read<UserAccount>().SingleOrDefault();
                var query7 = result.Read<UserAccount>().SingleOrDefault();
                var query8 = result.Read<UserAccount>().Single();

                var account = new UserAccount
                {
                    InitUsdBalance = query8.InitUsdBalance,
                    Balance = query1.Balance,
                    CurrencyRate = query1.CurrencyRate,
                    Exchange = ex
                };
                if (query2 != null)
                    account.BtcBalance = query2.BtcBalance;
                if (query3 != null)
                    account.BchBalance = query3.BchBalance;
                if (query4 != null)
                    account.LtcBalance = query4.LtcBalance;
                if (query5 != null)
                    account.EthBalance = query5.EthBalance;
                if (query6 != null)
                    account.XrpBalance = query6.XrpBalance;
                if (query7 != null)
                    account.DashBalance = query7.DashBalance;
                return account;
            }
        }

        internal bool CanUpdateBalances(ExchangeName ex)
        {
            DateTime? time = null;

            using (var db = BotcoinConnection())
                time = db.Query<DateTime?>(DbQueryStr.LastAccount, new { ExchangeId = ex }).SingleOrDefault();

            if (!time.HasValue)
                return true;

            int timeout = Int32.Parse(ConfigurationManager.AppSettings["SyncBalancesTimeoutMinutes"]);
            return DateTime.UtcNow >= time.Value.AddMinutes(timeout);
        }

        internal void UpdateBalances(List<UserAccount> accounts)
        {
            using (var db = BotcoinConnection())
            {
                db.Open();

                IDbTransaction trans = null;
                try
                {
                    trans = db.BeginTransaction();
                    accounts.ForEach(account =>
                    {
                        var accountId = db.Query<int>(DbQueryStr.SyncAccount, new
                        {
                            ExchangeId = account.Exchange,
                            CreatedAt = DateTime.UtcNow,
                            Balance = account.UsdBalance
                        },
                        transaction: trans);

                        db.Execute(String.Format(DbQueryStr.SyncAccountTemplate, "Btc"), new
                        {
                            AccountId = accountId,
                            BtcBalance = account.BtcBalance,
                        },
                        transaction: trans);

                        if (account.EthBalance.HasValue)
                        {
                            db.Execute(String.Format(DbQueryStr.SyncAccountTemplate, "Eth"),
                                       new
                                       {
                                           AccountId = accountId,
                                           EthBalance = account.EthBalance.Value
                                       },
                                       transaction: trans);
                        }
                        if (account.BchBalance.HasValue)
                        {
                            db.Execute(String.Format(DbQueryStr.SyncAccountTemplate, "Bch"),
                                       new
                                       {
                                           AccountId = accountId,
                                           BchBalance = account.BchBalance.Value
                                       },
                                       transaction: trans);
                        }
                        if (account.LtcBalance.HasValue)
                        {
                            db.Execute(String.Format(DbQueryStr.SyncAccountTemplate, "Ltc"),
                                       new
                                       {
                                           AccountId = accountId,
                                           LtcBalance = account.LtcBalance.Value
                                       },
                                       transaction: trans);
                        }
                        if (account.XrpBalance.HasValue)
                        {
                            db.Execute(String.Format(DbQueryStr.SyncAccountTemplate, "Xrp"),
                                       new
                                       {
                                           AccountId = accountId,
                                           XrpBalance = account.XrpBalance.Value
                                       },
                                       transaction: trans);
                        }
                        if (account.DashBalance.HasValue)
                        {
                            db.Execute(String.Format(DbQueryStr.SyncAccountTemplate, "Dash"),
                                       new
                                       {
                                           AccountId = accountId,
                                           DashBalance = account.DashBalance.Value
                                       },
                                       transaction: trans);
                        }
                    });
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    WriteServiceExceptionEvent(db, ex, ServiceName.Arbitrage);
                }
            }
        }

        internal UserAccount GetLastBalances(ExchangeName ex)
        {
            using (var db = BotcoinConnection())
            {
                return GetLastBalances(db, ex);
            }
        }

        internal int GetServiceEventSession()
        {
            using (var db = BotcoinConnection())
            {
                var obj = db.Query<DbSession>(DbQueryStr.GetLastEventSession).SingleOrDefault();
                return obj == null ? 1 : obj.SessionId + 1;
            }
        }

        internal void WriteServiceEvent(IDbConnection db, ServiceEventData data)
        {
            IDbConnection db1 = null;

            if (data.ServiceName == ServiceName.TradeDataBot ||
                data.ServiceName == ServiceName.InitService)
            {
                db1 = db == null ? TradeDataConnection() : db;
            }
            else
                db1 = db == null ? BotcoinConnection() : db;
            try
            {
                db1.Execute(DbQueryStr.CreateEventLog, new
                {
                    @time = data.Timestamp,
                    @sessionId = _sessionId,
                    @exchangeId = data.ExchangeId,
                    @service = data.ServiceName.ToString(),
                    @event = data.EventType.ToString(),
                    @msg = data.Message
                });
            }
            finally
            {
                if (db == null)
                    db1.Dispose();
            }
        }

        internal double GetCurrencyRate(CurrencyName currency)
        {
            using (var db = BotcoinConnection())
            {
                var res = db.Query<DbCurrencyRate>(DbQueryStr.GetCurrencyRates, new { CurrencyId = (int)currency }).Single();
                return res.Rate;
            }
        }

        internal DbExchange GetExchangeInfo(ExchangeName ex)
        {
            using (var db = BotcoinConnection())
                return db.Query<DbExchange>(DbQueryStr.GetExchange, new { ExchangeId = (int)ex }).Single();
        }

        internal void SaveBitmexTrade(TradeEventArgs e)
        {
            if (e.BtxTrades.Length == 0)
                return;

            using (var db = (SqlConnection)TradeDataConnection())
            {
                TransactionAction(db, trans =>
                {
                    var tablePrefix = GetTableNamePrefix(e.BtxTrades[0].Timestamp);
                    BulkInsert(db, trans, String.Format("[dbo].[{0}_TradeBitmex]", tablePrefix), () => CreateBitmexTrades(e.BtxTrades));
                },
                ex => WriteServiceExceptionEvent(null, "[Bitmex] Bulk insert failed. " + ex.Message, ServiceName.TradeDataBot)
                );
            }
        }

        internal void SaveTrade(TradeEventArgs e)
        {
            var args = SaveTradeParams(e);
            var sql = String.Empty;
            if (e.Exchange == ExchangeName.Binance)
            {
                sql = String.Format(DbQueryStr.CreateBinanceTrade, GetTableNamePrefix(e.Timestamp));
            }
            else
            {
                sql = String.Format("{0}_Trade{1}", GetTableNamePrefix(e.Timestamp), e.Exchange.ToString());
                sql = String.Format(DbQueryStr.CreateTradeTemplate, sql);
            }
            using (var db = TradeDataConnection())
            {
                db.Execute(sql, args);
            }
        }

        internal void SaveBitmexTicker(TickerEventArgs e)
        {
            DynamicParameters args = null;
            string sql = null;

            using (var db = TradeDataConnection())
            {
                args = SaveBitmexTickerParams(e);
                var names = args.ParameterNames.ToArray();

                if (names.Length > 0)
                {
                    args.Add("@Timestamp", e.BitmexTicker.Timestamp);
                    args.Add("@Instrument", e.BitmexTicker.Symbol);

                    names = args.ParameterNames.ToArray();
                    var argNames = names.Select(s => "@" + s).ToArray();

                    sql = String.Format(DbQueryStr.CreateBitmexTicker, GetTableNamePrefix(e.CreatedAt), String.Join(",", names), String.Join(",", argNames));
                    db.Execute(sql, args);
                }

                args = SaveBitmexFundingParams(e);
                names = args.ParameterNames.ToArray();

                if (names.Length > 0)
                {
                    args.Add("@Timestamp", e.BitmexTicker.Timestamp);
                    args.Add("@Instrument", e.BitmexTicker.Symbol);

                    names = args.ParameterNames.ToArray();
                    var argNames = names.Select(s => "@" + s).ToArray();

                    sql = String.Format(DbQueryStr.CreateBitmexFunding, GetTableNamePrefix(e.CreatedAt), String.Join(",", names), String.Join(",", argNames));
                    db.Execute(sql, args);
                }

                args = SaveBitmexInstrumentParams(e);
                names = args.ParameterNames.ToArray();

                if (names.Length > 0)
                {
                    args.Add("@Timestamp", e.BitmexTicker.Timestamp);
                    args.Add("@Instrument", e.BitmexTicker.Symbol);

                    names = args.ParameterNames.ToArray();
                    var argNames = names.Select(s => "@" + s).ToArray();

                    sql = String.Format(DbQueryStr.CreateBitmexInstrument, GetTableNamePrefix(e.CreatedAt), String.Join(",", names), String.Join(",", argNames));
                    db.Execute(sql, args);
                }
            }
        }

        internal void SaveLiquidation(LiquidationEventArgs args)
        {
            using (var db = TradeDataConnection())
            {
                TransactionAction(db, trans =>
                {
                    foreach (var liq in args.Data)
                    {
                        var p = SaveLiquidationParams(liq, args.Timestamp);
                        var sql = String.Format(DbQueryStr.CreateBitmexLiquidation, GetTableNamePrefix(args.Timestamp));

                        db.Execute(sql, p, trans);
                    }
                });
            }
        }

        internal void SaveTicker(TickerEventArgs e)
        {
            DynamicParameters args = null;
            string sql = null;

            if (e.Exchange == ExchangeName.Bitmex)
            {
                SaveBitmexTicker(e);
                return;
            }
            if (e.Exchange == ExchangeName.Binance)
            {
                args = SaveBinanceTickerParams(e);
                sql = String.Format(DbQueryStr.CreateBinanceTicker, GetTableNamePrefix(e.CreatedAt));
            }
            else if (e.Exchange == ExchangeName.Bitstamp)
            {
                args = SaveBitstampTickerParams(e);
                sql = String.Format(DbQueryStr.CreateBitstampTicker, GetTableNamePrefix(e.CreatedAt));
            }
            else if (e.Exchange == ExchangeName.Gdax)
            {
                args = SaveGdaxTickerParams(e);
                var names = args.ParameterNames.ToArray();
                var argNames = names.Select(s => "@" + s).ToArray();
                sql = String.Format(DbQueryStr.CreateGdaxTicker, GetTableNamePrefix(e.GdaxTicker.Time.Value), String.Join(",", names), String.Join(",", argNames));
            }
            else
                throw new NotImplementedException();

            using (var db = TradeDataConnection())
            {
                db.Execute(sql, args);
            }
        }

        private string GetTableNameSuffix()
        {
            return GetTableNamePrefix(DateTime.UtcNow);
        }

        private string GetTableNamePrefix(DateTime dt)
        {
            return String.Format("{0:00}{1:00}{2:00}", dt.Year - 2000, dt.Month, dt.Day);
        }

        private Tuple<string,DynamicParameters,string> CreateOrderBookParameters(ExchangePricesEventArgs e, OrderBook orders)
        {
            var suffix       = GetTableNamePrefix(e.CreatedAt);
            var item3        = String.Format("dbo.[{0}_OrderBookValue{1}]", suffix, e.Exchange.ToString());
            var exchangeName = String.Format("{0}_OrderBook{1}", suffix, e.Exchange.ToString());
            var sql          = String.Format(DbQueryStr.CreateOrderBookTemplate, exchangeName, "", "");
            var parameters   = new DynamicParameters();

            if (e.Exchange == ExchangeName.Gdax)
            {
                parameters = e.Gdax.ToSqlParameters();
                var names = parameters.ParameterNames.ToArray();
                var argNames = names.Select(s => "@" + s).ToArray();
                sql = String.Format(DbQueryStr.CreateGdaxOrderBook, exchangeName, String.Join(",", names), String.Join(",", argNames));

                return new Tuple<string, DynamicParameters, string>(sql, parameters, item3);
            }

            var bids = orders.Bids;
            var asks = orders.Asks;

            parameters.Add("@CreatedAt", e.CreatedAt);
            parameters.Add("@Instrument1", e.Instrument1.ToString());
            parameters.Add("@Instrument2", e.Instrument2.ToString());

            Order bid = null, ask = null;

            if (bids != null && bids.Length > 0) bid = bids.First();
            if (asks != null && asks.Length > 0) ask = asks.First();

            if (PusherExchange(e.Exchange))
            {
                parameters.Add("@BidPrice", bid == null ? 0 : bid.Price);
                parameters.Add("@BidAmount", bid == null ? 0 : bid.Amount);
                parameters.Add("@AskPrice", ask == null ? 0 : ask.Price);
                parameters.Add("@AskAmount", ask == null ? 0 : ask.Amount);

                if (e.Exchange == ExchangeName.Bitstamp)
                {
                    parameters.Add("@Timestamp", e.Timestamp);
                    parameters.Add("@MicroTimestamp", e.MicroTimestamp);
                    parameters.Add("@OrderId", e.OrderId);
                    parameters.Add("@IsDeleted", e.IsOrderDeleted);
                    sql = String.Format(DbQueryStr.CreatePusherOrderBookTemplate, exchangeName, "Timestamp,MicroTimestamp,IsDeleted,OrderId,", "@Timestamp,@MicroTimestamp,@IsDeleted,@OrderId,");
                }
                else
                    sql = String.Format(DbQueryStr.CreatePusherOrderBookTemplate, exchangeName, "", "");
            }
            else
            {
                double avg = 0.0;

                if (bid != null)
                {
                    var amount = orders.Bids.Where(o => o.Amount >= 1.0).ToArray();
                    if (amount.Length > 0)
                        avg = amount.Average(o => o.Amount);

                    parameters.Add("@BidPrice", bid.Price);
                    parameters.Add("@SumBid", orders.Bids.Sum(o => o.Amount));
                    parameters.Add("@AvgBid", avg);
                    parameters.Add("@MinBid", orders.Bids.Min(o => o.Amount));
                    parameters.Add("@MaxBid", orders.Bids.Max(o => o.Amount));
                }
                if (ask != null)
                {
                    var amount = orders.Asks.Where(o => o.Amount >= 1.0).ToArray();
                    if (amount.Length > 0)
                        avg = amount.Average(o => o.Amount);

                    parameters.Add("@AskPrice", ask.Price);
                    parameters.Add("@SumAsk", orders.Asks.Sum(o => o.Amount));
                    parameters.Add("@AvgAsk", avg);
                    parameters.Add("@MinAsk", orders.Asks.Min(o => o.Amount));
                    parameters.Add("@MaxAsk", orders.Asks.Max(o => o.Amount));
                }
                if (bid != null && ask != null)
                {
                    parameters.Add("@Spread", Math.Round(ask.Price - bid.Price, 8));
                }
            }
            return new Tuple<string, DynamicParameters, string>(sql, parameters, item3);
        }

        internal void SaveOrderBook(ExchangePricesEventArgs args, OrderBook orders)
        {
            bool needTrans = args.Exchange == ExchangeName.Binance;
            var tuple = CreateOrderBookParameters(args, orders);

            SqlTransaction trans = null;

            using (var db = (SqlConnection)TradeDataConnection())
            {
                db.Open();
                if (needTrans)
                    trans = db.BeginTransaction();
                try
                {
                    var orderBookId = db.Query<int>(tuple.Item1, tuple.Item2, trans).Single();
                    if (needTrans)
                    {
                        if (args.Instrument1 == CurrencyName.BTC)
                        {
                            BulkInsert(db, trans, tuple.Item3, () => CreateOrderBookValues(args, orderBookId, orders.Bids, orders.Asks));
                        }
                    }
                    if (needTrans)
                        trans.Commit();
                }
                catch (Exception ex)
                {
                    if (needTrans) trans.Rollback();
                    WriteServiceExceptionEvent(null, String.Format("[{0}] Bulk insert failed. {1}", args.Exchange.ToString(), ex.Message), ServiceName.TradeDataBot);
                }
            }
        }

        private bool PusherExchange(ExchangeName ex)
        {
            return ex == ExchangeName.Bitstamp || ex == ExchangeName.Wex || ex == ExchangeName.Bitmex;
        }

        private DataTable CreateOrderBookValues(ExchangePricesEventArgs args, int orderBookId, Order[] bids, Order[] asks)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("OrderBookId", typeof(int));
            table.Columns.Add("PriceType", typeof(short));
            table.Columns.Add("Price", typeof(double));
            table.Columns.Add("Amount", typeof(double));

            Action<short, double, double> createRow = (type, price, amount) =>
            {
                DataRow row = table.NewRow();
                row["OrderBookId"]  = orderBookId;
                row["PriceType"]    = type;
                row["Price"]        = price;
                row["Amount"]       = amount;
                table.Rows.Add(row);
            };

            if (asks != null)
            {
                asks = asks.OrderByDescending(a => a.Price).ToArray();

                foreach (var ask in asks)
                    createRow(1, ask.Price, ask.Amount);
            }
            if (bids != null)
                foreach (var bid in bids)
                    createRow(0, bid.Price, bid.Amount);

            return table;
        }

        internal bool CanInsertCurrencies()
        {
            DateTime? time = null;

            using (var db = BotcoinConnection())
                time = db.Query<DateTime?>(DbQueryStr.LastCurrencyUpdate).SingleOrDefault();

            if (!time.HasValue)
                return true;

            return time.Value.Date != DateTime.UtcNow.Date;
        }

        internal ExchangeSettingsData[] GetExchangeSettings()
        {
            using (var db = BotcoinConnection())
                return db.Query<ExchangeSettingsData>(DbQueryStr.GetExchangeSettings).ToArray();
        }

        internal void SaveCurrencies(CurrencyRate[] rates)
        {
            using (var db = BotcoinConnection())
            {
                db.Open();
                var trans = db.BeginTransaction();
                try
                {
                    foreach (var rate in rates)
                    {
                        db.Execute(DbQueryStr.CreateCurrencyRate,
                                   new { Currency = (int)rate.Currency, Rate = Math.Round(rate.Rate, 3), ExchangeDate = rate.ExchangeDate },
                                   transaction: trans);
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    WriteServiceExceptionEvent(db, ex, ServiceName.Arbitrage);
                    throw ex;
                }
            }
        }

        private void WriteServiceExceptionEvent(IDbConnection db, string message, ServiceName service)
        {
            WriteServiceEvent(db, new ServiceEventData
            {
                ServiceName = service,
                EventType = ServiceEventType.Fail,
                Message = message
            });
        }

        private void WriteServiceExceptionEvent(IDbConnection db, Exception ex, ServiceName service)
        {
            WriteServiceExceptionEvent(db, ex.Message + ex.StackTrace, service);
        }

        internal void SaveSpread(DbSpread data)
        {
            var args = new DynamicParameters();

            args.Add("@Instrument", data.Instrument1.ToString());
            args.Add("@Exchange1", data.Exchange1.ToString());
            args.Add("@Exchange2", data.Exchange2.ToString());
            args.Add("@SpreadPercent", data.SpreadPercent);
            args.Add("@CreatedAt", data.CreatedAt);
            args.Add("@BidAltcUsd", data.BidAltcUsd);
            args.Add("@AskAltcUsd", data.AskAltcUsd);
            args.Add("@BidAltcInstr2", data.BidAltcInstr2);
            args.Add("@AskAltcInstr2", data.AskAltcInstr2);
            args.Add("@BidUsd1", data.BidUsd1);
            args.Add("@AskUsd1", data.AskUsd1);
            args.Add("@BidUsd2", data.BidUsd2);
            args.Add("@AskUsd2", data.AskUsd2);

            using (var db = BotcoinConnection())
            {
                db.Execute(DbQueryStr.CreateSpread, args);
            }
        }

        internal void SaveSpread(DbInstrumentSpread data)
        {
            var args = new DynamicParameters();

            args.Add("@Instrument1", data.Instrument1.ToString());
            args.Add("@Instrument2", data.Instrument2.ToString());
            args.Add("@Exchange1", data.Exchange1.ToString());
            args.Add("@Exchange2", data.Exchange2.ToString());
            args.Add("@Spread1", data.Spread1);
            args.Add("@Spread2", data.Spread2);
            args.Add("@CreatedAt", data.CreatedAt);
            args.Add("@Bid1", data.Bid1);
            args.Add("@Bid2", data.Bid2);
            args.Add("@Ask1", data.Ask1);
            args.Add("@Ask2", data.Ask2);

            using (var db = BotcoinConnection())
            {
                db.Execute(DbQueryStr.CreateInstrumentSpread, args);
            }
        }

        internal DbBitstampTrade[] GetBitstampTrades(DateTime dt2, int secs)
        {
            var sql = String.Format(DbQueryStr.GetBitstampTradesTemplate, GetTableNameSuffix());
            var dt1 = dt2.AddSeconds(secs * -1);

            using (var db = TradeDataConnection())
            {
                return db.Query<DbBitstampTrade>(sql, new { TradeAt1 = (DateTime)dt1, TradeAt2 = (DateTime)dt2 }).ToArray();
            }
        }

        internal BitstampTicker[] GetBitstampTicker(DateTime dt1, DateTime dt2)
        {
            var sql = String.Format(DbQueryStr.GetBitstampTickerTemplate, GetTableNamePrefix(dt1));
            using (var db = TradeDataConnection())
            {
                return db.Query<BitstampTicker>(sql, new { Time1 = (DateTime)dt1, Time2 = (DateTime)dt2 }).ToArray();
            }
        }

        internal string[] GetChartDates()
        {
            using (var db = TradeDataConnection())
            {
                return db.Query<string>(DbQueryStr.GetTradeTableNames).ToArray();
            }
        }

        internal BitstampOrderData[] GetBitstampDailyCandles(DateTime dt2, string instrument, int secs, bool readForward = false)
        {
            DateTime? dt1;
            var sql = String.Format(DbQueryStr.GetBitstampCandleTemplate, GetTableNamePrefix(dt2));

            if (readForward)
            {
                dt1 = dt2;
                dt2 = dt2.AddSeconds(secs);
            }
            else
                dt1 = dt2.AddSeconds(secs * -1);

            using (var db = TradeDataConnection())
            {
                BitstampOrderData[] data = null;
                try
                {
                    var args = new { Time1 = (DateTime)dt1.Value, Time2 = (DateTime)dt2, Instrument = instrument };
                    data = db.Query<BitstampOrderData>(sql, args).ToArray();
                }
                catch
                {
                    data = new BitstampOrderData[0];
                }
                return data;
            }
        }

        internal BitstampOrderData[] GetBitstampVolume(DateTime dt1, DateTime dt2)
        {
            BitstampOrderData[] bids = null;
            BitstampOrderData[] asks = null;
            var sql = String.Format(DbQueryStr.GetBitstampOrdersTemplate, GetTableNameSuffix());

            using (var db = TradeDataConnection())
            {
                using (var dbResult = db.QueryMultiple(sql, new { TradeAt1 = (DateTime)dt1, TradeAt2 = (DateTime)dt2 }))
                {
                    bids = dbResult.Read<BitstampOrderData>().ToArray();
                    asks = dbResult.Read<BitstampOrderData>().ToArray();
                }
            }
            var result = new List<BitstampOrderData>();
            foreach (var b in bids)
                result.Add(new BitstampOrderData { Price = b.Price, Amount = b.Amount, OrderType = "Buy" });
            foreach (var a in asks)
                result.Add(new BitstampOrderData { Price = a.Price, Amount = a.Amount, OrderType = "Sell" });

            return result.ToArray();
        }

        internal void NewDatePriceLevel(string levelId, DateTime date)
        {
            using (var db = BotcoinConnection())
            {
                db.Execute(DbQueryStr.NewDatePriceLevel, new { LevelDate = date, LevelId = levelId });
            }
        }

        internal void RestorePriceLevel(string levelId)
        {
            using (var db = BotcoinConnection())
            {
                db.Execute(DbQueryStr.RestorePriceLevel, new { LevelId = levelId });
            }
        }

        internal void AddBreakDown(string levelId, bool isFalseBreakdown, DateTime date)
        {
            using (var db = BotcoinConnection())
            {
                db.Execute(DbQueryStr.CreateBreakdown, new { LevelId = levelId, Flag = isFalseBreakdown, Date = date });
            }
        }

        internal DbPriceLevel AddPriceLevel(double price, bool isLevelUp, string timeFrame, DateTime confirmDate, DateTime dt2)
        {
            var levelId = "lvl" + price.ToString();
            using (var db = BotcoinConnection())
            {
                db.Open();
                var trans = db.BeginTransaction();
                try
                {
                    db.Execute(DbQueryStr.CreatePriceLevel, new
                    {
                        LevelId = levelId,
                        IsLevelUp = isLevelUp,
                        TimeFrame = timeFrame,
                        Price = (double)price,
                        Date2 = (DateTime)dt2
                    }, trans);

                    db.Execute(DbQueryStr.NewDatePriceLevel, new
                    {
                        LevelId = levelId,
                        LevelDate = (DateTime)confirmDate
                    }, trans);

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    WriteServiceExceptionEvent(db, ex, ServiceName.WebApi);
                    throw ex;
                }
            }

            var level = new DbPriceLevel { Id = levelId, Price = price, Date2 = dt2 };
            level.ConfirmedDates.Add(confirmDate);

            return level;
        }

        internal DbPriceLevel[] GetPriceLevelById(string id)
        {
            using (var db = BotcoinConnection())
            {
                return db.Query<DbPriceLevel>(DbQueryStr.GetPriceLevelById, new { LevelId = id }).ToArray();
            }
        }

        internal DbPriceLevel[] GetPriceLevels(bool onlyActive)
        {
            using (var db = BotcoinConnection())
            {
                string sql = DbQueryStr.GetPriceLevelsTemplate;
                if (onlyActive)
                {
                    sql += " AND IsActual=1";
                }
                return db.Query<DbPriceLevel>(sql).ToArray();
            }
        }

        internal void RemovePriceLevel(string id, bool removeFromDb)
        {
            using (var db = BotcoinConnection())
            {
                db.Open();
                var trans = db.BeginTransaction();
                try
                {
                    db.Execute(removeFromDb ? DbQueryStr.RemovePriceLevel : DbQueryStr.UpdatePriceLevel, new { LevelId = id }, trans);
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    WriteServiceExceptionEvent(db, ex, ServiceName.WebApi);
                    throw ex;
                }
            }
        }

        private DynamicParameters CreateOrderParams(BitmexOrderData data, string accountId, out string[] names, out string[] argNames)
        {
            var args = new DynamicParameters();

            args.Add("@AccountId", accountId);
            args.Add("@Instrument", data.Symbol);
            args.Add("@CreatedAt", data.Timestamp);
            args.Add("@OrderId", data.OrderId);
            args.Add("@OrdStatus", data.OrdStatus);
            args.Add("@OrdType", data.OrdType);
            args.Add("@OrderQty", data.OrderQty);
            if (!String.IsNullOrEmpty(data.Side))
                args.Add("@OrdSide", data.Side);
            if (!String.IsNullOrEmpty(data.ExecInst))
                args.Add("@ExecInst", data.ExecInst);
            if (data.LeavesQty.HasValue)
                args.Add("@LeavesQty", data.LeavesQty.Value);
            if (data.CumQty.HasValue)
                args.Add("@CumQty", data.CumQty.Value);
            if (data.Price.HasValue)
                args.Add("@Price", data.Price.Value);
            if (data.StopPx.HasValue)
                args.Add("@StopPrice", data.StopPx.Value);
            if (data.AvgPx.HasValue)
                args.Add("@AvgPrice", data.AvgPx.Value);
            if (!String.IsNullOrEmpty(data.Text))
                args.Add("@OrdText", data.Text);

            names = args.ParameterNames.ToArray();
            argNames = names.Select(s => "@" + s).ToArray();

            return args;
        }

        private DynamicParameters CreateWalletParams(BitmexWalletData wallet, out string[] names, out string[] argNames)
        {
            var args = new DynamicParameters();

            args.Add("@AccountId", wallet.Account);
            args.Add("@Timestamp", wallet.Timestamp);
            args.Add("@Address", wallet.Addr);
            args.Add("@DeltaAmount", BitmexMargin.ToBtc(wallet.DeltaAmount));
            args.Add("@Balance", wallet.BalanceXBT);

            if (wallet.Withdrawn.HasValue)
                args.Add("@Withdrawn", wallet.Withdrawn.Value);
            if (wallet.DeltaDeposited.HasValue)
                args.Add("@DeltaDeposited", wallet.DeltaDeposited.Value);
            if (wallet.DeltaWithdrawn.HasValue)
                args.Add("@DeltaWithdrawn", wallet.DeltaWithdrawn.Value);

            names = args.ParameterNames.ToArray();
            argNames = names.Select(s => "@" + s).ToArray();

            return args;
        }

        private DynamicParameters CreateBitmexPositionStateParams(DbPositionState state, string accountId)
        {
            var args = new DynamicParameters();

            args.Add("@Opened", 0);
            args.Add("@AccountId", accountId);
            args.Add("@Instrument", state.Instrument);
            args.Add("@Timestamp", DateTime.UtcNow);
            args.Add("@HostName", state.HostName);
            args.Add("@StateName", state.StateName);
            args.Add("@OrderQty", state.OrderQty);
            args.Add("@LongPosition", state.LongPosition);
            args.Add("@StopOrderId", state.StopOrderId);
            args.Add("@StopLoss", state.StopLoss);
            args.Add("@StopSlip", state.StopSlip);
            args.Add("@StartWatchPrice", state.StartWatchPrice);
            args.Add("@StopPrice", state.StopPrice);

            return args;
        }

        private DynamicParameters CreateMarginParams(BitmexMarginData margin, out string[] names, out string[] argNames)
        {
            var args = new DynamicParameters();

            args.Add("@AccountId", margin.Account);
            args.Add("@Timestamp", margin.Timestamp);
            if (margin.WalletBalance.HasValue)
                args.Add("@WalletBalance", BitmexMargin.ToBtc(margin.WalletBalance.Value));
            if (margin.MarginBalance.HasValue)
                args.Add("@MarginBalance", BitmexMargin.ToBtc(margin.MarginBalance.Value));
            if (margin.AvailableMargin.HasValue)
                args.Add("@AvailableMargin", BitmexMargin.ToBtc(margin.AvailableMargin.Value));
            if (margin.MarginUsedPcnt.HasValue)
                args.Add("@MarginUsedPcnt", margin.MarginUsedPcnt.Value);
            if (margin.RealisedPnl.HasValue)
                args.Add("@RealisedPnl", BitmexMargin.ToBtc(margin.RealisedPnl.Value));
            if (margin.GrossComm.HasValue)
                args.Add("@GrossComm", BitmexMargin.ToBtc(margin.GrossComm.Value));

            names = args.ParameterNames.ToArray();
            argNames = names.Select(s => "@" + s).ToArray();

            return args;
        }

        internal void SaveBitmexOrder(OrderRequest state)
        {
            string[] names;
            string[] argNames;
            var ord1 = state.Order;

            using (var db = BotcoinConnection())
            {
                var args = CreateOrderParams(ord1, state.User.Id, out names, out argNames);
                var sql = String.Format(DbQueryStr.CreateBitmexOrder, String.Join(",", names), String.Join(",", argNames));

                var ord2 = db.Query<DbOrder>(DbQueryStr.GetBitmexOrderForUpdate, new { OrderId = ord1.OrderId }).SingleOrDefault();
                if (ord2 == null)
                    goto exec;

                if (!String.IsNullOrEmpty(ord1.OrdStatus) && !String.IsNullOrEmpty(ord2.OrdStatus))
                    if (ord1.OrdStatus != ord2.OrdStatus) goto exec;

                if (ord1.Price.HasValue && ord2.Price.HasValue)
                    if (ord1.Price.Value != ord2.Price.Value) goto exec;

                if (ord1.StopPx.HasValue && ord2.StopPrice.HasValue)
                    if (ord1.StopPx.Value != ord2.StopPrice.Value) goto exec;

                if (ord1.OrderQty.HasValue && ord2.OrderQty.HasValue)
                    if (ord1.OrderQty.Value != ord2.OrderQty.Value) goto exec;

                if (ord1.CumQty.HasValue && ord2.CumQty.HasValue)
                    if (ord1.CumQty.Value != ord2.CumQty.Value) goto exec;

                if (ord1.LeavesQty.HasValue && ord2.LeavesQty.HasValue)
                    if (ord1.LeavesQty.Value != ord2.LeavesQty.Value) goto exec;

                if (ord1.AvgPx.HasValue && ord2.AvgPrice.HasValue)
                    if (ord1.AvgPx.Value != ord2.AvgPrice.Value) goto exec;

                return;
exec:
                db.Execute(sql, args);
            }
        }

        internal void LogScalperEvent(string sessionId, DateTime time, string eventType, string message)
        {
            using (var db = BotcoinConnection())
            {
                db.Execute(DbQueryStr.CreateScalperEvent, new
                {
                    SessionId = sessionId,
                    Timestamp = time,
                    EventType = eventType,
                    Message = message
                });
            }
        }

        private DynamicParameters CreatePositionParams(BitmexTradeHistory trade, string positionId, double? balance, string accountId, out string[] names, out string[] argNames)
        {
            var args = new DynamicParameters();

            if (String.Compare(trade.Side, "Sell", true) == 0)
                trade.OrderQty *= -1;

            args.Add("@TransactTime", trade.TransactTime);
            args.Add("@Side", trade.Side);
            args.Add("@OrderQty", trade.OrderQty);
            args.Add("@LeavesQty", trade.LeavesQty);
            args.Add("@CumQty", trade.CumQty);
            args.Add("@OrderId", trade.OrderId);
            args.Add("@OrdStatus", trade.OrdStatus);
            args.Add("@OrdText", trade.Text);
            args.Add("@Instrument", trade.Symbol);
            args.Add("@AccountId", accountId);
            args.Add("@PositionId", positionId);

            if (balance.HasValue)
                args.Add("@Balance", balance.Value);
            if (trade.LastQty.HasValue)
                args.Add("@LastQty", trade.LastQty.Value);
            if (trade.LastPx.HasValue)
                args.Add("@LastPrice", trade.LastPx.Value);
            if (trade.Price.HasValue)
                args.Add("@Price", trade.Price.Value);
            if (trade.ExecComm.HasValue)
                args.Add("@FeePaidXBT", BitmexMargin.ToBtc(trade.ExecComm.Value));
            if (trade.ExecCost.HasValue)
                args.Add("@ExecCostXBT", BitmexMargin.ToBtc(trade.ExecCost.Value));
            if (trade.Commission.HasValue)
                args.Add("@FeeRate", trade.Commission.Value);

            names = args.ParameterNames.ToArray();
            argNames = names.Select(s => "@" + s).ToArray();

            return args;
        }

        private DynamicParameters UpdateBitmexTradeParams(DateTime endTime, double priceGain, string elapsedTime,
                                                          double closePrice, double feePaidXbt, double realisedPnl,
                                                          double stopValue, string positionId, long orderQty,
                                                          out string[] names, out string[] argNames)
        {
            var args = new DynamicParameters();

            args.Add("@EndTime", endTime);
            args.Add("@OrderQty", orderQty);
            args.Add("@ElapsedTime", elapsedTime);
            args.Add("@PriceGain", priceGain);
            args.Add("@ClosePrice", closePrice);
            args.Add("@FeePaidXBT", feePaidXbt);
            args.Add("PositionId", positionId);
            args.Add("@RealisedPnlXBT", realisedPnl);
            args.Add("@TakeStopRatio", Math.Round(priceGain / stopValue, 2));

            names = args.ParameterNames.ToArray();
            argNames = names.Select(s => "@" + s).ToArray();

            return args;
        }

        private DynamicParameters CreateBitmexStopPriceParams(TradeRequest req)
        {
            var args = new DynamicParameters();

            args.Add("@PositionId", req.Position.PositionId);
            args.Add("@Timestamp", DateTime.UtcNow);
            args.Add("@StopPrice", req.StopPrice);
            args.Add("@StartWatchPrice", req.StartWatchPrice);

            return args;
        }

        
        private DynamicParameters CreateBitmexTradeParams(TradeRequest req, out string[] names, out string[] argNames)
        {
            var args = new DynamicParameters();
            var pos = req.Position;

            args.Add("@TradeType", pos.Side == "Buy" ? "LONG" : "SHORT");
            args.Add("@StartTime", pos.TransactTime);
            args.Add("@OrderQty", pos.OrderQty);
            args.Add("@OpenPrice", pos.Price);
            args.Add("@RiskPcnt", req.RiskPercent);
            args.Add("@StopValue", req.StopValue);
            args.Add("@PositionId", pos.PositionId);
            args.Add("@Instrument", pos.Instrument);
            args.Add("@AccountId", pos.AccountId);

            names = args.ParameterNames.ToArray();
            argNames = names.Select(s => "@" + s).ToArray();

            return args;
        }

        private string ConvertToElapsedTime(TimeSpan ts)
        {
            var str = new StringBuilder();

            if (ts.Days > 0) str.AppendFormat("{0}d ", ts.Days);
            if (ts.Hours > 0) str.AppendFormat("{0}h ", ts.Hours);
            if (ts.Minutes > 0) str.AppendFormat("{0} min ", ts.Minutes);
            if (ts.Seconds > 0) str.AppendFormat("{0} sec", ts.Seconds);

            return str.ToString();
        }

        private void CalculatePnl(string side, long qty, double price1, double price2, out double realisedPnl, out double priceGain)
        {
            if (String.Compare(side, "Buy", true) == 0)
            {
                realisedPnl = qty * (1 / price1 - 1 / price2);
                priceGain = price2 - price1;
            }
            else
            {
                realisedPnl = qty * (1 / price2 - 1 / price1);
                priceGain = price1 - price2;
            }
        }

        internal DbPositionState GetBitmexPositionState(string account, string hostName, string instrument)
        {
            DbPositionState state = null;
            IDbTransaction trans = null;

            using (var db = BotcoinConnection())
            {
                db.Open();
                try
                {
                    trans = db.BeginTransaction();

                    var args = new { AccountId = account, Instrument = instrument };
                    state = db.Query<DbPositionState>(DbQueryStr.GetBtxScalperState, args, trans).SingleOrDefault();

                    if (state != null && state.Opened == 0)
                    {
                        var sqlArgs = String.Format("HostName='{0}',Opened=1", hostName);
                        var sql = String.Format(DbQueryStr.UpdateBtxScalperState, sqlArgs);
                        db.Execute(sql, new { Id = state.Id }, trans);
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    WriteServiceExceptionEvent(db, ex, ServiceName.WebApi);
                }
            }
            return state;
        }

        internal void SaveBitmexPositionState(PositionRequest request)
        {
            using (var db = BotcoinConnection())
            {
                TransactionAction(db, trans =>
                {
                    foreach (var state in request.PositionStates)
                    {
                        var args     = CreateBitmexPositionStateParams(state, request.User.Id);
                        var names    = args.ParameterNames.ToArray();
                        var argNames = names.Select(s => "@" + s).ToArray();
                        var args1    = new { AccountId = request.User.Id, Instrument = state.Instrument };
                        var dbState  = db.Query<DbPositionState>(DbQueryStr.GetBtxScalperState, args1, trans).SingleOrDefault();
                        string sql   = null;

                        if (dbState != null)
                        {
                            var str = new StringBuilder();
                            for (int i = 0; i < argNames.Length; i++)
                            {
                                str.AppendFormat("{0}={1}", names[i], argNames[i]);
                                if (i + 1 < argNames.Length) str.Append(",");
                            }
                            sql = String.Format(DbQueryStr.UpdateBtxScalperState, str.ToString());
                            args.Add("@Id", dbState.Id);
                        }
                        else
                            sql = String.Format(DbQueryStr.SaveBtxScalperState, String.Join(",", names), String.Join(",", argNames));

                        db.Execute(sql, args, trans);
                    }
                });
            }
        }

        private double GetWalletBalance(IDbConnection db, string accountId, string positionId, IDbTransaction trans)
        {
            string sql = null;

            if (accountId != null)
                sql = String.Format(DbQueryStr.GetBalance, accountId);
            else
                sql = String.Format(DbQueryStr.GetPositionBalance, positionId);

            return db.Query<double>(sql, null, trans).Single();
        }

        internal DbMessage SaveBitmexPosition(PositionRequest state)
        {
            var positionId = Guid.NewGuid().ToString();
            var msg        = new DbMessage();
            var trade      = state.Trade;
            var isOpen     = trade.Text == "OPN";

            double? balance = null;
            string[] names, argNames;

            using (var db = BotcoinConnection())
            {
                TransactionAction(db, trans =>
                {
                    #region Create position
                    if (isOpen)
                    {
                        balance = GetWalletBalance(db, state.User.Id, null, trans);
                    }
                    else
                    {
                        var arg = new { AccountId = state.User.Id, Instrument = trade.Symbol };
                        positionId = db.Query<string>(DbQueryStr.GetBitmexLastPositionId, arg, trans).SingleOrDefault();
                    }
                    var args1 = CreatePositionParams(trade, positionId, balance, state.User.Id, out names, out argNames);
                    var sql = String.Format(DbQueryStr.CreateBitmexPosition, String.Join(",", names), String.Join(",", argNames));
                    msg.Positions = new DbPosition[] { CreateDbPosition(trade, positionId, state.User.Id, isOpen) };

                    int rowId = db.Query<int>(sql, args1, trans).Single();
                    if (isOpen) return;
                    #endregion

                    #region Position changed
                    sql = String.Format(DbQueryStr.GetPositionSide, positionId);
                    var side = db.Query<string>(sql, null, trans).Single();

                    if (String.Compare(trade.Side, side, true) == 0)
                    {
                        return;
                    }
                    else
                    {
                        sql = String.Format(DbQueryStr.GetPositionTotalQty, positionId);
                        var qtySum = db.Query<long>(sql, null, trans).Single();

                        if (qtySum != 0) return;
                    }
                    #endregion

                    #region Save trade                    
                    sql = String.Format(DbQueryStr.CalculatePostionVwap, positionId);

                    using (var result = db.QueryMultiple(sql, null, trans))
                    {
                        var qty    = result.Read<long>().Single();
                        var fees   = result.Read<double>().Single();
                        var price1 = result.Read<double>().Single();
                        var price2 = result.Read<double>().Single();

                        double priceGain = 0, realisedPnl = 0;
                        CalculatePnl(side, qty, price1, price2, out realisedPnl, out priceGain);

                        var dbTrade = db.Query<DbTrade>(DbQueryStr.GetBitmexBotTradeById, new { PositionId = positionId }, trans).Single();
                        var elapsedTime = ConvertToElapsedTime(trade.TransactTime - dbTrade.StartTime);

                        var args3 = UpdateBitmexTradeParams(
                                        trade.TransactTime,
                                        priceGain,
                                        elapsedTime,
                                        price2,
                                        fees,
                                        realisedPnl,
                                        dbTrade.StopValue,
                                        positionId,
                                        qty,
                                        out names, out argNames
                                        );
                        sql = String.Format(DbQueryStr.UpdateBitmexBotTrade, String.Join(",", names), String.Join(",", argNames));
                        msg.Trades = new DbTrade[] { db.Query<DbTrade>(sql, args3, trans).Single() };

                        balance = GetWalletBalance(db, null, positionId, trans);
                        balance -= realisedPnl + fees;

                        db.Execute(DbQueryStr.UpdateBitmexPosition, new { Id = rowId, Balance = balance }, trans);
                    }
                    #endregion
                });
            }
            return msg;
        }

        internal DbIndicatorVwapLite[] GetVwapGains(DateTime date, string exchange)
        {
            var tablePrefix = GetTableNamePrefix(date);
            var sql = String.Format(DbQueryStr.GetVwapGains, tablePrefix);

            using (var db = TradeDataConnection())
                return db.Query<DbIndicatorVwapLite>(sql, new { Exchange = exchange }).ToArray();
        }

        internal DbMessage GetTrades(string account, string instrument, DateTime startDate, DateTime endDate, int? count)
        {
            var msg = new DbMessage();
            var args = new { StartDate = startDate, EndDate = endDate, Instrument = instrument, AccountId = account };
            var sql = String.Format(DbQueryStr.GetBitmexBotTrades, count.HasValue ? "TOP " + count.Value.ToString() : "");

            using (var db = BotcoinConnection())
            {
                msg.Trades = db.Query<DbTrade>(sql, args).ToArray();
                msg.Positions = db.Query<DbPosition>(DbQueryStr.GetBitmexBotPositions, args).ToArray();

                var args2 = new { Instrument = instrument, AccountId = account };
                msg.OpenPosFee = db.Query<double?>(DbQueryStr.GetFeeFromLastPosition, args2).SingleOrDefault();
            }
            return msg;
        }

        internal void SaveBitmexTrade(TradeRequest req)
        {
            string[] names, argNames;
            IDbTransaction trans = null;

            using (var db = BotcoinConnection())
            {
                db.Open();

                trans = db.BeginTransaction();
                try
                {
                    var args = CreateBitmexTradeParams(req, out names, out argNames);
                    var sql = String.Format(DbQueryStr.CreateBitmexBotTrade, String.Join(",", names), String.Join(",", argNames));

                    db.Execute(sql, args, trans);
                    CreateBitmexStopPrices(db, req, trans);

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    WriteServiceExceptionEvent(db, ex, ServiceName.Arbitrage);
                }
            }
        }

        private void CreateBitmexStopPrices(IDbConnection db, TradeRequest req, IDbTransaction trans = null)
        {
            //var args = CreateBitmexStopPriceParams(req);
            //db.Execute(DbQueryStr.CreateBitmexStopPrices, args, trans);
        }

        internal BitmexInstrumentSettings[] GetBitmexInstruments(string accountId)
        {
            using (var db = BotcoinConnection())
            {
                var arg = new { AccountId = accountId };
                return db.Query<BitmexInstrumentSettings>(DbQueryStr.GetBitmexInstruments, arg).ToArray();
            }
        }

        internal void SaveBitmexInstruments(BitmexInstrumentSettings[] commissions, string accountId)
        {
            using (var db = BotcoinConnection())
            {
                TransactionAction(db, trans =>
                {
                    Action<BitmexInstrumentSettings> insert = c =>
                    {
                        var arg1 = new
                        {
                            Symbol        = c.Symbol,
                            AccountId     = accountId,
                            MakerFee      = c.MakerFee,
                            TakerFee      = c.TakerFee,
                            TickSize      = c.TickSize,
                            Timestamp     = c.Timestamp,
                            Index         = c.Index,
                            SettlementFee = c.SettlementFee
                        };
                        db.Execute(DbQueryStr.SaveBtxInstruments, arg1, trans);
                    };

                    var arg = new { AccountId = accountId };
                    var instruments = db.Query<BitmexInstrumentSettings>(DbQueryStr.GetBitmexInstruments, arg, trans).ToDictionary(k => k.Symbol, v => v);

                    foreach (var c in commissions)
                    {
                        if (instruments.ContainsKey(c.Symbol))
                        {
                            var inst = instruments[c.Symbol];

                            if (c.MakerFee.Value == inst.MakerFee.Value &&
                                c.TakerFee.Value == inst.TakerFee.Value &&
                                c.SettlementFee.Value == inst.SettlementFee.Value)
                            {
                                continue;
                            }
                            insert(c);
                        }
                        else
                            insert(c);
                    }
                });
            }
        }

        private DbPosition CreateDbPosition(BitmexTradeHistory trade, string positionId, string accountId, bool isOpen)
        {
            var pos = new DbPosition
            {
                TransactTime  = trade.TransactTime,
                PositionId    = positionId,
                AccountId     = accountId,
                Instrument    = trade.Symbol,
                OrderQty      = trade.OrderQty,
                IsOpen        = isOpen,
                Side          = trade.Side
            };
            if (trade.Commission.HasValue)
                pos.FeeRate = trade.Commission.Value;
            if (trade.ExecCost.HasValue)
                pos.ExecCost = trade.ExecCost.Value;
            if (trade.LastPx.HasValue)
                pos.Price = trade.LastPx.Value;

            return pos;
        }

        internal void SaveWallet(WalletRequest state)
        {
            string[] names, argNames;

            using (var db = BotcoinConnection())
            {
                var arg = new { AccountId = (long)state.Wallet.Account };
                var balance = db.Query<double?>(DbQueryStr.GetWalletForUpdate, arg).SingleOrDefault();

                if (balance.HasValue && state.Wallet.BalanceXBT == balance.Value)
                    return;

                var args = CreateWalletParams(state.Wallet, out names, out argNames);
                var sql = String.Format(DbQueryStr.CreateBitmexWallet, String.Join(",", names), String.Join(",", argNames));

                db.Execute(sql, args);
            }
        }

        internal void SaveMargin(MarginRequest state)
        {
            string[] names, argNames;

            using (var db = BotcoinConnection())
            {
                var arg       = new { AccountId = (long)state.Margin.Account };
                var dbBalance = db.Query<double?>(DbQueryStr.GetMarginForUpdate, arg).SingleOrDefault();
                var balance   = state.Margin.WalletBalance;

                if (dbBalance.HasValue && balance.HasValue && BitmexMargin.ToBtc(balance.Value) == dbBalance.Value)
                    return;

                var args = CreateMarginParams(state.Margin, out names, out argNames);
                var sql  = String.Format(DbQueryStr.CreateBitmexMargin, String.Join(",", names), String.Join(",", argNames));

                db.Execute(sql, args);
            }
        }

        internal ExchangeName ShouldRestartTradeDataBot(int limit)
        {
            var time = DateTime.UtcNow;
            var sql = String.Format(DbQueryStr.TradeDatabotHeartbeat, GetTableNamePrefix(time));

            using (var db = TradeDataConnection())
            {
                using (var result = db.QueryMultiple(sql))
                {
                    var timeGdax     = result.Read<DateTime?>().SingleOrDefault();
                    var timeBitstamp = result.Read<DateTime?>().SingleOrDefault();
                    var timeBinance  = result.Read<DateTime?>().SingleOrDefault();
                    var timeBitmex   = result.Read<DateTime?>().SingleOrDefault();

                    if (!timeGdax.HasValue) return ExchangeName.Gdax;
                    var ts = timeGdax.Value - time;
                    if (Math.Abs(ts.Minutes) > limit) return ExchangeName.Gdax;

                    if (!timeBitstamp.HasValue) return ExchangeName.Bitstamp;
                    ts = timeBitstamp.Value - time;
                    if (Math.Abs(ts.Minutes) > limit) return ExchangeName.Bitstamp;

                    if (!timeBinance.HasValue) return ExchangeName.Binance;
                    ts = timeBinance.Value - time;
                    if (Math.Abs(ts.Minutes) > limit) return ExchangeName.Binance;

                    if (!timeBitmex.HasValue) return ExchangeName.Bitmex;
                    ts = timeBitmex.Value - time;
                    if (Math.Abs(ts.Minutes) > limit) return ExchangeName.Bitmex;
                }
            }
            return ExchangeName.Undefined;
        }

        internal void SaveVwapIndicator(string exchange, List<string[]> instruments, List<Tuple<DateTime, DateTime>> dates, string periodName)
        {
            if (dates.Count == 0)
                throw new ArgumentException("Empty dates list.");

            string tablePrefix = GetTableNamePrefix(dates[0].Item1);
            using (var db = TradeDataConnection())
            {
                SaveVwap((SqlConnection)db, tablePrefix, instruments, dates, periodName, exchange);
            }
        }

        internal void SaveVwapRatio(List<string[]> pairList, string exchange, DateTime[] dates, string periodName)
        {
            if (dates.Length == 0)
                throw new ArgumentException("Empty dates list.");

            string tablePrefix = GetTableNamePrefix(dates[0]);
            using (var db = TradeDataConnection())
            {
                SaveVwapRatio((SqlConnection)db, tablePrefix, pairList, dates, periodName, exchange);
            }
        }

        private void SaveVwapRatio(SqlConnection db, string tablePrefix, List<string[]> instruments, DateTime[] dates, string periodName, string exchange)
        {
            TransactionAction(db, trans =>
            {
                var items = new List<DbIndicatorVwap>();
                var args = new { Exchange = exchange, TimePeriod = periodName, Time1 = dates.First(), Time2 = dates.Last() };
                var vwaps = db.Query<DbIndicatorVwap>(String.Format(DbQueryStr.GetVwapInstruments, tablePrefix), args, trans).ToArray();

                if (vwaps.Length == 0) return;

                foreach (var instrument in instruments)
                {
                    foreach (var date in dates)
                    {
                        var item1 = vwaps.Where(v => v.Instrument == instrument[0]).Where(v => v.Timestamp == date).SingleOrDefault();
                        if (item1 == null) continue;

                        var item2 = vwaps.Where(v => v.Instrument == instrument[1]).Where(v => v.Timestamp == date).SingleOrDefault();
                        if (item2 == null) continue;

                        items.Add(new DbIndicatorVwap
                        {
                            Exchange        = exchange,
                            TimePeriod      = periodName,
                            Timestamp       = date,
                            Instrument1     = item1.Instrument,
                            Instrument2     = item2.Instrument,
                            PriceVwapRatio1 = item1.VwapRatioPcnt,
                            PriceVwapRatio2 = item2.VwapRatioPcnt,
                            VwapGain15Min1  = item1.VwapGain15Min1,
                            VwapGain15Min2  = item2.VwapGain15Min1,
                            CumVwapGain15Min1 = item1.CumVwapGain15Min1,
                            CumVwapGain15Min2 = item2.CumVwapGain15Min1
                        });
                    }
                }
                if (items.Count > 0)
                    BulkInsert(db, trans, String.Format("[dbo].[{0}_IndicatorVwapRatios]", tablePrefix), () => CreateVwapRatioValues(items));
            });
        }

        private DataTable CreateBitmexTrades(BitmexTradeData[] items)
        {
            var table = new DataTable();

            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Timestamp", typeof(DateTime));
            table.Columns.Add("Instrument", typeof(string));
            table.Columns.Add("Side", typeof(string));
            table.Columns.Add("Price", typeof(double));
            table.Columns.Add("Size", typeof(long));
            table.Columns.Add("TickDirection", typeof(string));
            table.Columns.Add("GrossValue", typeof(double));

            foreach (var item in items)
            {
                DataRow row = table.NewRow();

                row["Timestamp"]    = item.Timestamp;
                row["Instrument"]   = item.Symbol;
                row["Side"]         = item.Side;
                row["Size"]         = item.Size;
                row["Price"]        = item.Price;
                row["TickDirection"] = ConvertTickDirection(item.TickDirection);
                row["GrossValue"]   = item.GrossValue;

                table.Rows.Add(row);
            }
            return table;
        }
#if VWAP_INDEX
        private bool CanNotAddInstrument(string symbol)
        {
            // ignore BTC futures
            return symbol.StartsWith("XBT") && Char.IsDigit(symbol[symbol.Length - 1]);
        }

        private bool PerpetualInstrument(string symbol)
        {
            return symbol == "XBTUSD" || symbol == "ETHUSD";
        }

        private bool AltcoinFuturesInstrument(string symbol)
        {
            if (symbol.StartsWith("XBT") && Char.IsDigit(symbol[symbol.Length - 1])) return false;
            if (symbol.StartsWith("ETH") && Char.IsDigit(symbol[symbol.Length - 1])) return false;

            return true;
        }

        private DataTable CreateVwapIndexValues(List<DbIndicatorVwap> items)
        {
            var initVwaps    = new Dictionary<string, double>();
            var table        = new DataTable();
            var idxs         = new List<double>();
            var swapIdxs     = new List<double>();
            var altcoinIdxs  = new List<double>();
            var exchange     = items.First().Exchange;
            var timePeriod   = items.First().TimePeriod;
            
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Exchange", typeof(string));
            table.Columns.Add("TimePeriod", typeof(string));
            table.Columns.Add("Timestamp", typeof(DateTime));
            table.Columns.Add("Instrument", typeof(string));
            table.Columns.Add("VwapGainPcnt", typeof(double));
            table.Columns.Add("VwapIndex", typeof(double));
            table.Columns.Add("SwapVwapIndex", typeof(double));
            table.Columns.Add("AltVwapIndex", typeof(double));

            if (items.Count == 0) return table;
            
            foreach (var itemByTime in items.GroupBy(i => i.Timestamp).Select(i => i).ToArray())
            {
                var itemsByPeriod = items.Where(i => i.Timestamp == itemByTime.Key).ToArray();
                DataRow row = null;

                for (int i = 0; i < itemsByPeriod.Length; i++)
                {
                    var item = itemsByPeriod[i];
                    if (CanNotAddInstrument(item.Instrument1)) continue;

                    row = table.NewRow();
                    
                    row["Exchange"]   = item.Exchange;
                    row["TimePeriod"] = item.TimePeriod;
                    row["Timestamp"]  = item.Timestamp;

                    //if (!initVwaps.ContainsKey(item.Instrument1))
                    //    initVwaps[item.Instrument1] = item.InitVwap;

                    //var initVwap = initVwaps[item.Instrument1];
                    //var gainPcnt = Math.Round(((item.Vwap - initVwap) / initVwap) * 100, 3);

                    row["Instrument"]    = item.Instrument1;
                    row["VwapGainPcnt"]  = DBNull.Value; //gainPcnt;
                    row["VwapIndex"]     = DBNull.Value;
                    row["SwapVwapIndex"] = DBNull.Value;
                    row["AltVwapIndex"]  = DBNull.Value;

                    table.Rows.Add(row);
                    //idxs.Add(gainPcnt);

                    //if (PerpetualInstrument(item.Instrument1))
                    //    swapIdxs.Add(gainPcnt);
                    //else if (AltcoinFuturesInstrument(item.Instrument1))
                    //    altcoinIdxs.Add(gainPcnt);
                }
                row = table.NewRow();
                
                row["Exchange"]     = exchange;
                row["TimePeriod"]   = timePeriod;
                row["Timestamp"]    = itemByTime.Key;
                row["Instrument"]   = DBNull.Value;
                row["VwapGainPcnt"] = DBNull.Value;

                if (idxs.Count == 0) row["VwapIndex"] = DBNull.Value;
                else row["VwapIndex"] = Math.Round(idxs.Average(), 3);

                if (swapIdxs.Count == 0) row["SwapVwapIndex"] = DBNull.Value;
                else row["SwapVwapIndex"] = Math.Round(swapIdxs.Average(), 3);

                if (altcoinIdxs.Count == 0) row["AltVwapIndex"] = DBNull.Value;
                else row["AltVwapIndex"] = Math.Round(altcoinIdxs.Average(), 3);

                idxs.Clear(); swapIdxs.Clear(); altcoinIdxs.Clear();
                table.Rows.Add(row);
            }            
            return table;
        }
#endif
        private void AddVwap(Dictionary<string, double> dict, string symbol, double value)
        {
            if (dict.ContainsKey(symbol)) dict.Remove(symbol);
            dict[symbol] = value;
        }

        private DataTable CreateVwapValues(List<DbIndicatorVwap> items, List<DbIndicatorVwap> vwapsInfo)
        {
            var cumVwapGains = new Dictionary<string, double>();
            var vwaps = new Dictionary<string, double>();
            var table = new DataTable();

            // in case of real-time
            if (items[0].Timestamp.Date == DateTime.UtcNow.Date)
            {
                foreach (var info in vwapsInfo)
                {
                    if (info == null) continue;
                     
                    AddVwap(cumVwapGains, info.Instrument, info.CumVwapGain15Min1.Value);
                    AddVwap(vwaps, info.Instrument, info.Vwap);
                }
            }
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Exchange", typeof(string));
            table.Columns.Add("TimePeriod", typeof(string));
            table.Columns.Add("Timestamp", typeof(DateTime));
            table.Columns.Add("Instrument", typeof(string));
            table.Columns.Add("SumVolume", typeof(double));
            table.Columns.Add("SumTypeVol", typeof(double));            
            table.Columns.Add("Vwap", typeof(double));
            table.Columns.Add("VwapRatioPcnt", typeof(double));
            table.Columns.Add("TradesCount", typeof(int));
            table.Columns.Add("TradesCountRatio", typeof(double));
            table.Columns.Add("TotalTradesCount", typeof(int));
            table.Columns.Add("TotalTradesRatio", typeof(double));
            table.Columns.Add("OpenPrice", typeof(double));
            table.Columns.Add("ClosePrice", typeof(double));
            table.Columns.Add("ExtremClosePrice", typeof(int));
            table.Columns.Add("VwapGain15Min", typeof(double));
            table.Columns.Add("CumVwapGain15Min", typeof(double));
            table.Columns.Add("VwapGainRatio15Min", typeof(double));

            foreach (var item in items)
            {
                DataRow row = table.NewRow();
#if VWAP_INDEX
                if (!initVwaps.ContainsKey(item.Instrument1))
                    initVwaps[item.Instrument1] = item.Vwap;
                item.InitVwap = initVwaps[item.Instrument1];
#endif                
                row["Exchange"]      = item.Exchange;
                row["TimePeriod"]    = item.TimePeriod;
                row["Timestamp"]     = item.Timestamp;
                row["Instrument"]    = item.Instrument1;
                row["SumVolume"]     = item.SumVolume;
                row["SumTypeVol"]    = item.SumTypeVol;
                row["Vwap"]          = item.Vwap;
                row["VwapRatioPcnt"] = Math.Round((item.ClosePrice / item.Vwap - 1) * 100, 3);
                row["TradesCount"]      = item.TradesCount;
                row["TradesCountRatio"] = item.TradesCountRatio;
                row["TotalTradesCount"] = item.TotalTradesCount;
                row["TotalTradesRatio"] = item.TotalTradesRatio;
                row["OpenPrice"]        = item.OpenPrice;
                row["ClosePrice"]       = item.ClosePrice;
                row["ExtremClosePrice"] = item.ExtremClosePrice;

                Set15MinVwapGainFor3MinPeriod(item, row, cumVwapGains, vwaps);
                table.Rows.Add(row);
            }
            return table;
        }

        private void Set15MinVwapGainFor3MinPeriod(DbIndicatorVwap item, DataRow row, Dictionary<string, double> cumVwapGains, Dictionary<string, double> vwaps)
        {
            if (item.TimePeriod == "3m")
            {
                var ts = item.Timestamp;                
                if (ts.Minute == 0 || ts.Minute == 12 || ts.Minute == 24 || ts.Minute == 36 || ts.Minute == 48 ||
                    //(ts.Minute == 0 || ts.Minute == 15 || ts.Minute == 30 || ts.Minute == 45 ||
                    (ts.Hour == 23 && ts.Minute == 57))
                {
                    var symbol = item.Instrument1;

                    if (!cumVwapGains.ContainsKey(symbol)) cumVwapGains[symbol] = 0;
                    if (!vwaps.ContainsKey(symbol))        vwaps[symbol]        = item.Vwap;
                    
                    var vwapGain    = 100 * ((item.Vwap - vwaps[symbol]) / item.Vwap);
                    var cumGain     = vwapGain + cumVwapGains[symbol];
                    var prevCumGain = cumVwapGains[symbol];
                    var gainRatio   = 10 * (cumGain - prevCumGain);

                    row["VwapGainRatio15Min"] = Math.Round(gainRatio, 4);
                    row["CumVwapGain15Min"]   = Math.Round(cumGain, 4);
                    row["VwapGain15Min"]      = Math.Round(vwapGain, 4);                    

                    AddVwap(cumVwapGains, symbol, cumGain);
                    AddVwap(vwaps, symbol, item.Vwap);
                }
                else
                {
                    row["CumVwapGain15Min"]   = DBNull.Value;
                    row["VwapGain15Min"]      = DBNull.Value;
                    row["VwapGainRatio15Min"] = DBNull.Value;
                }
            }
        }

        private DataTable CreateVwapRatioValues(List<DbIndicatorVwap> items)
        {
            var table = new DataTable();

            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Exchange", typeof(string));
            table.Columns.Add("TimePeriod", typeof(string));
            table.Columns.Add("Timestamp", typeof(DateTime));
            table.Columns.Add("Instrument1", typeof(string));
            table.Columns.Add("Instrument2", typeof(string));
            table.Columns.Add("Vwaps15MinSpread", typeof(double));
            table.Columns.Add("VwapGain15Min1", typeof(double));
            table.Columns.Add("VwapGain15Min2", typeof(double));
            table.Columns.Add("PriceVwapRatio1", typeof(double));
            table.Columns.Add("PriceVwapRatio2", typeof(double));
            table.Columns.Add("PriceVwapDiv", typeof(double));
            table.Columns.Add("CumVwapGain15Min1", typeof(double));
            table.Columns.Add("CumVwapGain15Min2", typeof(double));
            table.Columns.Add("CumVwapGainDiv", typeof(double));

            foreach (var item in items)
            {
                DataRow row = table.NewRow();

                row["Exchange"]        = item.Exchange;
                row["TimePeriod"]      = item.TimePeriod;
                row["Timestamp"]       = item.Timestamp;
                row["Instrument1"]     = item.Instrument1;
                row["Instrument2"]     = item.Instrument2;
                row["PriceVwapRatio1"] = item.PriceVwapRatio1.Value;
                row["PriceVwapRatio2"] = item.PriceVwapRatio2.Value;
                row["PriceVwapDiv"]    = Math.Round(item.GetVwapPriceSpread(), 4);

                double? divergence = item.GetCumVwapGainSpread();
                if (divergence.HasValue)
                {
                    row["CumVwapGain15Min1"] = item.CumVwapGain15Min1.Value;
                    row["CumVwapGain15Min2"] = item.CumVwapGain15Min2.Value;
                    row["CumVwapGainDiv"]    = Math.Round(divergence.Value, 4);
                }
                else
                {
                    row["CumVwapGain15Min1"] = DBNull.Value;
                    row["CumVwapGain15Min2"] = DBNull.Value;
                    row["CumVwapGainDiv"]    = DBNull.Value;
                }

                divergence = item.GetVwapsSpread();

                if (divergence.HasValue) row["Vwaps15MinSpread"] = Math.Round(divergence.Value, 4);
                else row["Vwaps15MinSpread"] = DBNull.Value;

                if (item.VwapGain15Min1.HasValue) row["VwapGain15Min1"] = item.VwapGain15Min1.Value;
                else row["VwapGain15Min1"] = DBNull.Value;

                if (item.VwapGain15Min2.HasValue) row["VwapGain15Min2"] = item.VwapGain15Min2.Value;
                else row["VwapGain15Min2"] = DBNull.Value;

                table.Rows.Add(row);
            }
            return table;
        }

        private DbIndicatorVwap GetLastVwap2(SqlConnection db, SqlTransaction trans, string instrument, string periodName, string tablePrefix, string exchange)
        {
            var args = new { Instrument = instrument, Exchange = exchange, Period = periodName };
            return db.Query<DbIndicatorVwap>(String.Format(DbQueryStr.GetLastVwap2, tablePrefix), args, trans).SingleOrDefault();
        }

        private void GetExtremClosePrice(SqlConnection db, SqlTransaction trans, string instrument, string periodName, string tablePrefix, string exchange, DateTime time, out double min, out double max)
        {
            var args = new { Instrument = instrument, Exchange = exchange, Period = periodName, Time1 = time.Date, Time2 = time };
            var obj = db.Query<DbIndicatorVwap>(String.Format(DbQueryStr.GetExtremClosePrice, tablePrefix), args, trans).SingleOrDefault();

            max = obj.HighPrice;
            min = obj.LowPrice;
        }

        private void GetExtremClosePrice(List<DbIndicatorVwap> items, out double highPrice, out double lowPrice)
        {
            highPrice = 0; lowPrice = Double.MaxValue;
            foreach (var item in items)
            {
                if (item.ClosePrice > highPrice) highPrice = item.ClosePrice;
                if (item.ClosePrice < lowPrice)  lowPrice = item.ClosePrice;
            }
        }

        private DbIndicatorVwap GetLastVwap(SqlConnection db, SqlTransaction trans, string instrument1, string instrument2, List<Tuple<DateTime, DateTime>> dates, string periodName, string tablePrefix, string exchange)
        {
            var args1 = new { Instrument = instrument1 + instrument2, Exchange = exchange, Period = periodName };
            var time  = dates.First().Item1;
            
            if (time.Hour == 0 && time.Minute == 0)
                return new DbIndicatorVwap { Timestamp = dates.Last().Item2, Instrument1 = args1.Instrument, Exchange = args1.Exchange, TimePeriod = periodName };

            var obj = db.Query<DbIndicatorVwap>(String.Format(DbQueryStr.GetLastVwap, tablePrefix), args1, trans).SingleOrDefault();
            if (obj == null)
                return new DbIndicatorVwap { Timestamp = time, Instrument1 = args1.Instrument, Exchange = args1.Exchange, TimePeriod = periodName };

            return obj;
        }

        private IDbTradeObject[] GetTrades(SqlConnection db, SqlTransaction trans, string instrument1, string instrument2, List<Tuple<DateTime, DateTime>> dates, string periodName, string tablePrefix, string exchange, out DbIndicatorVwap lastVwap)
        {
            lastVwap = GetLastVwap(db, trans, instrument1, instrument2, dates, periodName, tablePrefix, exchange);

            object args = null;
            string sql  = null;

            if (exchange == "Binance")
            {
                args = new { Instrument1 = instrument1, Instrument2 = instrument2, Time1 = dates.First().Item1, Time2 = dates.Last().Item2 };
                sql  = String.Format(DbQueryStr.GetBinanceCandlesForVwap, tablePrefix);

                var trades = db.Query<DbTradeBinance>(sql, args, trans).ToArray();
                if (trades.Length == 0 && instrument2 == "USD")
                {
                    args = new { Instrument1 = instrument1, Instrument2 = "USDT", Time1 = dates.First().Item1, Time2 = dates.Last().Item2 };
                    trades = db.Query<DbTradeBinance>(sql, args, trans).ToArray();
                }
                return trades;
            }
            else if (exchange == "Bitmex")
            {
                args = new { Time1 = dates.First().Item1, Time2 = dates.Last().Item2 };
                sql = String.Format(DbQueryStr.GetBitmexCandlesForVwap, tablePrefix, instrument1);

                return db.Query<DbTradeBitmex>(sql, args, trans).ToArray();
            }
            throw new InvalidOperationException();
        }

        private IDbTradeObject[] GetTradesForPeriod(IDbTradeObject[] allTrades, DateTime date1, DateTime date2, out double highPrice, out double lowPrice, out double totalSize)
        {
            var trades = new List<IDbTradeObject>();
            highPrice = 0; lowPrice = Double.MaxValue; totalSize = 0;

            foreach (var trade in allTrades)
            {
                var ts = trade.Timestamp;
                if (ts >= date1 && ts <= date2)
                {
                    if (trade.Price > highPrice) highPrice = trade.Price;
                    if (trade.Price < lowPrice) lowPrice = trade.Price;
                    totalSize += trade.Size;
                    trades.Add(trade);
                }
            }
            return trades.ToArray();
        }
                
        private void SaveVwap(SqlConnection db, string tablePrefix, List<string[]> instruments, List<Tuple<DateTime, DateTime>> dates, string periodName, string exchange)
        {            
            TransactionAction(db, trans =>
            {
                var extremPrices = new List<DbIndicatorVwap>();
                var vwapsInfo    = new List<DbIndicatorVwap>();
                var items        = new List<DbIndicatorVwap>();

                foreach (var inst in instruments)
                {
                    DbIndicatorVwap lastVwap;

                    var allTrades = GetTrades(db, trans, inst[0], inst[1], dates, periodName, tablePrefix, exchange, out lastVwap);
                    if (allTrades.Length == 0) continue;

                    vwapsInfo.Add(GetLastVwap2(db, trans, inst[0] + inst[1], periodName, tablePrefix, exchange));

                    foreach (var date in dates)
                    {
                        double highPrice, lowPrice, totalSize;

                        var trades = GetTradesForPeriod(allTrades, date.Item1, date.Item2, out highPrice, out lowPrice, out totalSize);
                        if (trades.Length == 0) continue;

                        var obj = new DbIndicatorVwap { HighPrice = highPrice, LowPrice = lowPrice };
                        if (obj.HighPrice == 0 || obj.LowPrice == 0) continue;

                        obj.OpenPrice = trades[0].Price;
                        obj.ClosePrice = trades[trades.Length - 1].Price;
                        if (obj.ClosePrice == 0) continue;

                        obj.SumTypeVol  = lastVwap.SumTypeVol + obj.TypicalPrice * totalSize;
                        obj.SumVolume   = lastVwap.SumVolume + totalSize;
                        obj.TimePeriod  = periodName;
                        obj.Timestamp   = date.Item1;
                        obj.Exchange    = exchange;                        
                        obj.Instrument1 = inst[0] + inst[1];
                        obj.Vwap        = Math.Round(obj.SumTypeVol / obj.SumVolume, 8);
                        obj.TradesCount = trades.Length;
                        obj.TotalTradesCount = lastVwap.TotalTradesCount + trades.Length;

                        if (dates.Count == 1)   // real-time
                        {
                            double minPrice, maxPrice;
                            GetExtremClosePrice(db, trans, inst[0] + inst[1], periodName, tablePrefix, exchange, date.Item1, out minPrice, out maxPrice);

                            if (maxPrice != 0 && obj.ClosePrice > Math.Round(maxPrice, 8)) obj.ExtremClosePrice = 1;
                            if (minPrice != 0 && obj.ClosePrice < Math.Round(minPrice, 8)) obj.ExtremClosePrice = -1;
                        }
                        else
                        {
                            if (extremPrices.Count > 0)
                            {
                                double maxPrice, minPrice;
                                GetExtremClosePrice(extremPrices, out maxPrice, out minPrice);
                                
                                if (obj.ClosePrice > maxPrice) obj.ExtremClosePrice = 1;
                                if (obj.ClosePrice < minPrice) obj.ExtremClosePrice = -1;
                            }
                            extremPrices.Add(obj);
                        }
                        items.Add(obj);
                        lastVwap = obj;
                    }
                    extremPrices.Clear();
                }
                foreach (var group in items.GroupBy(i => i.Timestamp).ToArray())
                {
                    var list = items.Where(i => i.Timestamp == group.Key);
                    var totalCount = (double)list.Sum(i => i.TotalTradesCount);
                    var tradesCount = (double)list.Sum(i => i.TradesCount);

                    foreach (var item in list)
                    {
                        item.TotalTradesRatio = Math.Round((item.TotalTradesCount / totalCount) * 100, 2);
                        item.TradesCountRatio = Math.Round((item.TradesCount / tradesCount) * 100, 2);
                    }
                }
                if (items.Count > 0)
                {
                    BulkInsert(db, trans, String.Format("[dbo].[{0}_IndicatorVwap]", tablePrefix), () => CreateVwapValues(items, vwapsInfo));
#if VWAP_INDEX
                    BulkInsert(db, trans, "[dbo].[VwapIndex]", () => CreateVwapIndexValues(items));
#endif
                }
            });
        }                

        internal DateTime? GetLastVwapTimePeriod(DateTime time, string periodName, string exchange)
        {
            using (var db = TradeDataConnection())
            {
                var sql = String.Format(DbQueryStr.GetLastVwapTimePeriod, GetTableNamePrefix(time));
                return db.Query<DateTime?>(sql, new { TimePeriod = periodName, Exchange = exchange }).SingleOrDefault();                
            }
        }
    }
}
