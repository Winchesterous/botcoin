namespace BotCoin.Service
{
    internal static class DbQueryStr
    {
        public static string GetLastBalances = @"
                SELECT TOP 1 Balance, CurrencyRate FROM dbo.Account WHERE ExchangeId = @Exchange ORDER BY Id DESC;
                SELECT BtcBalance FROM BtcAccount WHERE AccountId=(SELECT TOP 1 Id FROM dbo.Account WHERE ExchangeId = @Exchange ORDER BY Id DESC);
                SELECT BchBalance FROM BchAccount WHERE AccountId=(SELECT TOP 1 Id FROM dbo.Account WHERE ExchangeId = @Exchange ORDER BY Id DESC);
                SELECT LtcBalance FROM LtcAccount WHERE AccountId=(SELECT TOP 1 Id FROM dbo.Account WHERE ExchangeId = @Exchange ORDER BY Id DESC);
                SELECT EthBalance FROM EthAccount WHERE AccountId=(SELECT TOP 1 Id FROM dbo.Account WHERE ExchangeId = @Exchange ORDER BY Id DESC);
                SELECT XrpBalance FROM XrpAccount WHERE AccountId=(SELECT TOP 1 Id FROM dbo.Account WHERE ExchangeId = @Exchange ORDER BY Id DESC);
                SELECT DashBalance FROM DashAccount WHERE AccountId=(SELECT TOP 1 Id FROM dbo.Account WHERE ExchangeId = @Exchange ORDER BY Id DESC);
                SELECT TOP 1 Balance / CurrencyRate AS InitUsdBalance FROM dbo.Account WHERE OperationCode='SYN' AND ExchangeId = @Exchange ORDER BY Id DESC;";

        public static string GetExchange = @"
                SELECT TradeFee,UsdMinValue FROM dbo.Exchange WHERE Id = @ExchangeId";

        public static string GetCurrencyRates = @"
                SELECT TOP 1 Rate,RateDate FROM dbo.CurrencyRate WHERE CurrencyId = @CurrencyId ORDER BY Id DESC";

        public static string GetLastEventSession = @"
                SELECT MAX(SessionId) 'SessionId' FROM dbo.EventLog";

        public static string CreateEventLog = @"
                INSERT INTO dbo.EventLog(Timestamp,SessionId,ExchangeId,ServiceName,EventType,Message) 
                VALUES(@time,@sessionId,@exchangeId,@service,@event,@msg)";

        public static string LastCurrencyUpdate = @"
                SELECT TOP 1 RateDate FROM dbo.CurrencyRate GROUP BY RateDate ORDER BY RateDate DESC";

        public static string LastAccount = @"
                SELECT TOP 1 CreatedAt FROM dbo.Account WHERE ExchangeId = @ExchangeId AND OperationCode='SYN' 
                GROUP BY CreatedAt ORDER BY CreatedAt DESC";

        public static string SyncAccount = @"
                DECLARE @Rate money; SELECT @Rate=Rate FROM dbo.CurrencyRate WHERE CurrencyId = 
                (SELECT CurrencyId FROM dbo.Exchange WHERE Id = @ExchangeId); IF @Rate IS NULL SET @Rate = 1;
                INSERT INTO dbo.Account(CreatedAt,ExchangeId,OperationCode,CurrencyRate,Balance,Amount)
                VALUES(@CreatedAt,@ExchangeId,'SYN',@Rate,@Balance / @Rate,0);
                SELECT CAST(SCOPE_IDENTITY() AS int)";

        public static string SyncAccountTemplate = @"
                INSERT INTO dbo.{0}Account(AccountId,{0}Balance,Amount) VALUES(@AccountId,@{0}Balance,0)";

        public static string CreateCurrencyRate = @"
                INSERT INTO dbo.CurrencyRate(CurrencyId,Rate,RateDate) VALUES(@Currency,@Rate,@ExchangeDate)";

        public static string CreateTradeTemplate = @"
                INSERT INTO dbo.[{0}](CreatedAt,TradeAt,Instrument1,Instrument2,TradeId,Price,Quantity,TradeType,Volume) 
                VALUES (@CreatedAt,@TradeAt,@Instrument1,@Instrument2,@TradeId,@Price,@Quantity,@TradeType,@Volume);
                SELECT CAST(SCOPE_IDENTITY() AS int)";

        public static string CreateBinanceTrade = @"
                INSERT INTO dbo.[{0}_TradeBinance](CreatedAt,TradeAt,Instrument1,Instrument2,TradeId,Price,Quantity,BuyerOrderId,SellerOrderId,Volume,IsBuyerMMaker)
                VALUES (@CreatedAt,@TradeAt,@Instrument1,@Instrument2,@TradeId,@Price,@Quantity,@BuyerOrderId,@SellerOrderId,@Volume,@IsBuyerMMaker);
                SELECT CAST(SCOPE_IDENTITY() AS int)";

        public static string CreateBinanceTicker = @"
                INSERT INTO dbo.[{0}_TickerBinance](CreatedAt,Instrument1,Instrument2,Vwap,PriceChange,PriceChangePcnt,HighPrice,OpenPrice,LowPrice,QtyClose,BestBidPrice,BestAskPrice,BestBidQty,BestAskQty,ClosePricePrevDay,ClosePriceCurrDay,TotalBaseVolume,TotalQuoteVolume,OpenTimeStats,CloseTimeStats)
                VALUES (@CreatedAt,@Instrument1,@Instrument2,@Vwap,@PriceChange,@PriceChangePcnt,@HighPrice,@OpenPrice,@LowPrice,@QtyClose,@BestBidPrice,@BestAskPrice,@BestBidQty,@BestAskQty,@ClosePricePrevDay,@ClosePriceCurrDay,@TotalBaseVolume,@TotalQuoteVolume,@OpenTimeStats,@CloseTimeStats);
                SELECT CAST(SCOPE_IDENTITY() AS int)";

        public static string CreateBitstampTicker = @"
                INSERT INTO dbo.[{0}_TickerBitstamp](CreatedAt,Instrument1,Instrument2,Vwap,HighPrice,OpenPrice,LowPrice,BestBidPrice,BestAskPrice,Volume)
                VALUES (@CreatedAt,@Instrument1,@Instrument2,@Vwap,@HighPrice,@OpenPrice,@LowPrice,@BestBidPrice,@BestAskPrice,@Volume);
                SELECT CAST(SCOPE_IDENTITY() AS int)";

        public static string CreateBitmexTicker = @"
                INSERT INTO dbo.[{0}_TickerBitmex]({1}) VALUES ({2});
                SELECT CAST(SCOPE_IDENTITY() AS int)";

        public static string CreateBitmexInstrument = @"
                INSERT INTO dbo.[{0}_BitmexInstrument]({1}) VALUES ({2});
                SELECT CAST(SCOPE_IDENTITY() AS int)";

        public static string CreateBitmexFunding = @"
                INSERT INTO dbo.[{0}_BitmexFunding]({1}) VALUES ({2});
                SELECT CAST(SCOPE_IDENTITY() AS int)";

        public static string CreateGdaxTicker = @"
                INSERT INTO dbo.[{0}_TickerGdax]({1}) VALUES ({2});
                SELECT CAST(SCOPE_IDENTITY() AS int)";

        public static string CreateOrderBookTemplate = @"
                INSERT INTO dbo.[{0}]({1}CreatedAt,Instrument1,Instrument2,BidPrice,AskPrice,Spread,SumBid,SumAsk,AvgBid,AvgAsk,MinBid,MinAsk,MaxBid,MaxAsk) 
                VALUES ({2}@CreatedAt,@Instrument1,@Instrument2,@BidPrice,@AskPrice,@Spread,@SumBid,@SumAsk,@AvgBid,@AvgAsk,@MinBid,@MinAsk,@MaxBid,@MaxAsk);
                SELECT CAST(SCOPE_IDENTITY() AS int)";

        public static string CreateGdaxOrderBook = @"
                INSERT INTO dbo.[{0}]({1}) VALUES ({2});
                SELECT CAST(SCOPE_IDENTITY() AS int)";

        public static string CreatePusherOrderBookTemplate = @"
                INSERT INTO dbo.[{0}]({1}CreatedAt,Instrument1,Instrument2,BidPrice,AskPrice,BidAmount,AskAmount) 
                VALUES ({2}@CreatedAt,@Instrument1,@Instrument2,@BidPrice,@AskPrice,@BidAmount,@AskAmount);
                SELECT CAST(SCOPE_IDENTITY() AS int)";

        public static string CreateBittrexArbitrage = @"
                INSERT INTO dbo.BittrexArbitrage
                (CreatedAt,Instrument1,Instrument2,Exchange1,Exchange2,SellPrice,BuyPrice,BittrexRatio,ProfitRatio,Ratio,Fees,ProfitUsd)
                VALUES (@CreatedAt,@Instrument1,@Instrument2,@Exchange1,@Exchange2,@SellPrice,@BuyPrice,@BittrexRatio,@ProfitRatio,@Ratio,@Fees,@ProfitUsd)";

        public static string GetExchangeSettings = @"
                SELECT e.Name as ExchangeName,c.Code as CurrencyName,'https://'+RestUrl as RestUrl,WebsocketUrl,PublicKey,SecretKey,ClientId,PusherKey,CertificateName,TradeFee,MakerFee
                FROM dbo.Exchange e JOIN dbo.ExchangeApiKey k ON e.Id=k.ExchangeId
                JOIN Currency c ON c.Id=e.CurrencyId";

        public static string CreateSpread = @"
                INSERT INTO dbo.Spread(CreatedAt,Instrument,Exchange1,Exchange2,SpreadPercent,BidAltcUsd,AskAltcUsd,BidAltcInstr2,AskAltcInstr2,BidUsd1,AskUsd1,BidUsd2,AskUsd2) 
                VALUES(@CreatedAt,@Instrument,@Exchange1,@Exchange2,@SpreadPercent,@BidAltcUsd,@AskAltcUsd,@BidAltcInstr2,@AskAltcInstr2,@BidUsd1,@AskUsd1,@BidUsd2,@AskUsd2)";

        public static string CreateInstrumentSpread = @"
                INSERT INTO dbo.InstrumentSpread(CreatedAt,Instrument1,Instrument2,Exchange1,Exchange2,Spread1,Spread2,Bid1,Ask1,Bid2,Ask2) 
                VALUES(@CreatedAt,@Instrument1,@Instrument2,@Exchange1,@Exchange2,@Spread1,@Spread2,@Bid1,@Ask1,@Bid2,@Ask2);";

        public static string GetBitstampCandleTemplate = @"
                SELECT TradeAt AS 'DateTime',Price FROM dbo.[{0}_TradeBitstamp] 
                WHERE TradeAt BETWEEN @Time1 and @Time2 AND Instrument1=@Instrument AND Instrument2='USD'";

        public static string GetBitstampTickerTemplate = @"
                SELECT CreatedAt 'Time',Vwap FROM dbo.[{0}_TickerBitstamp] WHERE Instrument1='BTC' AND Instrument2='USD' AND
                CreatedAt BETWEEN @Time1 AND @Time2";

        public static string GetBitstampTradesTemplate = @"
                SELECT TradeType,Quantity,Volume FROM dbo.[{0}_TradeBitstamp] WHERE TradeAt BETWEEN @TradeAt1 AND @TradeAt2";

        public static string GetBitstampOrdersTemplate = @"
                SELECT Price,Quantity FROM dbo.[{0}_OrderBookBitstamp] WHERE IsBuyOrder=1 ORDER BY Id;
                SELECT Price,Quantity FROM dbo.[{0}_OrderBookBitstamp] WHERE IsBuyOrder=0 ORDER BY Id";

        public static string NewDatePriceLevel = @"
                INSERT INTO dbo.PriceLevelDate(LevelId,LevelDate) VALUES (@LevelId,@LevelDate)";

        public static string RestorePriceLevel = @"
                UPDATE dbo.PriceLevel SET IsActual=1 WHERE LevelId = @LevelId";

        public static string CreatePriceLevel = @"
                INSERT INTO dbo.PriceLevel(LevelId,Date2,Price,IsLevelUp,TimeFrame,IsActual) VALUES (@LevelId,@Date2,@Price,@IsLevelUp,@TimeFrame,1)";

        public static string GetPriceLevelsTemplate = @"
                SELECT p1.LevelId AS Id,Price,IsActual,LevelDate,Date2 FROM dbo.PriceLevel p1 JOIN dbo.PriceLevelDate p2 
                ON p1.LevelId=p2.LevelId WHERE LevelDate IS NOT NULL";

        public static string GetPriceLevelById = @"
                SELECT Price,LevelDate AS Date2 FROM dbo.PriceLevel p1 JOIN dbo.PriceLevelDate p2 
                ON p1.LevelId=p2.LevelId WHERE p1.LevelId=@LevelId AND LevelDate IS NOT NULL";

        public static string UpdatePriceLevel = @"
                INSERT INTO dbo.PriceLevelDate(LevelId,LevelDate) VALUES (@LevelId,@LevelDate)";

        public static string CreateBreakdown = @"
                INSERT INTO dbo.PriceLevelDate(LevelId,IsFalseBreakdown,BreakdownDate) VALUES (@LevelId,@Flag,@Date)";

        public static string RemovePriceLevel = @"
                DELETE FROM dbo.PriceLevelDate WHERE LevelId=@LevelId;
                DELETE FROM dbo.PriceLevel WHERE LevelId=@LevelId";

        public static string CreateScalperEvent = @"
                INSERT INTO dbo.ScalperEvent(SessionId,Timestamp,EventType,Message)
                VALUES(@SessionId,@Timestamp,@EventType,@Message)";

        public static string GetBitmexLastPositionId = @"
                SELECT TOP 1 PositionId FROM dbo.BtxPosition 
                WHERE AccountId=@AccountId AND Instrument=@Instrument ORDER BY Id DESC";

        public static string GetBitmexBotTradeById = @"
                SELECT StartTime,StopValue FROM dbo.BtxTrade WHERE PositionId=@PositionId";

        public static string GetBitmexBotTrades = @"
                SELECT {0} * FROM dbo.BtxTrade WHERE Instrument=@Instrument AND AccountId=@AccountId 
                    AND StartTime BETWEEN @StartDate AND @EndDate AND EndTime IS NOT NULL";
                
        public static string GetBitmexBotPositions = @"
                SELECT * FROM dbo.BtxPosition WHERE Instrument=@Instrument AND AccountId=@AccountId 
                    AND TransactTime BETWEEN @StartDate AND @EndDate";

        public static string CreateBitmexPosition = @"
                INSERT INTO dbo.BtxPosition({0}) VALUES({1});
                SELECT CAST(SCOPE_IDENTITY() AS int)";

        public static string UpdateBitmexPosition = @"
                UPDATE dbo.BtxPosition SET Balance=@Balance,OrdText='END' WHERE Id=@Id";

        public static string CreateBitmexOrder = @"
                INSERT INTO dbo.BtxOrder({0}) VALUES({1})";

        public static string CreateBitmexBotTrade = @"
                INSERT INTO dbo.BtxTrade({0}) VALUES({1})";

        public static string CreateBitmexStopPrices = @"
                INSERT INTO dbo.BtxStopOrder(PositionId,Timestamp,StopPrice,StartWatchPrice) 
                VALUES(@PositionId,@Timestamp,@StopPrice,@StartWatchPrice)";

        public static string GetBalance = @"
                SELECT TOP 1 WalletBalance FROM dbo.BtxMargin WHERE AccountId='{0}' AND WalletBalance IS NOT NULL ORDER BY Id DESC";

        public static string GetPositionBalance = @"
                SELECT Balance FROM dbo.BtxPosition WHERE PositionId='{0}' AND Balance IS NOT NULL";

        public static string UpdateBitmexBotTrade = @"
                UPDATE dbo.BtxTrade SET OrderQty=@OrderQty,EndTime=@EndTime,ClosePrice=@ClosePrice,PriceGain=@PriceGain,
                    FeePaidXBT=@FeePaidXBT,RealisedPnlXBT=@RealisedPnlXBT,TakeStopRatio=@TakeStopRatio,ElapsedTime=@ElapsedTime
                WHERE PositionId=@PositionId;
                SELECT * FROM dbo.BtxTrade WHERE PositionId=@PositionId";

        public static string GetPositionSide = @"
                SELECT Side FROM dbo.BtxPosition WHERE PositionId='{0}' AND OrdText='OPN'";

        public static string GetPositionTotalQty = @"
                SELECT SUM(OrderQty) FROM dbo.BtxPosition WHERE PositionId='{0}' AND OrderId<>'00000000-0000-0000-0000-000000000000'";

        public static string CalculatePostionVwap = @"
                SELECT SUM(OrderQty) FROM dbo.BtxPosition WHERE PositionId='{0}' 
                    AND OrderId<>'00000000-0000-0000-0000-000000000000'
                    AND Side=(SELECT Side FROM dbo.BtxPosition WHERE PositionId='{0}' AND OrdText='OPN');
                SELECT SUM(FeePaidXBT)'Fees' FROM dbo.BtxPosition WHERE PositionId='{0}';
                EXEC dbo.GetPositionOpenPrice @positionId='{0}';
                EXEC dbo.GetPositionClosePrice @positionId='{0}'";

        public static string GetBitmexOrderForUpdate = @"
                SELECT TOP 1 OrdStatus,OrdType,OrdSide,OrderQty,Price,StopPrice 
                FROM dbo.BtxOrder WHERE OrderId=@OrderId";

        public static string GetTradeTableNames = @"
                SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE CHAR(ASCII(TABLE_NAME)) BETWEEN '1' AND '9' ORDER BY TABLE_NAME";

        public static string GetWalletForUpdate = @"
                SELECT TOP 1 Balance FROM dbo.BtxWallet WHERE AccountId=@AccountId ORDER BY Id DESC";

        public static string GetMarginForUpdate = @"
                SELECT TOP 1 WalletBalance FROM dbo.BtxMargin WHERE AccountId=@AccountId ORDER BY Id DESC";

        public static string CreateBitmexWallet = @"
                INSERT INTO dbo.BtxWallet({0}) VALUES({1})";

        public static string CreateBitmexMargin = @"
                INSERT INTO dbo.BtxMargin({0}) VALUES({1})";

        public static string GetBitmexInstruments = @"
                SELECT Symbol,MakerFee,TakerFee,SettlementFee,TickSize,[Index] FROM dbo.BtxInstrument WHERE AccountId=@AccountId";

        public static string SaveBtxInstruments = @"
                INSERT INTO dbo.BtxInstrument (Symbol,Timestamp,AccountId,MakerFee,TakerFee,SettlementFee,TickSize,[Index])
                VALUES (@Symbol,@Timestamp,@AccountId,@MakerFee,@TakerFee,@SettlementFee,@TickSize,@Index)";

        public static string GetFeeFromLastPosition = @"
                SELECT TOP 1 FeeRate FROM dbo.BtxPosition WHERE AccountId=@AccountId AND Instrument=@Instrument ORDER BY Id DESC";

        public static string SaveBtxScalperState = @"
                INSERT INTO dbo.BtxScalperState({0}) VALUES({1})";

        public static string GetBtxScalperState = @"
                SELECT TOP 1 * FROM dbo.BtxScalperState WHERE AccountId=@AccountId AND Instrument=@Instrument ORDER BY Id DESC";

        public static string UpdateBtxScalperState = @"
                UPDATE dbo.BtxScalperState SET {0} WHERE Id=@Id";

        public static string TradeDatabotHeartbeat = @"
                SELECT TOP 1 CreatedAt FROM dbo.[{0}_OrderBookGdax] ORDER BY Id DESC;
                SELECT TOP 1 CreatedAt FROM dbo.[{0}_OrderBookBitstamp] ORDER BY Id DESC;
                SELECT TOP 1 CreatedAt FROM dbo.[{0}_OrderBookBinance] ORDER BY Id DESC;
                SELECT TOP 1 Timestamp FROM dbo.[{0}_TickerBitmex] ORDER BY Id DESC";

        public static string CreateBitmexLiquidation = @"
                INSERT INTO dbo.[{0}_BitmexLiquidation](Timestamp,OrderId,Symbol,Side,Price,LeavesQty)
                VALUES (@Timestamp,@OrderId,@Symbol,@Side,@Price,@LeavesQty)";

        public static string GetLastVwap2 = @"
                SELECT TOP 1 Timestamp,Instrument,Vwap,VwapGain15Min AS 'VwapGain15Min1', CumVwapGain15Min AS 'CumVwapGain15Min1' FROM dbo.[{0}_IndicatorVwap] 
                WHERE TimePeriod=@Period AND Instrument=@Instrument AND Exchange=@Exchange AND VwapGain15Min IS NOT NULL
                ORDER BY [Timestamp] DESC";

        public static string GetExtremClosePrice = @"
                SELECT MIN(ClosePrice) AS 'LowPrice',MAX(ClosePrice) AS 'HighPrice' FROM [dbo].[{0}_IndicatorVwap]
                WHERE Instrument=@Instrument AND Exchange=@Exchange AND TimePeriod=@Period AND Timestamp BETWEEN @Time1 AND @Time2";

        public static string GetLastVwap = @"
                SELECT TOP 1 SumTypeVol,SumVolume,TotalTradesCount
                FROM dbo.[{0}_IndicatorVwap] WHERE TimePeriod=@Period AND Instrument=@Instrument AND Exchange=@Exchange
                ORDER BY Id DESC";

        public static string GetBinanceCandlesForVwap = @"
                SELECT TradeAt,Price,Quantity FROM [dbo].[{0}_TradeBinance] WITH (NOLOCK)
                WHERE TradeAt BETWEEN @Time1 AND @Time2 AND Instrument1=@Instrument1 AND Instrument2=@Instrument2
                ORDER BY TradeAt";

        public static string GetBitmexCandlesForVwap = @"
                SELECT Timestamp,Price,Size FROM [dbo].[{0}_TradeBitmex] WITH (NOLOCK)
                WHERE Timestamp BETWEEN @Time1 AND @Time2 AND Instrument='{1}'
                ORDER BY Timestamp";

        public static string GetVwapInstruments = @"
                SELECT Timestamp,Instrument,VwapRatioPcnt,VwapGain15Min AS 'VwapGain15Min1',CumVwapGain15Min AS 'CumVwapGain15Min1' 
                FROM dbo.[{0}_IndicatorVwap] 
                WHERE TimePeriod=@TimePeriod AND Exchange=@Exchange AND Timestamp BETWEEN @Time1 AND @Time2
                ORDER BY Timestamp";

        public static string GetLastVwapTimePeriod = @"
                SELECT TOP 1 Timestamp FROM [dbo].[{0}_IndicatorVwap] 
                WHERE TimePeriod=@TimePeriod AND Exchange=@Exchange ORDER BY Id DESC";

        public static string GetVwapGains = @"
                SELECT Id,Timestamp,Instrument,VwapRatioPcnt,VwapGainRatio15Min,CumVwapGain15Min AS 'CumVwapGain15Min1',ExtremClosePrice 
                FROM dbo.[{0}_IndicatorVwap] WHERE Exchange=@Exchange AND VwapGainRatio15Min IS NOT NULL 
                ORDER BY Timestamp";
    }
}
