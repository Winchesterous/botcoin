
CREATE TABLE [dbo].[PriceLevel]
(
	[LevelId]			[nvarchar](32)		NOT NULL,
	[IsActual]			[bit]				NOT	NULL,	
	[Date2]				[datetime]				NULL,
	[TimeFrame]			[nvarchar](3)			NULL,
		
	CONSTRAINT [PK_PriceLevel] PRIMARY KEY CLUSTERED 
	(
		[LevelId] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO