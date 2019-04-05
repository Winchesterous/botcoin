declare @usd smallint
select @usd=id from dbo.Currency where Code='USD'

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl)
VALUES (1,'Kuna',(select id from dbo.Currency where Code='UAH'),0.0025,10.0,'kuna.io/api/v2/');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl,PusherKey,CertificateName)
VALUES (2,'Bitstamp',(select id from dbo.Currency where Code='USD'),0.0025,0,'www.bitstamp.net/api/v2/','de504dc5763aeef9ff52','bitstampnet.crt');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl)
VALUES (3,'Bitbay',(select id from dbo.Currency where Code='PLN'),0.0043,10.0,'bitbay.net/API/');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl,WebsocketUrl)
VALUES (4,'Cex',@usd,0.0025,10.0,'cex.io/api/','wss://ws.cex.io/ws/');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl)
VALUES (5,'Quadriga',(select id from dbo.Currency where Code='CAD'),0.005,10.0,'api.quadrigacx.com/v2/');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl,WebsocketUrl)
VALUES (6,'Bitfinex',@usd,0.0025,10.0,'api.bitfinex.com/v1/','wss://api.bitfinex.com/ws/');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl)
VALUES (7,'Kraken',@usd,0.0025,10.0,'api.kraken.com');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl)
VALUES (8,'Bittrex',@usd,0.0025,10.0,'bittrex.com/api/v1.1/');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl,WebsocketUrl,CertificateName)
VALUES (9,'Binance',@usd,0.001,10.0,'api.binance.com/','wss://stream.binance.com:9443/stream?streams=','binancecom.crt');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl,PusherKey)
VALUES (10,'Wex',@usd,0.001,10.0,'wex.nz','ee987526a24ba107824c');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl,WebsocketUrl)
VALUES (11,'HitBtc',@usd,0.001,10.0,'api.hitbtc.com/api/2/','wss://api.hitbtc.com/api/2/ws');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl,WebsocketUrl)
VALUES (12,'XBtce',@usd,0.0025,10.0,'ttexchange.xbtce.net:8443/api/v1/','wss://ttexchange.xbtce.net:3020');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl,WebsocketUrl)
VALUES (13,'Btcc',@usd,0.001,10.0,'api.btcc.com/api_trade_v1.php','wss://spotusd-wsp.btcc.com/');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl)
VALUES (14,'Liqui',@usd,0.0025,10.0,'api.liqui.io/api/3/');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl,WebsocketUrl)
VALUES (15,'Okex',@usd,0.002,10.0,'www.okex.com/api/v1','wss://real.okex.com:10440/websocket/okexapi');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl,WebsocketUrl)
VALUES (16,'Bitmex',@usd,0.001,10.0,'www.bitmex.com','wss://www.bitmex.com/realtime');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,UsdMinValue,RestUrl,WebsocketUrl)
VALUES (17,'Gdax',@usd,0.001,10.0,'api.pro.coinbase.com','wss://ws-feed.pro.coinbase.com');

INSERT INTO dbo.Exchange (Id,Name,CurrencyId,TradeFee,MakerFee,UsdMinValue,RestUrl,WebsocketUrl,CertificateName)
VALUES (18,'BitmexTest',@usd,0.00075,-0.00025,10.0,'testnet.bitmex.com','wss://testnet.bitmex.com/realtime','wwwbitmexcom.crt');