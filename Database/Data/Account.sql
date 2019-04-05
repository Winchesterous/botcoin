declare @balance money, @btc_ float, @eth_ float, @bch_ float, @ltc_ float, @id int, @xrp_ float, @dash_ float, @dt datetime
set @balance=900
set @btc_=0.1
set @eth_=1
set @bch_=0.8
set @ltc_=5
set @xrp_=1170
set @dash_=1.5
set @dt=getutcdate()

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='Kuna'),'SYN',28.3,@balance*28.3,@dt);	
set @id=@@IDENTITY
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@id,@btc_);
INSERT INTO [dbo].[LtcAccount](AccountId,LtcBalance) 
VALUES (@id,@ltc_);
INSERT INTO [dbo].[BchAccount](AccountId,BchBalance) 
VALUES (@id,@bch_);
INSERT INTO [dbo].[EthAccount](AccountId,EthBalance) 
VALUES (@id,@eth_);
INSERT INTO [dbo].[XrpAccount](AccountId,XrpBalance) 
VALUES (@id,@xrp_);
INSERT INTO [dbo].[DashAccount](AccountId,DashBalance) 
VALUES (@id,@dash_);

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='Bitstamp'),'SYN',1,@balance,@dt);
set @id=@@IDENTITY
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@id,@btc_);
INSERT INTO [dbo].[BchAccount](AccountId,BchBalance) 
VALUES (@id,@bch_);
INSERT INTO [dbo].[EthAccount](AccountId,EthBalance) 
VALUES (@id,@eth_);
INSERT INTO [dbo].[LtcAccount](AccountId,LtcBalance) 
VALUES (@id,@ltc_);
INSERT INTO [dbo].[XrpAccount](AccountId,XrpBalance) 
VALUES (@id,@xrp_);

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='Bitbay'),'SYN',3.34,@balance*3.34,@dt);
set @id=@@IDENTITY
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@id,@btc_);
INSERT INTO [dbo].[BchAccount](AccountId,BchBalance) 
VALUES (@id,@bch_);
INSERT INTO [dbo].[EthAccount](AccountId,EthBalance) 
VALUES (@id,@eth_);
INSERT INTO [dbo].[LtcAccount](AccountId,LtcBalance) 
VALUES (@id,@ltc_);
INSERT INTO [dbo].[DashAccount](AccountId,DashBalance) 
VALUES (@id,@dash_);

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='CEX'),'SYN',1,@balance,@dt);
set @id=@@IDENTITY
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@id,@btc_);
INSERT INTO [dbo].[BchAccount](AccountId,BchBalance) 
VALUES (@id,@bch_);
INSERT INTO [dbo].[EthAccount](AccountId,EthBalance) 
VALUES (@id,@eth_);
INSERT INTO [dbo].[XrpAccount](AccountId,XrpBalance) 
VALUES (@id,@xrp_);
INSERT INTO [dbo].[DashAccount](AccountId,DashBalance) 
VALUES (@id,@dash_);

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='Quadriga'),'SYN',1.253,@balance*1.253,@dt);
set @id=@@IDENTITY
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@id,@btc_);
INSERT INTO [dbo].[BchAccount](AccountId,BchBalance) 
VALUES (@id,@bch_);
INSERT INTO [dbo].[EthAccount](AccountId,EthBalance) 
VALUES (@id,@eth_);
INSERT INTO [dbo].[LtcAccount](AccountId,LtcBalance) 
VALUES (@id,@ltc_);

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='Bitfinex'),'SYN',1,@balance,@dt);
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@@IDENTITY,@btc_);

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='Kraken'),'SYN',1,@balance,@dt);
set @id=@@IDENTITY
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@id,@btc_);
INSERT INTO [dbo].[BchAccount](AccountId,BchBalance) 
VALUES (@id,@bch_);
INSERT INTO [dbo].[EthAccount](AccountId,EthBalance) 
VALUES (@id,@eth_);
INSERT INTO [dbo].[LtcAccount](AccountId,LtcBalance) 
VALUES (@id,@ltc_);
INSERT INTO [dbo].[XrpAccount](AccountId,XrpBalance) 
VALUES (@id,@xrp_);
INSERT INTO [dbo].[DashAccount](AccountId,DashBalance) 
VALUES (@id,@dash_);

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='Bittrex'),'SYN',1,@balance,@dt);	
set @id=@@IDENTITY
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@id,@btc_);
INSERT INTO [dbo].[EthAccount](AccountId,EthBalance) 
VALUES (@id,@eth_);
INSERT INTO [dbo].[LtcAccount](AccountId,LtcBalance) 
VALUES (@id,@ltc_);
INSERT INTO [dbo].[XrpAccount](AccountId,XrpBalance) 
VALUES (@id,@xrp_);
INSERT INTO [dbo].[DashAccount](AccountId,DashBalance) 
VALUES (@id,@dash_);

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='OkEx'),'SYN',1,@balance,@dt);	
set @id=@@IDENTITY
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@id,@btc_);
INSERT INTO [dbo].[EthAccount](AccountId,EthBalance) 
VALUES (@id,@eth_);
INSERT INTO [dbo].[LtcAccount](AccountId,LtcBalance) 
VALUES (@id,@ltc_);
INSERT INTO [dbo].[BchAccount](AccountId,BchBalance) 
VALUES (@id,@bch_);

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='Binance'),'SYN',1,@balance,@dt);	
set @id=@@IDENTITY
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@id,@btc_);
INSERT INTO [dbo].[EthAccount](AccountId,EthBalance) 
VALUES (@id,@eth_);
INSERT INTO [dbo].[BchAccount](AccountId,BchBalance) 
VALUES (@id,@bch_);
INSERT INTO [dbo].[LtcAccount](AccountId,LtcBalance) 
VALUES (@id,@ltc_);

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='Wex'),'SYN',1,@balance,@dt);	
set @id=@@IDENTITY
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@id,@btc_);
INSERT INTO [dbo].[EthAccount](AccountId,EthBalance) 
VALUES (@id,@eth_);
INSERT INTO [dbo].[BchAccount](AccountId,BchBalance) 
VALUES (@id,@bch_);
INSERT INTO [dbo].[LtcAccount](AccountId,LtcBalance) 
VALUES (@id,@ltc_);
INSERT INTO [dbo].[DashAccount](AccountId,DashBalance) 
VALUES (@id,@dash_);

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='HitBtc'),'SYN',1,@balance,@dt);	
set @id=@@IDENTITY
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@id,@btc_);

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='XBtce'),'SYN',1,@balance,@dt);	
set @id=@@IDENTITY
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@id,@btc_);
INSERT INTO [dbo].[EthAccount](AccountId,EthBalance) 
VALUES (@id,@eth_);
INSERT INTO [dbo].[BchAccount](AccountId,BchBalance) 
VALUES (@id,@bch_);
INSERT INTO [dbo].[LtcAccount](AccountId,LtcBalance) 
VALUES (@id,@ltc_);
INSERT INTO [dbo].[DashAccount](AccountId,DashBalance) 
VALUES (@id,@dash_);

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='Btcc'),'SYN',1,@balance,@dt);	
set @id=@@IDENTITY
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@id,@btc_);

INSERT INTO [dbo].[Account](ExchangeId,OperationCode,CurrencyRate,Amount,CreatedAt) 
VALUES ((SELECT Id FROM Exchange WHERE Name='Liqui'),'SYN',1,@balance,@dt);	
set @id=@@IDENTITY
INSERT INTO [dbo].[BtcAccount](AccountId,BtcBalance) 
VALUES (@id,@btc_);

UPDATE [dbo].[Account] SET Balance = Amount