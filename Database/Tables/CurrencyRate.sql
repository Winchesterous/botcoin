
CREATE TABLE [dbo].[CurrencyRate]
(
	[Id]			[int] IDENTITY(1,1)	NOT NULL,
	[CurrencyId]	[smallint]			NOT NULL,
	[Rate]			[money]				NOT NULL,
	[RateDate]		[datetime]			NOT NULL,
	
	CONSTRAINT [PK_CurrencyRate] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

ALTER TABLE [dbo].[CurrencyRate]  WITH CHECK ADD  CONSTRAINT [FK_CurrencyRate_Currency] 
FOREIGN KEY([CurrencyId]) REFERENCES [dbo].[Currency] ([Id])
GO

ALTER TABLE [dbo].[CurrencyRate] CHECK CONSTRAINT [FK_CurrencyRate_Currency]
GO