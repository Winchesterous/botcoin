CREATE TABLE [dbo].[BtxTrade]
(
	[Id]				[int] IDENTITY(1,1) NOT NULL,	
	[TradeType]			[nvarchar](5)		NOT NULL,
	[StartTime]			[datetime]			NOT NULL,
	[EndTime]			[datetime]				NULL,
	[ElapsedTime]		[nvarchar](50)			NULL,
	[OrderQty]			[int]				NOT	NULL,
	[OpenPrice]			[float]				NOT	NULL,
	[ClosePrice]		[float]					NULL,
	[RiskPcnt]			[float]				NOT	NULL,
	[StopValue]			[float]				NOT	NULL,
	[PriceGain]			[float]					NULL,	
	[RealisedPnlXBT]	[decimal](18,8)			NULL,
	[FeePaidXBT]		[decimal](18,8)			NULL,
	[TakeStopRatio]		[decimal](18,2)			NULL,
	[Instrument]		[nvarchar](10)		NOT NULL,
	[PositionId]		[nvarchar](50)		NOT	NULL,	
	[AccountId]			[nvarchar](50)		NOT NULL,
		
	CONSTRAINT [PK_BtxTrade] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	CONSTRAINT [IX_BtxTrade] UNIQUE NONCLUSTERED 
	(
		[PositionId] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

CREATE INDEX IDX_BtxTrade1 ON dbo.BtxTrade (StartTime,EndTime)
CREATE INDEX IDX_BtxTrade2 ON dbo.BtxTrade ([Instrument],[AccountId])
CREATE INDEX IDX_BtxTrade3 ON dbo.BtxTrade ([TradeType])
CREATE INDEX IDX_BtxTrade4 ON dbo.BtxTrade ([ClosePrice])
CREATE INDEX IDX_BtxTrade5 ON dbo.BtxTrade ([OpenPrice])

