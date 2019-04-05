CREATE TABLE [dbo].[ScalperEvent]
(
	[Id]			[int] IDENTITY(1,1) NOT NULL,
	[SessionId]		[nvarchar](36)		NOT NULL,
	[Timestamp]		[datetime]			NOT NULL,
	[EventType]		[nvarchar](10)		NOT NULL,
	[Message]		[nvarchar](max)		NOT NULL,
	
	CONSTRAINT [PK_ScalperEvent] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

ALTER TABLE [dbo].[ScalperEvent] ADD CONSTRAINT [CK_ScalperEvent_EventType] 
CHECK ([EventType] IN ('Order','Margin','Position','Watcher'))
GO
