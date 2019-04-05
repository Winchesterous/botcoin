DROP PROCEDURE [dbo].[GetPositionOpenPrice]
GO

CREATE PROCEDURE [dbo].[GetPositionOpenPrice]
	@positionId	 nvarchar(50)
AS
SET NOCOUNT ON
BEGIN
	DECLARE @openSide nvarchar(4)
	
	SELECT @openSide=Side FROM dbo.BtxPosition WHERE PositionId=@PositionId AND OrdText='OPN'

	SELECT (SELECT * FROM (
                SELECT SUM(LastQty*LastPrice)'SumProd' FROM dbo.BtxPosition
                WHERE PositionId=@PositionId AND Side=@openSide AND OrderId<>'00000000-0000-0000-0000-000000000000'
                )x)
				/
           (SELECT * FROM (
                SELECT SUM(LastQty)'Sum' FROM dbo.BtxPosition
                WHERE PositionId=@PositionId AND Side=@openSide AND OrderId<>'00000000-0000-0000-0000-000000000000'
                )x)
END
GO