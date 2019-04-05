USE master
GO

IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'BotCoin') 
BEGIN
	ALTER DATABASE BotCoin SET SINGLE_USER WITH ROLLBACK IMMEDIATE
	DROP DATABASE BotCoin
END
 
CREATE DATABASE BotCoin ON PRIMARY 
(
	NAME = BotCoin_data, FILENAME = 'E:\SqlServerData\Botcoin_data.mdf'
)
LOG ON 
(
	NAME = BotCoin_log, FILENAME = 'E:\SqlServerData\Botcoin_log.ldf'
)
