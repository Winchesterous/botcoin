using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.Exchange;
using BotCoin.Instruments;
using BotCoin.Logger;
using BotCoin.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BotCoin.OrderBookService
{
    internal class PricesPublisher
    {
        readonly Dictionary<ExchangeName, BaseExchange> _restExchanges;
        readonly Dictionary<ExchangeName, BaseExchange> _wsExchanges;
        readonly Dictionary<CurrencyName, Instrument> _wsInstruments;
        readonly Dictionary<CurrencyName, Instrument> _restInstruments;
        readonly RestExchangeListener _restSubscriber;
        readonly ServiceEventLogger _log;
        readonly DbGatewayService _dbGateway;
        readonly DbRepositoryService _dbRepo;
        readonly TcpService _tcp;
        private TickerService _ticker;
        private IPEndPoint _endPoint;

        public PricesPublisher(ServiceEventLogger log)
        {
            _restInstruments = new Dictionary<CurrencyName, Instrument>();
            _restExchanges   = new Dictionary<ExchangeName, BaseExchange>();
            _wsExchanges     = new Dictionary<ExchangeName, BaseExchange>();
            _wsInstruments   = new Dictionary<CurrencyName, Instrument>();            
            _dbGateway       = (DbGatewayService)log.DbRepository;
            _dbRepo          = new DbRepositoryService();
            _restSubscriber  = new RestExchangeListener(_dbGateway, log);
            _tcp             = new TcpService();
            _log             = log;

            Instrument.InitWebsocketInstruments(_wsInstruments);
            Instrument.InitRestInstruments(_restInstruments);

            _restSubscriber.ExchangePricesUpdate += RestRequestHandler;
        }

        private void CreateExchanges()
        {
            var settings = _dbGateway.GetExchangeSettings();

            Action<string, Type, bool> initExchange = (exchangeName, type, isWebsocket) =>
            {
                var setting = settings.Where(s => s.Exchange.ToString() == exchangeName).Single();
                var ex = (BaseExchange)Activator.CreateInstance(type, setting);
                ex.Log = _log;
                ex.DbGateway = _dbGateway;
                ex.InitExchange();
                //if (isWebsocket) _wsExchanges[ex.GetExchangeName()] = ex;
                /*else*/ _restExchanges[ex.GetExchangeName()] = ex;
            };

            #region REST
            var restExchanges = _dbGateway.GetRestEnabledExchanges();
            var exchanges = new Dictionary<string, Type>();

            Utils.MapExchangeAttributes((attrName, isWebsocket, type) =>
                {
                    if (restExchanges.ContainsKey(attrName))
                        if (!isWebsocket)
                            exchanges[attrName] = type;
                });

            foreach (var ex in exchanges)
                initExchange(ex.Key, ex.Value, false);
            #endregion

            #region WebSocket
            var wsExchanges = _dbGateway.GetWebSocketEnabledExchanges();
            exchanges.Clear();
            
           Utils.MapExchangeAttributes((attrName, isWebsocket, type) =>
                {
                    if (wsExchanges.ContainsKey(attrName))
                        if (isWebsocket)
                            exchanges[attrName] = type;
                });

            foreach (var ex in exchanges)
                initExchange(ex.Key, ex.Value, true);
            #endregion
        }

        private void RestRequestHandler(object sender, CurrencyName currency)
        {
            foreach (BaseRestExchange ex in _restExchanges.Values)
            {
                Task.Run(() => ex.RestRequestHandler(currency));
            }
        }

        private void OrderBookRestHandler(ExchangeName exName, OrderBookEventArgs orderBook)
        {
            var ex = _restExchanges[exName];
            SendExchangePrices(ex, orderBook, _restInstruments[orderBook.Instrument1]);
        }

        private void OrderBookWebSocketHandler(ExchangeName exName, OrderBookEventArgs args)
        {
            var ex = _wsExchanges[exName];
            SendExchangePrices(ex, args, _wsInstruments[args.Instrument1]);
        }

        private void SendExchangePrices(BaseExchange ex, OrderBookEventArgs args, Instrument instrument)
        {
            if (!ex.ValidOrderPrices(args, instrument))
                return;

            var priceArgs = new ExchangePricesEventArgs
            {
                CreatedAt       = DateTime.UtcNow,
                Timestamp       = args.Timestamp,
                Instrument1     = args.Instrument1,
                Instrument2     = args.Instrument2,
                OrderId         = args.OrderId,
                Exchange        = ex.GetExchangeName(),
                IsOrderDeleted  = args.IsOrderDeleted
            };

            Task.Run(() => _dbRepo.SaveOrderBook(priceArgs, args.OrderBook));

            instrument.InitExchangePrice(ex, priceArgs);
            _dbGateway.SendPrices(priceArgs);
        }

        public void Start()
        {
            Action<BaseExchange> subscribeRestExchange = client =>
            {
                client.Logon();
                foreach (var ins in _restInstruments.Values)
                    ins.SubscribeOrderBook(client, (n, o) => OrderBookRestHandler(n, o));
            };

            var config     = (BotcoinConfigSection)ConfigurationManager.GetSection("botcoin");
            var connection = config.FindConnectionElement("Arbitrage");
#if true
            var domainName = connection.DomainName;
            if (Dns.Resolve(Dns.GetHostName()).AddressList[0].ToString() == "185.226.113.60")
            {
                var str = domainName.Split('.');
                domainName = String.Format("{0}-test.{1}.{2}", str[0], str[1], str[2]);
            }
#endif
            _endPoint = new IPEndPoint(DbGatewayService.GetAddressByHost(domainName), connection.Port);
            _ticker   = new TickerService(_restInstruments, /*_wsInstruments,*/ _dbGateway.GetExchangeSettings(), _log);

            CreateExchanges();

            try
            {
                _ticker.Update();
                
                foreach (var client in _restExchanges.Values)
                    subscribeRestExchange(client);

                foreach (var client in _wsExchanges.Values)
                    subscribeWsExchange(client);

                _ticker.Start();
                _restSubscriber.Start();
            }
            catch (Exception ex)
            {
                _log.WriteError(ex.Message);
            }
        }

        public void Stop()
        {
            try
            {
                foreach (var client in _wsExchanges.Values)
                    unsubscribe(client);

                foreach (var client in _restExchanges.Values)
                    client.Logout();

                _restSubscriber.Stop();
                _ticker.Stop();
            }
            catch (Exception ex)
            {
                _log.WriteError(ex.Message);
            }
        }
    }
}
