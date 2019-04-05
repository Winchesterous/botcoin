CREATE TABLE [dbo].[BtxOrder]
(
	[Id]				[int] IDENTITY(1,1) NOT NULL,
	[Instrument]		[nvarchar](6)		NOT NULL,
	[OrderId]			[nvarchar](50)		NOT NULL,
	[CreatedAt]			[datetime]			NOT NULL,
	[OrdStatus]			[nvarchar](20)			NULL,
	[ExecInst]			[nvarchar](max)			NULL,
	[OrdType]			[nvarchar](15)			NULL,
	[OrdSide]			[nvarchar](4)			NULL,	
	[Price]				[decimal](18, 2)		NULL,
	[StopPrice]			[decimal](18, 2)		NULL,
	[OrderQty]			[int]					NULL,
	[CumQty]			[int]					NULL,
	[LeavesQty]			[int]					NULL,
	[AvgPrice]			[decimal](18, 2)		NULL,
	[OrdText]			[nvarchar](max)			NULL,
	[AccountId]			[nvarchar](50)		NOT	NULL,
		
	CONSTRAINT [PK_BtxOrder] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

ALTER TABLE [dbo].[BtxOrder] ADD CONSTRAINT [CK_BtxOrderType] 
CHECK ([OrdType] IN ('Limit','Market','Stop','StopLimit','LimitIfTouched','MarketIfTouched'))
GO
ALTER TABLE [dbo].[BtxOrder] ADD CONSTRAINT [CK_BtxOrderState] 
CHECK ([OrdStatus] IN ('New','Filled','PartiallyFilled','Canceled','Updated','Rejected','StopOrderTriggered'))
GO
ALTER TABLE [dbo].[BtxOrder] ADD CONSTRAINT [CK_BtxOrderSide] 
CHECK ([OrdSide] IN ('Buy','Sell'))
GO
CREATE INDEX IDX_BtxOrder1 ON dbo.BtxOrder ([CreatedAt])
CREATE INDEX IDX_BtxOrder2 ON dbo.BtxOrder ([OrdType],[OrdStatus])
CREATE INDEX IDX_BtxOrder3 ON dbo.BtxOrder ([OrdSide])