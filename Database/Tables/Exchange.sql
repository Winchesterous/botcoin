
CREATE TABLE [dbo].[Exchange]
(
	[Id]				[smallint]			NOT NULL,
	[Name]				[nvarchar](15)		NOT NULL,
	[CurrencyId]		[smallint]			NOT NULL,	
	[TradeFee]			[float]				NOT NULL,
	[UsdMinValue]		[float]				NOT NULL,
	[RestUrl]			[nvarchar](100)		NOT NULL,	
	[WebsocketUrl]		[nvarchar](200)			NULL,	
	[PusherKey]			[nvarchar](30)			NULL,	
	[CertificateName]	[nvarchar](30)			NULL,
	[MakerFee]			[float]					NULL,
		
	CONSTRAINT [PK_Exchange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

ALTER TABLE [dbo].[Exchange]  WITH CHECK ADD  CONSTRAINT [FK_Exchange_Currency]
FOREIGN KEY([CurrencyId]) REFERENCES [dbo].[Currency] ([Id])
GO
ALTER TABLE [dbo].[Exchange] CHECK CONSTRAINT [FK_Exchange_Currency]
GO