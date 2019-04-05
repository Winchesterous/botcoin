CREATE TABLE [dbo].[BtxScalperState]
(
	[Id]				[int] IDENTITY(1,1) NOT NULL,	
	[AccountId]			[nvarchar](50)		NOT NULL,
	[Instrument]		[nvarchar](6)		NOT NULL,
	[Opened]			[smallint]			NOT NULL,
	[Timestamp]			[datetime]			NOT NULL,
	[HostName]			[nvarchar](50)		NOT	NULL,
	[StateName]			[nvarchar](max)		NOT	NULL,
	[OrderQty]			[int]					NULL,
	[LongPosition]		[smallint]				NULL,	
	[StopOrderId]		[nvarchar](50)			NULL,	
	[StopLoss]			[float]					NULL,
	[StopSlip]			[float]					NULL,
	[StartWatchPrice]	[float]					NULL,
	[StopPrice]			[float]					NULL,
		
	CONSTRAINT [PK_BtxScalperState] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO
