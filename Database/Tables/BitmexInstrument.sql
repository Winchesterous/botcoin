CREATE TABLE [dbo].[BtxInstrument]
(
	[Id]			[int] IDENTITY(1,1) NOT NULL,
	[AccountId]		[nvarchar](50)		NOT	NULL,
	[Symbol]		[nvarchar](40)		NOT NULL,
	[Timestamp]		[datetime]			NOT NULL,
	[MakerFee]		[decimal](18,5)		NOT	NULL,
	[TakerFee]		[decimal](18,5)		NOT	NULL,
	[SettlementFee]	[decimal](18,5)		NOT	NULL,
	[TickSize]		[float]				NOT	NULL,
	[Index]			[int]				NOT	NULL,
		
	CONSTRAINT [PK_BtxInstrument] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO
