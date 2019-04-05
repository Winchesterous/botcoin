using BotCoin.DataType;
using BotCoin.Exchange;
using System;

namespace BotCoin.TradeDataBotService
{
    class Okex
    {
        public static void SubscribeInstruments(OkExExchange ex, CurrencyName[] btcInstruments, CurrencyName[] usdInstruments, Action<object, ExchangePricesEventArgs> callback, string usdToken = "usdt")
        {
            foreach (var ins in btcInstruments)
            {
                ex.SubscribeOrderBook(ins, null, CurrencyName.BTC);
            }
            foreach (var ins in usdInstruments)
            {
                ex.SubscribeOrderBook(ins);
            }
            ex.InstrumentReceived += (s, e) => callback(ex, e);
        }

        public static void SubscribeTrades(OkExExchange ex, CurrencyName[] btcInstruments, CurrencyName[] usdInstruments, Action<object, TradeEventArgs> callback, string usdToken = "usdt")
        {
            foreach (var ins in btcInstruments)
            {
                ex.SubscribeTrade(ins);
            }
            foreach (var ins in usdInstruments)
            {
                ex.SubscribeTrade(ins);
            }
            ex.TradeReceived += (s, e) => callback(ex, e);
        }
    }
}
