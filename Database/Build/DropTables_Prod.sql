SET QUOTED_IDENTIFIER ON;
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BittrexArbitrage]') AND type in (N'U'))
	DROP TABLE [dbo].[BittrexArbitrage]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EventLog]') AND type in (N'U'))
	DROP TABLE [dbo].[EventLog]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FailTrade]') AND type in (N'U'))
	DROP TABLE [dbo].[FailTrade]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BtcAccount]') AND type in (N'U'))
	DROP TABLE [dbo].[BtcAccount]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BchAccount]') AND type in (N'U'))
	DROP TABLE [dbo].[BchAccount]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LtcAccount]') AND type in (N'U'))
	DROP TABLE [dbo].[LtcAccount]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EthAccount]') AND type in (N'U'))
	DROP TABLE [dbo].[EthAccount]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[XrpAccount]') AND type in (N'U'))
	DROP TABLE [dbo].[XrpAccount]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DashAccount]') AND type in (N'U'))
	DROP TABLE [dbo].[DashAccount]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Account]') AND type in (N'U'))
	DROP TABLE [dbo].[Account]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Trade]') AND type in (N'U'))
	DROP TABLE [dbo].[Trade]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CurrencyRate]') AND type in (N'U'))
	DROP TABLE [dbo].[CurrencyRate]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExchangeApiKey]') AND type in (N'U'))
	DROP TABLE [dbo].[ExchangeApiKey]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Exchange]') AND type in (N'U'))
	DROP TABLE [dbo].[Exchange]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Currency]') AND type in (N'U'))
	DROP TABLE [dbo].[Currency]