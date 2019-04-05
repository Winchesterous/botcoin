using BotCoin.DataType;
using BotCoin.Logger;
using BotCoin.Service;

namespace BotCoin.OrderBookService
{
    internal class SelfHost
    {
        readonly ServiceEventLogger _log;
        readonly PricesPublisher _publisher;

        public SelfHost()
        {
            _log = new ServiceEventLogger(ServiceName.OrderBook, new DbGatewayService(ServiceName.Arbitrage));
            _publisher = new PricesPublisher(_log);
        }

        public void Start()
        {
            _log.WriteInfo("Starting");
            _publisher.Start();
            _log.WriteInfo("Started");
        }

        public void Stop()
        {
            _log.WriteInfo("Stopping");
            _publisher.Stop();
            _log.WriteInfo("Stopped");
        }
    }
}
