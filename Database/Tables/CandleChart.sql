CREATE TABLE [dbo].[CandleChart]
(
	[Id]				[int] IDENTITY(1,1) NOT NULL,
	[TradeAt]			[datetime]			NOT NULL,
	[ExchangeId]		[smallint]			NOT NULL,
	[OpenPrice]			[money]				NOT NULL,
	[ClosePrice]		[money]				NOT NULL,
	[HighPrice]			[money]				NOT NULL,
	[LowPrice]			[money]				NOT NULL,
	[Instrument1]		[nvarchar](5)		NOT NULL,
	[Instrument2]		[nvarchar](5)		NOT NULL,	
		
	CONSTRAINT [PK_CandleChart] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

ALTER TABLE [dbo].[CandleChart]  WITH CHECK ADD  CONSTRAINT [FK_CandleChart_Exchange] 
FOREIGN KEY([ExchangeId]) REFERENCES [dbo].[Exchange] ([Id])
GO
ALTER TABLE [dbo].[CandleChart] CHECK CONSTRAINT [FK_CandleChart_Exchange]
GO

CREATE INDEX IDX_CandleChart1 ON dbo.CandleChart ([TradeAt])
CREATE INDEX IDX_CandleChart2 ON dbo.CandleChart ([Instrument1],[Instrument2])
CREATE INDEX IDX_CandleChart3 ON dbo.CandleChart ([ExchangeId])
