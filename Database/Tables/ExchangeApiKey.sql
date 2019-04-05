
CREATE TABLE [dbo].[ExchangeApiKey]
(
	[Id]				[int] IDENTITY(1,1) NOT NULL,
	[ExchangeId]		[smallint]			NOT NULL,
	[ApiType]			[nvarchar](10)		NOT NULL,
	[PublicKey]			[nvarchar](300)		NOT NULL,	
	[SecretKey]			[nvarchar](300)		NOT NULL,	
	[ClientId]			[nvarchar](50)			NULL,	
		
	CONSTRAINT [PK_ExchangeApiKey] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

ALTER TABLE [dbo].[ExchangeApiKey]  WITH CHECK ADD  CONSTRAINT [FK_ExchangeApiKey_Exchange] 
FOREIGN KEY([ExchangeId]) REFERENCES [dbo].[Exchange] ([Id])
GO
ALTER TABLE [dbo].[ExchangeApiKey] CHECK CONSTRAINT [FK_ExchangeApiKey_Exchange]
GO

ALTER TABLE [dbo].[ExchangeApiKey] ADD CONSTRAINT [CK_ApiKeyType]
CHECK ([ApiType] IN ('Trade'))
GO