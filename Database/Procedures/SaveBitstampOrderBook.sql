CREATE PROCEDURE [dbo].[SaveBitstampOrderBook]
	@orderId		nvarchar(20),
	@timestamp		datetime,
	@createdAt		datetime,
	@deleted		bit,
	@instrument1	nvarchar(5),
	@instrument2	nvarchar(5),
	@bidPrice		decimal(18, 5),
	@askPrice		decimal(18, 5),
	@bidAmount		decimal(18, 8),
	@askAmount		decimal(18, 8),
	@orderBookId	int output
AS
SET NOCOUNT ON
BEGIN
	declare @spread decimal(18, 5),
			@minAsk decimal(18, 5),
			@maxBid decimal(18, 5)

	if @deleted=0
	begin
		select top 1 @minAsk=MinAsk,@maxBid=MaxBid from dbo.OrderBookBitstamp where Instrument1=@instrument1 order by Id desc
	end

	insert into dbo.OrderBookBitstamp(
		OrderId,IsDeleted,Timestamp,CreatedAt,Instrument1,Instrument2,BidPrice,AskPrice,BidAmount,AskAmount,Spread,MinAsk,MaxBid
	) 
    values (
		@OrderId,@IsDeleted,@Timestamp,@CreatedAt,@Instrument1,@Instrument2,@BidPrice,@AskPrice,@BidAmount,@AskAmount,@spread,@minAsk,@maxBid
	)

    select @orderBookId=cast(scope_identity() as int)
END
GO