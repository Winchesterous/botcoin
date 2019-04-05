using BotCoin.DataType;
using BotCoin.DataType.Database;
using BotCoin.DataType.RemoteCommand;
using Dapper;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BotCoin.DbGatewayService
{
    internal class Account
    {
        public int Id { set; get; }
        public int Balance { set; get; }
    }
    internal class InstrumentAccount
    {
        public double Balance { set; get; }
    }

    internal class DbRepository
    {
        readonly string _connectionString;

        public DbRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["Botcoin"].ConnectionString;
        }

        private IDbConnection DbConnection
        {
            get { return new SqlConnection(_connectionString); }
        }

        public void SaveTransaction(MatchingData data, Action<Exception> onException)
        {
            using (var db = DbConnection)
            {
                var count = db.Query<int>(DbQueryStr.TradeExist, data).Single();
                if (count > 0) return;

                db.Open();

                var args = SaveTradeProcParams(data);
                var trans = db.BeginTransaction();
                int tradeId = 0;
                int accountId = 0;

                try
                {
#if true
                    var sql = @"
                    insert into dbo.Trade (
 		                CreatedAt, ExchangeId1, ExchangeId2, OrderId1, OrderId2, ProfitRatio, Profit,
		                CurrentProfitRatio, AskAmount, BidAmount, Amount, BidPrice2, AskPrice1, CurrencyId, TransCode
	                )
	                values (
		                @createdAt, @exchange1, @exchange2, @order1, @order2, @profitRatio, @profit, @currentProfitRatio, 
		                @askAmount, @bidAmount, @amount, @bidPrice2, @askPrice1, @instrumentId, @transCode
	                );
                    select cast(scope_identity() as int)";

                    tradeId = db.Query<int>(sql, args, transaction: trans).Single();

                    if (data.TransactionState == TransactionState.Fail2 || data.TransactionState == TransactionState.OK)
                    {
                        var cryptoAmount = data.Amount;
                        var amount = -(data.BuyUsdAmount * data.CurrencyRate1);

                        sql = @"select top 1 Id, Balance from dbo.Account where ExchangeId = @ExchangeId order by Id desc";
                        var account = db.Query<Account>(sql, new { ExchangeId = data.Exchange1 }, transaction: trans).Single();
                        var balance = Math.Round(account.Balance + amount, 2);
                        if (balance < 0)
                        {
                        }
                        sql = @"insert into dbo.Account (ExchangeId, OperationCode, TradeId, CurrencyRate, Amount, Balance) 
		                        values (@exchange1, 'BUY', @tradeId, @currencyRate1, @amount, @balance);
                                select cast(scope_identity() as int)";
                        accountId = db.Query<int>(sql, SaveAccountProcParams1(data, tradeId, balance), transaction: trans).Single();

                        if (data.Instrument == CurrencyName.BTC)
                        {
                            InsertInstrument(db, trans, accountId, cryptoAmount, account.Id, "Btc");
                        }
                        else
                            throw new NotImplementedException();
                    }
                    if (data.TransactionState == TransactionState.Fail1 || data.TransactionState == TransactionState.OK)
                    {
                        var cryptoAmount = -data.Amount;
                        var amount = data.SellUsdAmount * data.CurrencyRate2;

                        sql = @"select top 1 Id, Balance from dbo.Account where ExchangeId = @ExchangeId order by Id desc";
                        var account = db.Query<Account>(sql, new { ExchangeId = data.Exchange2 }, transaction: trans).Single();
                        var balance = Math.Round(account.Balance + amount, 2);

                        sql = @"insert into dbo.Account (ExchangeId, OperationCode, TradeId, CurrencyRate, Amount, Balance) 
		                        values (@exchange2, 'SEL', @tradeId, @currencyRate2, @amount, @balance);
                                select cast(scope_identity() as int)";
                        accountId = db.Query<int>(sql, SaveAccountProcParams2(data, tradeId, balance), transaction: trans).Single();

                        if (data.Instrument == CurrencyName.BTC)
                        {
                            InsertInstrument(db, trans, accountId, cryptoAmount, account.Id, "Btc");
                        }
                        else
                            throw new NotImplementedException();
                    }
#else
                    db.Execute("SaveTrade", args, commandType: CommandType.StoredProcedure, transaction: trans);
                    tradeId = args.Get<int>("@tradeId");

                    args = SaveAccountProcParams(data, tradeId);
                    db.Execute("SaveAccount", args, commandType: CommandType.StoredProcedure, transaction: trans);
#endif
                    if (!String.IsNullOrEmpty(data.FailReason))
                    {
                        db.Execute(DbQueryStr.SaveFailReason, data, transaction: trans);
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    onException(ex);
                }
            }
        }

        private void InsertInstrument(IDbConnection db, IDbTransaction trans, int accountId, double amount, int currentAccountId, string instrument)
        {
            var sql = String.Format(@"select top 1 {0}Balance as Balance from dbo.{0}Account where AccountId = @AccountId", instrument);
            var res = db.Query<InstrumentAccount>(sql, new { AccountId = currentAccountId }, transaction: trans).Single();
            var balance = Math.Round(res.Balance + amount, 8);
            if (balance < 0)
            {
            }
            db.Execute(String.Format("INSERT INTO dbo.{0}Account(AccountId, Amount, {0}Balance) VALUES(@AccountId, @Amount, @Balance)", instrument),
                       new { AccountId = accountId, Amount = amount, Balance = balance },
                       transaction: trans);
        }

        private DynamicParameters SaveTradeProcParams(MatchingData data)
        {
            var args = new DynamicParameters();

            args.Add("@createdAt", data.CreatedAt);
            args.Add("@exchange1", (int)data.Exchange1);
            args.Add("@exchange2", (int)data.Exchange2);
            args.Add("@order1", data.Order1);
            args.Add("@order2", data.Order2);
            args.Add("@profitRatio", data.ProfitRatio);
            args.Add("@currentProfitRatio", data.CurrentProfitRatio);
            args.Add("@profit", data.Profit);
            args.Add("@amount", data.Amount);
            args.Add("@askAmount", data.AskAmount);
            args.Add("@bidAmount", data.BidAmount);
            args.Add("@askPrice1", data.AskPrice1);
            args.Add("@bidPrice2", data.BidPrice2);
            args.Add("@instrumentId", (int)data.Instrument);
            args.Add("@transCode", data.TransactionState.ToString());
            
            return args;
        }

        private DynamicParameters SaveAccountProcParams1(MatchingData data, int tradeId, double balance)
        {
            var args = new DynamicParameters();

            args.Add("@tradeId", tradeId);
            args.Add("@exchange1", (int)data.Exchange1);
            args.Add("@currencyRate1", data.CurrencyRate1);
            args.Add("@amount", data.Amount);
            args.Add("@balance", balance);

            return args;
        }

        private DynamicParameters SaveAccountProcParams2(MatchingData data, int tradeId, double balance)
        {
            var args = new DynamicParameters();

            args.Add("@tradeId", tradeId);
            args.Add("@exchange2", (int)data.Exchange2);
            args.Add("@currencyRate2", data.CurrencyRate2);
            args.Add("@amount", data.Amount);
            args.Add("@balance", balance);

            return args;
        }

        private DynamicParameters SaveAccountProcParams(MatchingData data, int tradeId)
        {
            var args = new DynamicParameters();

            args.Add("@tradeId", tradeId);
            args.Add("@exchange1", (int)data.Exchange1);
            args.Add("@exchange2", (int)data.Exchange2);
            args.Add("@currencyRate1", data.CurrencyRate1);
            args.Add("@currencyRate2", data.CurrencyRate2);
            args.Add("@amount", data.Amount);
            args.Add("@buyUsdAmount", data.BuyUsdAmount);
            args.Add("@sellUsdAmount", data.SellUsdAmount);
            args.Add("@transCode", data.TransactionState.ToString());
            args.Add("@currencyCode", data.Instrument.ToString());

            return args;
        }

        public DbAccount GetLastBalances(IDbConnection db, ExchangeName ex)
        {
            using (var result = db.QueryMultiple(DbQueryStr.GetLastBalances, new { Exchange = (int)ex }))
            {
                var query1 = result.Read<DbAccount>().Single();
                var query2 = result.Read<DbAccount>().SingleOrDefault();
                var query3 = result.Read<DbAccount>().SingleOrDefault();
                var query4 = result.Read<DbAccount>().SingleOrDefault();
                var query5 = result.Read<DbAccount>().SingleOrDefault();
                var query6 = result.Read<DbAccount>().Single();
                var account = new DbAccount
                {
                    CurrencyRate = query1.CurrencyRate,
                    InitUsdBalance = query6.InitUsdBalance,
                    UsdBalance = query1.UsdBalance,
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

                return account;
            }
        }

        public DbSyncBalancesCommand SyncBalances(DbSyncBalancesCommand cmd)
        {
            using (var db = DbConnection)
            {
                cmd.Account1 = GetLastBalances(db, cmd.Data.Exchange1);
                cmd.Account2 = GetLastBalances(db, cmd.Data.Exchange2);
            }
            return cmd;
        }

        public DbAccount GetLastBalances(ExchangeName ex)
        {
            using (var db = DbConnection)
                return GetLastBalances(db, ex);
        }

        public int GetServiceEventSession()
        {
            using (var db = DbConnection)
            {
                var id = db.Query<int?>(DbQueryStr.GetLastEventSession).SingleOrDefault();
                return id.HasValue ? id.Value + 1 : 1;
            }
        }

        public async void WriteServiceEvent(ServiceEventData data)
        {
            using (var db = DbConnection)
            {
                await db.ExecuteAsync(DbQueryStr.CreateServiceEvent, new
                {
                    @time = data.Timestamp,
                    @sessionId = data.SessionId,
                    @exchangeId = data.ExchangeId,
                    @service = data.ServiceName.ToString(),
                    @event = data.EventType.ToString(),
                    @msg = data.Message
                });
            }
        }

        public double GetCurrencyRate(CurrencyName currency)
        {
            using (var db = DbConnection)
            {
                var res = db.Query<DbCurrencyRate>(DbQueryStr.GetCurrencyRates, new { CurrencyId = (int)currency }).Single();
                return res.Rate;
            }
        }

        public DbExchange GetExchangeInfo(ExchangeName ex)
        {
            using (var db = DbConnection)
                return db.Query<DbExchange>(DbQueryStr.GetExchange, new { ExchangeId = (int)ex }).Single();
        }

        public Tuple<bool, DbAccount[]> CanResetBalances(ExchangeName[] exchanges)
        {
            using (var db = DbConnection)
            {
                bool canReset = false;

                using (var result = db.QueryMultiple(DbQueryStr.GetProfitRatioRelation))
                {
                    var query1 = result.Read<float>().Single();
                    var query2 = result.Read<float>().Single();
                    if (query2 != 0)
                        canReset = query1 / query2 > 3.0;
                }

                var accounts = new DbAccount[exchanges.Length];
                for (int i = 0; i < exchanges.Length; i++)
                    accounts[i] = GetLastBalances(db, exchanges[i]);

                return new Tuple<bool, DbAccount[]>(canReset, accounts.ToArray());
            }
        }

        public void SaveOrderBook(ExchangePricesEventArgs args)
        {
            using (var db = DbConnection)
            {
                var data = SaveOrderBookProcParams(args);
                db.Execute(DbQueryStr.SaveMatchingData, data);
            }
        }

        private DynamicParameters SaveOrderBookProcParams(ExchangePricesEventArgs data)
        {
            var args = new DynamicParameters();
            args.Add("@CreatedAt", data.CreatedAt);
            args.Add("@InstrumentId", (int)data.Instrument);
            args.Add("@ExchangeId", (int)data.Exchange);

            switch (data.Instrument)
            {
            case CurrencyName.BTC:
                {
                    args.Add("@BidPrice", data.BtcPrice[0]);
                    args.Add("@AskPrice", data.BtcPrice[1]);
                    args.Add("@BidAmount", data.BtcAmount[0]);
                    args.Add("@BidOrderAmount", data.BtcAmount[2]);
                    args.Add("@AskAmount", data.BtcAmount[1]);
                    args.Add("@AskOrderAmount", data.BtcAmount[3]);
                    break;
                }
            case CurrencyName.BCH:
                {
                    args.Add("@BidPrice", data.BchPrice[0]);
                    args.Add("@AskPrice", data.BchPrice[1]);
                    args.Add("@BidAmount", data.BchAmount[0]);
                    args.Add("@BidOrderAmount", data.BchAmount[2]);
                    args.Add("@AskAmount", data.BchAmount[1]);
                    args.Add("@AskOrderAmount", data.BchAmount[3]);
                    break;
                }
            case CurrencyName.LTC:
                {
                    args.Add("@BidPrice", data.LtcPrice[0]);
                    args.Add("@AskPrice", data.LtcPrice[1]);
                    args.Add("@BidAmount", data.LtcAmount[0]);
                    args.Add("@BidOrderAmount", data.LtcAmount[2]);
                    args.Add("@AskAmount", data.LtcAmount[1]);
                    args.Add("@AskOrderAmount", data.LtcAmount[3]);
                    break;
                }
            case CurrencyName.ETH:
                {
                    args.Add("@BidPrice", data.EthPrice[0]);
                    args.Add("@AskPrice", data.EthPrice[1]);
                    args.Add("@BidAmount", data.EthAmount[0]);
                    args.Add("@BidOrderAmount", data.EthAmount[2]);
                    args.Add("@AskAmount", data.EthAmount[1]);
                    args.Add("@AskOrderAmount", data.EthAmount[3]);
                    break;
                }
            }
            return args;
        }
    }
}
