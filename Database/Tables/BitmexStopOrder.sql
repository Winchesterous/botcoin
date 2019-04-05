CREATE TABLE [dbo].[BtxStopOrder]
(
	[Id]				[int] IDENTITY(1,1) NOT NULL,	
	[PositionId]		[nvarchar](50)		NOT	NULL,
	[Timestamp]			[datetime]			NOT NULL,
	[StopPrice]			[float]				NOT	NULL,
	[StartWatchPrice]	[float]				NOT	NULL,
		
	CONSTRAINT [PK_BtxStopOrder] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

ALTER TABLE [dbo].[BtxStopOrder]  WITH CHECK ADD  CONSTRAINT [FK_BtxStopOrder_BtxStopOrder] FOREIGN KEY([PositionId])
REFERENCES [dbo].[BtxTrade] ([PositionId])
GO
ALTER TABLE [dbo].[BtxStopOrder] CHECK CONSTRAINT [FK_BtxStopOrder_BtxStopOrder]
GO

CREATE INDEX IDX_BtxStopOrder1 ON dbo.BtxStopOrder ([Timestamp])
GO
