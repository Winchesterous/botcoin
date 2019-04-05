CREATE TABLE [dbo].[BtxPosition]
(
	[Id]				[int] IDENTITY(1,1) NOT NULL,	
	[TransactTime]		[datetime]			NOT NULL,
	[Side]				[nvarchar](4)		NOT	NULL,	
	[OrderQty]			[int]				NOT	NULL,
	[LeavesQty]			[int]				NOT	NULL,
	[CumQty]			[int]				NOT	NULL,
	[LastQty]			[int]					NULL,
	[LastPrice]			[float]					NULL,
	[Price]				[float]					NULL,
	[FeeRate]			[float]					NULL,
	[FeePaidXBT]		[decimal](18,8)			NULL,
	[ExecCostXBT]		[float]					NULL,
	[Balance]			[decimal](18,8)			NULL,
	[PositionId]		[nvarchar](50)			NULL,
	[OrderId]			[nvarchar](50)		NOT	NULL,
	[OrdStatus]			[nvarchar](40)			NULL,
	[OrdText]			[nvarchar](max)			NULL,
	[Instrument]		[nvarchar](10)		NOT NULL,	
	[AccountId]			[nvarchar](50)		NOT NULL,
		
	CONSTRAINT [PK_BtxPosition] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

CREATE INDEX IDX_BtxPosition1 ON dbo.BtxPosition ([TransactTime])
CREATE INDEX IDX_BtxPosition2 ON dbo.BtxPosition ([Instrument],[AccountId])
CREATE INDEX IDX_BtxPosition3 ON dbo.BtxPosition ([OrderId])
CREATE INDEX IDX_BtxPosition4 ON dbo.BtxPosition ([PositionId])
