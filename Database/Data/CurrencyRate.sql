declare @dt datetime
set @dt = GETUTCDATE()

INSERT INTO [dbo].[CurrencyRate] VALUES (5,28.3,@dt);
INSERT INTO [dbo].[CurrencyRate] VALUES (6,1,@dt);
INSERT INTO [dbo].[CurrencyRate] VALUES (7,112.613,@dt);
INSERT INTO [dbo].[CurrencyRate] VALUES (8,1.253,@dt);
INSERT INTO [dbo].[CurrencyRate] VALUES (9,3.34,@dt);
INSERT INTO [dbo].[CurrencyRate] VALUES (10,11.867,@dt);
INSERT INTO [dbo].[CurrencyRate] VALUES (14,31.42,@dt);

