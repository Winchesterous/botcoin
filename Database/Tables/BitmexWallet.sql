CREATE TABLE [dbo].[BtxWallet]
(
	[Id]				[int] IDENTITY(1,1) NOT NULL,
	[AccountId]			[nvarchar](50)		NOT NULL,
	[Timestamp]			[datetime]			NOT NULL,
	[Balance]			[decimal](18, 8)	NOT	NULL,
	[DeltaAmount]		[decimal](18, 8)	NOT	NULL,
	[Withdrawn]			[decimal](18, 8)		NULL,
	[DeltaDeposited]	[decimal](18, 8)		NULL,
	[DeltaWithdrawn]	[decimal](18, 8)		NULL,
	[Address]			[nvarchar](max)			NULL,
		
	CONSTRAINT [PK_BtxWallet] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

CREATE INDEX IDX_BtxWallet1 ON dbo.BtxWallet ([AccountId])
CREATE INDEX IDX_BtxWallet2 ON dbo.BtxWallet ([Timestamp])