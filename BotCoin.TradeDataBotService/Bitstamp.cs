using BotCoin.DataType;
using BotCoin.Exchange;
using BotCoin.Logger;
using BotCoin.Service;
using System;

namespace BotCoin.TradeDataBotService
{
    class Bitstamp
    {
        public static void SubscribeInstruments(BitstampExchange ex, CurrencyName[] btcInstruments, CurrencyName[] usdInstruments, Action<object, ExchangePricesEventArgs> callback, string usdToken = "usdt")
        {
            ex.OnBtcOrderBook += (s, e) => callback(ex, new ExchangePricesEventArgs(e, CurrencyName.BTC));
            ex.OnEthOrderBook += (s, e) => callback(ex, new ExchangePricesEventArgs(e, CurrencyName.ETH));
            ex.OnLtcOrderBook += (s, e) => callback(ex, new ExchangePricesEventArgs(e, CurrencyName.LTC));
            ex.OnXrpOrderBook += (s, e) => callback(ex, new ExchangePricesEventArgs(e, CurrencyName.XRP));
            ex.OnBchOrderBook += (s, e) => callback(ex, new ExchangePricesEventArgs(e, CurrencyName.BCH));
        }

        public static void SubscribeTrades(BitstampExchange ex, CurrencyName[] btcInstruments, CurrencyName[] usdInstruments, Action<object, TradeEventArgs> callback, string usdToken = "usdt")
        {
            ex.OnBtcTrade += (s, e) => callback(ex, e);
            ex.OnEthTrade += (s, e) => callback(ex, e);
            ex.OnLtcTrade += (s, e) => callback(ex, e);
            ex.OnXrpTrade += (s, e) => callback(ex, e);
            ex.OnBchTrade += (s, e) => callback(ex, e);
        }

        public static void SubscribeTicker(BitstampExchange exch, ServiceEventLogger log)
        {
            exch.TickerTimer.Elapsed += (s, e) =>
            {
                try
                {
                    var dbRepo = (DbRepositoryService)exch.Log.DbRepository;
                    var ticker = exch.Exchange.GetTicker();

                    dbRepo.SaveTicker(new TickerEventArgs
                    {
                        Instrument1 = CurrencyName.BTC,
                        Instrument2 = CurrencyName.USD,
                        Exchange = ExchangeName.Bitstamp,
                        BitstampTicker = ticker
                    });
                }
                catch (Exception ex)
                {
                    log.WriteError(String.Format("[{0}] {1} {2}", ex.GetType(), ex.Message, ex.StackTrace));
                }
            };
        }
    }
}
