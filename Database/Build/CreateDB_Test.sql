USE master
GO

IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'Botcoin_Test') 
BEGIN
	ALTER DATABASE Botcoin_Test SET SINGLE_USER WITH ROLLBACK IMMEDIATE
	DROP DATABASE Botcoin_Test
END
 
CREATE DATABASE Botcoin_Test ON PRIMARY 
(
	NAME = Botcoin_Test_data, FILENAME = 'E:\SqlServerData\Botcoin_test_data.mdf'
)
LOG ON 
(
	NAME = Botcoin_Test_log, FILENAME = 'E:\SqlServerData\Botcoin_test_log.ldf'
)
