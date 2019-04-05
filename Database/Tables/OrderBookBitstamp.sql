CREATE TABLE dbo.OrderBookBitstamp
(
	[Id]			[int] IDENTITY(1,1)	NOT NULL,
	[IsBuyOrder]	[bit]				NOT	NULL,	
	[Price]			[decimal](18, 8)	NOT NULL,
	[Amount]		[decimal](18, 8)	NOT NULL,
	
	CONSTRAINT PK_OrderBookBitstamp PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]

CREATE INDEX IDX_Bitstamp_Price ON dbo.OrderBookBitstamp (Price)
