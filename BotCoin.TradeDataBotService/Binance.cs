using BotCoin.DataType;
using BotCoin.Exchange;
using System;
using System.Text;

namespace BotCoin.TradeDataBotService
{
    class Binance
    {
        public static void SubscribeInstruments(BinanceExchange ex, CurrencyName[] btcInstruments, CurrencyName[] usdInstruments, Action<object, ExchangePricesEventArgs> callback, string usdToken = "usdt")
        {
            var str = new StringBuilder(ex.WsUrl);
            foreach (var ins in btcInstruments)
            {
                str.AppendFormat("{0}btc@depth20/", ins.ToString().ToLower());
            }
            foreach (var ins in usdInstruments)
            {
                str.AppendFormat("{0}{1}@depth20/", ins.ToString().ToLower(), usdToken);
            }
            ex.WsUrl = str.ToString();
            ex.InstrumentReceived += (s, e) => callback(ex, e);
        }

        public static void SubscribeTrades(BinanceExchange ex, CurrencyName[] btcInstruments, CurrencyName[] usdInstruments, Action<object, TradeEventArgs> callback, string usdToken = "usdt")
        {
            var str = new StringBuilder(ex.WsUrl);
            foreach (var ins in btcInstruments)
            {
                str.AppendFormat("{0}btc@trade/", ins.ToString().ToLower());
            }
            foreach (var ins in usdInstruments)
            {
                str.AppendFormat("{0}{1}@trade/", ins.ToString().ToLower(), usdToken);
            }
            ex.WsUrl = str.ToString();
            ex.TradeReceived += (s, e) => callback(ex, e);
        }

        public static void SubscribeTicker(BinanceExchange ex, CurrencyName[] btcInstruments, CurrencyName[] usdInstruments, Action<object, TickerEventArgs> callback, string usdToken = "usdt")
        {
            var str = new StringBuilder(ex.WsUrl);
            foreach (var ins in btcInstruments)
            {
                str.AppendFormat("{0}btc@ticker/", ins.ToString().ToLower());
            }
            foreach (var ins in usdInstruments)
            {
                str.AppendFormat("{0}{1}@ticker/", ins.ToString().ToLower(), usdToken);
            }
            ex.WsUrl = str.ToString();
            ex.TickerReceived += (s, e) => callback(ex, e);
        }        
    }
}
