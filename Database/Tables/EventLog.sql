
CREATE TABLE [dbo].[EventLog]
(
	[Id]			[int] IDENTITY(1,1) NOT NULL,
	[SessionId]		[nvarchar](50)		NOT NULL,
	[Timestamp]		[datetime]			NOT NULL,
	[ServiceName]	[nvarchar](20)		NOT NULL,
	[EventType]		[nvarchar](4)		NOT NULL,
	[ExchangeId]	[smallint]				NULL,
	[Message]		[nvarchar](max)		NOT NULL,
	
	CONSTRAINT [PK_EventLog] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

ALTER TABLE [dbo].[EventLog] ADD CONSTRAINT [CK_EventLog_EventType] 
CHECK ([EventType] IN ('Info','Fail','Warn'))
GO

ALTER TABLE [dbo].[EventLog] ADD CONSTRAINT [CK_EventLog_ServiceName] 
CHECK ([ServiceName] IN ('TradeDataBot','WebApi','Desktop','InitService','BitmexBot'))
GO