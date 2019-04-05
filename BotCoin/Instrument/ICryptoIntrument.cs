using BotCoin.DataType;
using BotCoin.DataType.Exchange;

namespace BotCoin.Instruments
{
    public interface ICryptoIntrument
    {
        string ToStringPrice(double price);
        string GetSymbol();
        double SetBidPrice(ExchangePricesEventArgs e);
        double SetAskPrice(ExchangePricesEventArgs e);
        double GetBidPrice();
        double GetAskPrice();
        void SetCommission(BitmexInstrumentSettings comm);
        void SetProfit();
        double GetPnl(long qty, double openPrice, double closePrice);
        double ConvertToPrice(long qty, double price);
        string FormatOrderValue(long qty, double price);
        double GetTakerFee();
        double GetMakerFee();
        double TickSize { get; }
    }
}
