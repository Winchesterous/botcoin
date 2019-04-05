using BotCoin.DataType;
using BotCoin.Exchange;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace BotCoin.TradeDataBotService
{
    class Bitmex
    {
        readonly List<string> Instruments;
        readonly BitmexExchange _bitmex;
        List<string> _contracts;

        public Bitmex(BitmexExchange exchange)
        {
            Instruments = new List<string> { "XBT", "ETH", "EOS", "BCH", "XRP", "LTC", "TRX", "ADA" };            
            _contracts  = new List<string> { "XBTUSD", "ETHUSD" };
            _bitmex     = exchange;

            CreateContracts();
        }
        
        public List<string> Contracts
        {
            get { return _contracts; }
        }

        private void CreateContracts()
        {
            var xbtQuarterly = ConfigurationManager.AppSettings["QuarterlyLetter"][0];
            foreach (var instrument in Instruments)
            {
                var contract = String.Format("{0}{1}{2}", instrument, xbtQuarterly, (DateTime.UtcNow.Year - 2000).ToString());
                _contracts.Add(contract);
            }
        }

        public void SubscribeLiquidation(BitmexExchange ex, Action<object, LiquidationEventArgs> liqCallback)
        {
            foreach (var contract in _contracts)
            {
                ex.SubscribeTopics(true, contract, BtxSubscriptionItem.liquidation);
            }
            ex.LiquidationReceived += (s, e) =>
            {
                liqCallback(ex, e);
            };
        }

        public void SubscribeTicker(BitmexExchange ex, Action<object, TickerEventArgs> callback)
        {
            foreach (var contract in _contracts)
            {
                ex.SubscribeTopics(true, contract, BtxSubscriptionItem.instrument);
            }
            ex.InstrumentChanged += (s, e) =>
            {
                callback(ex, new TickerEventArgs(e.BtxInstrument));
            };
        }
    }
}
