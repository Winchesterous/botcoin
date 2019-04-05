
CREATE TABLE [dbo].[PriceLevelDate]
(
	[Id]				[int] IDENTITY(1,1)	NOT NULL,
	[LevelId]			[nvarchar](32)		NOT NULL,
	[IsLevelUp]			[bit]				NOT	NULL,	
	[Price]				[decimal](18, 2)		NULL,		
	[LevelDate]			[datetime]				NULL,
	[IsFalseBreakdown]	[bit]					NULL,
		
	CONSTRAINT [PK_PriceLevelDate] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

ALTER TABLE dbo.PriceLevelDate  WITH CHECK ADD  CONSTRAINT FK_PriceLevelDate 
FOREIGN KEY(LevelId) REFERENCES dbo.PriceLevel (LevelId)
GO
ALTER TABLE dbo.PriceLevelDate CHECK CONSTRAINT FK_PriceLevelDate
GO

CREATE INDEX IDX_PriceLevelDate1 ON dbo.PriceLevelDate (Price)
GO