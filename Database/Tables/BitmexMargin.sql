CREATE TABLE [dbo].[BtxMargin]
(
	[Id]				[int] IDENTITY(1,1) NOT NULL,
	[AccountId]			[nvarchar](50)		NOT NULL,
	[Timestamp]			[datetime]			NOT NULL,
	[WalletBalance]		[decimal](18, 8)		NULL,
	[MarginBalance]		[decimal](18, 8)		NULL,
	[AvailableMargin]	[decimal](18, 8)		NULL,
	[MarginUsedPcnt]	[float]					NULL,
	[RealisedPnl]		[decimal](18, 8)		NULL,
	[GrossComm]			[decimal](18, 8)		NULL,
			
	CONSTRAINT [PK_BtxMargin] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

CREATE INDEX IDX_BtxMargin1 ON dbo.BtxMargin ([AccountId])
CREATE INDEX IDX_BtxMargin2 ON dbo.BtxMargin ([Timestamp])
CREATE INDEX IDX_BtxMargin3 ON dbo.BtxMargin ([WalletBalance],[MarginBalance])