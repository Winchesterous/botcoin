namespace BotCoin.DataType
{
    public enum ExchangeName
    {
        Undefined, Kuna, Bitstamp, Bitbay, Cex, Quadriga,
        Bitfinex, Kraken, Bittrex, Binance, Wex,
        HitBtc, XBtce, Btcc, Liqui, OkEx, Bitmex,
        Gdax, BitmexTest
    }

    public enum OrderState
    {
        Undefined, Created, Updated, Deleted
    }

    public enum CurrencyName
    {
        Undefined,
        BTC, BCH, LTC, ETH, XRP, DSH, EOS, STORM, IOTA, VET,
        TRX, ADA, NEO, XLM, XVG, QTUM, XEM, STEEM, BCC, DASH, ZIL, 
        ONT, ICX, ZRX, ENJ, NULS, BCHSV, BCHABC, BTT,
        BNB, FET, WAVES, KNC, RVN, BAT, WAN, PHX,
        USD = 100, UAH, JPY, CAD, PLN, USDT
    }

    public enum TickDirection
    {
        Undefined, MinusTick, PlusTick, ZeroMinusTick, ZeroPlusTick
    }

    public enum TradingState
    {
        Fail1, Fail2, Fail12, Reject, Negative, NoPrice, NoBalance, NoUsd, NoCrypt, NoProfit, Ok, Back
    }

    public enum ServiceName
    {
        Undefined, TradeDataBot, Arbitrage, WebApi, Desktop, InitService, BitmexBot
    }

    public enum ServiceEventType
    {
        Undefined, Info, Fail, Warn
    }

    public enum OrderSide
    {
        ASK, BID, Both
    }

    public enum TradeOrderType
    {
        Market = 1, Limit, StopLoss, StopLossLimit, TakeProfit, TakeProfitLimit, LimitMarket
    }
}
