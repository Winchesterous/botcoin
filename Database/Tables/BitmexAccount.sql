CREATE TABLE [dbo].[BtxAccount]
(
	[AccountId]		[nvarchar](50)		NOT	NULL,
	[AccountName]	[nvarchar](20)		NOT NULL,
	[UserName]		[nvarchar](50)		NOT NULL,
	[Created]		[datetime]			NOT NULL,
	[FirstName]		[nvarchar](50)			NULL,
	[LastName]		[nvarchar](50)			NULL,
		
	CONSTRAINT [PK_BtxAccount] PRIMARY KEY CLUSTERED 
	(
		[AccountId] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO
