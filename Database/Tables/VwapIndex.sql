CREATE TABLE [dbo].[VwapIndex]
(
	[Id]			[int] IDENTITY(1,1) NOT NULL,
	[Exchange]		[nvarchar](15)		NOT NULL,
	[TimePeriod]	[nvarchar](3)		NOT NULL,
	[Timestamp]		[datetime]			NOT NULL,
	[Instrument]	[nvarchar](10)			NULL,
	[VwapGainPcnt]	[decimal](18, 3)		NULL,
	[VwapIndex]		[decimal](18, 3)		NULL,
	[SwapVwapIndex] [decimal](18, 3)		NULL,
	[AltVwapIndex]	[decimal](18, 3)		NULL,
			
	CONSTRAINT [PK_VwapIndex] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

CREATE INDEX IDX_VwapIndex1 ON dbo.VwapIndex ([Exchange])
CREATE INDEX IDX_VwapIndex2 ON dbo.VwapIndex ([TimePeriod])
CREATE INDEX IDX_VwapIndex3 ON dbo.VwapIndex ([Instrument])
GO