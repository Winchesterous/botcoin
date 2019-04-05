namespace BotCoin.DbGatewayService
{
    internal static class DbQueryStr
    {
        public static string GetLastBalances = @"
                SELECT TOP 1 Balance / CurrencyRate AS UsdBalance,CurrencyRate FROM Account WHERE ExchangeId = @Exchange ORDER BY Id DESC;
                SELECT BtcBalance FROM BtcAccount WHERE AccountId=(SELECT TOP 1 Id FROM Account WHERE ExchangeId = @Exchange ORDER BY Id DESC);
                SELECT BchBalance FROM BchAccount WHERE AccountId=(SELECT TOP 1 Id FROM Account WHERE ExchangeId = @Exchange ORDER BY Id DESC);
                SELECT LtcBalance FROM LtcAccount WHERE AccountId=(SELECT TOP 1 Id FROM Account WHERE ExchangeId = @Exchange ORDER BY Id DESC);
                SELECT EthBalance FROM EthAccount WHERE AccountId=(SELECT TOP 1 Id FROM Account WHERE ExchangeId = @Exchange ORDER BY Id DESC);
                SELECT TOP 1 Balance / CurrencyRate AS InitUsdBalance FROM Account WHERE OperationCode='SND' AND ExchangeId = @Exchange ORDER BY Id DESC;";

        public static string GetExchange = @"
                SELECT TradeFee,UsdMinValue,BalanceStepBtc,BalanceStepBch,BalanceStepLtc,BalanceStepEth 
                FROM Exchange WHERE Id = @ExchangeId";

        public static string GetCurrencyRates = @"
                SELECT TOP 1 Rate,RateDate 
                FROM CurrencyRate WHERE CurrencyId = @CurrencyId ORDER BY Id DESC";

        public static string GetLastEventSession = @"
                SELECT TOP 1 SessionId FROM dbo.ServiceEvent ORDER BY Id DESC";

        public static string CreateServiceEvent = @"
                INSERT INTO [dbo].[ServiceEvent](Timestamp,SessionId,ExchangeId,ServiceName,EventType,Message) 
                VALUES(@time,@sessionId,@exchangeId,@service,@event,@msg)";

        public static string GetProfitRatioRelation = @"
                SELECT CAST((SELECT COUNT(*) FROM dbo.Trade WHERE CurrentProfitRatio = 0) AS FLOAT);
                SELECT CAST((SELECT COUNT(*) FROM dbo.Trade WHERE CurrentProfitRatio <> 0) AS FLOAT);";

        public static string SaveMatchingData = @"
                INSERT INTO [dbo].[Matching](CreatedAt,ExchangeId,InstrumentId,BidPrice,AskPrice,BidAmount,BidOrderAmount,AskAmount,AskOrderAmount) VALUES
                (@CreatedAt,@ExchangeId,@InstrumentId,@BidPrice,@AskPrice,@BidAmount,@BidOrderAmount,@AskAmount,@AskOrderAmount)";

        public static string SaveFailReason = @"
                INSERT INTO dbo.FailTrade(TradeId, Reason) VALUES(@TableId, @FailReason)";

        public static string TradeExist = @"
                SELECT COUNT(*) FROM dbo.Trade WHERE ExchangeId1 = @Exchange1 AND ExchangeId2 = @Exchange2 
                AND CurrencyId = @Instrument AND AskPrice1 = @AskPrice1 AND BidPrice2 = @BidPrice2 AND Amount = @Amount";
    }
}
