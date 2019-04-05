CREATE PROCEDURE CreateBitmexFunding
	@date		datetime,
	@dropOnly	bit = 0
AS
BEGIN
	SET NOCOUNT ON;

    declare @sql nvarchar(max), @suffix varchar(6)
	set @suffix = 
		cast(datepart(year, @date)-2000 as varchar(2)) 
		+
		case 
			when datepart(month, @date) < 10 then '0' + cast(datepart(month, @date) as varchar(2))
			when datepart(month, @date) >= 10 then cast(datepart(month, @date) as varchar(2))
		end 
		+
		case 
			when datepart(day, @date) < 10 then '0' + cast(datepart(day, @date) as varchar(2))
			when datepart(day, @date) >= 10 then cast(datepart(day, @date) as varchar(2))
		end

	set @sql='
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''dbo.[' + @suffix + '_BitmexFunding]'') AND type in (N''U''))
		DROP TABLE dbo.[' + @suffix + '_BitmexFunding]'
		
	if @dropOnly = 0
		set @sql = @sql + '
		CREATE TABLE dbo.[' + @suffix + '_BitmexFunding]
		(
			[Id]					[int] IDENTITY(1,1)	NOT NULL,
			[Timestamp]				[datetime] NOT NULL,
			[Instrument]			[nvarchar](10) NOT NULL,
			[Vwap]					[decimal](18, 5) NULL,
			[FundingRate]			[float] NULL,
			[FundingTimestamp]		[datetime] NULL,
			[IndicativeFundingRate] [float] NULL,
			[HighPrice]				[money] NULL,
			[LowPrice]				[money] NULL,
			CONSTRAINT PK_BitmexFunding' + @suffix + ' PRIMARY KEY CLUSTERED 
			(
				[Id] ASC
			)
			WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) 
		ON [PRIMARY]
		CREATE INDEX IDX_BitmexFunding' + @suffix + '_Time ON dbo.[' + @suffix + '_BitmexFunding] ([Timestamp])
		CREATE INDEX IDX_BitmexFunding' + @suffix + '_Instrument ON dbo.[' + @suffix + '_BitmexFunding] ([Instrument])'

	exec (@sql)
END
GO
