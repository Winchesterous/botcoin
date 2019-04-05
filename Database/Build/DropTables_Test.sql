SET QUOTED_IDENTIFIER ON;
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VwapIndex]') AND type in (N'U'))
	DROP TABLE [dbo].[VwapIndex]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BtxStopOrder]') AND type in (N'U'))
	DROP TABLE [dbo].[BtxStopOrder]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BtxLiquidation]') AND type in (N'U'))
	DROP TABLE [dbo].[BtxLiquidation]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BtxScalperState]') AND type in (N'U'))
	DROP TABLE [dbo].[BtxScalperState]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BitmexWallet]') AND type in (N'U'))
	DROP TABLE [dbo].[BitmexWallet]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BitmexTrade]') AND type in (N'U'))
	DROP TABLE [dbo].[BitmexTrade]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BitmexPosition]') AND type in (N'U'))
	DROP TABLE [dbo].[BitmexPosition]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BitmexOrder]') AND type in (N'U'))
	DROP TABLE [dbo].[BitmexOrder]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BitmexMargin]') AND type in (N'U'))
	DROP TABLE [dbo].[BitmexMargin]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BitmexInstrument]') AND type in (N'U'))
	DROP TABLE [dbo].[BitmexInstrument]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BitmexAccount]') AND type in (N'U'))
	DROP TABLE [dbo].[BitmexAccount]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExchangeOrder]') AND type in (N'U'))
	DROP TABLE [dbo].[ExchangeOrder]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExchangePosition]') AND type in (N'U'))
	DROP TABLE [dbo].[ExchangePosition]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EventLog]') AND type in (N'U'))
	DROP TABLE [dbo].[EventLog]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CurrencyRate]') AND type in (N'U'))
	DROP TABLE [dbo].[CurrencyRate]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ScalperEvent]') AND type in (N'U'))
	DROP TABLE [dbo].[ScalperEvent]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExchangeApiKey]') AND type in (N'U'))
	DROP TABLE [dbo].[ExchangeApiKey]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExchangeOrder]') AND type in (N'U'))
	DROP TABLE [dbo].[ExchangeOrder]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PriceLevel]') AND type in (N'U'))
	DROP TABLE [dbo].[PriceLevel]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CandleChart]') AND type in (N'U'))
	DROP TABLE [dbo].[CandleChart]
	
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Exchange]') AND type in (N'U'))
	DROP TABLE [dbo].[Exchange]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Currency]') AND type in (N'U'))
	DROP TABLE [dbo].[Currency]
