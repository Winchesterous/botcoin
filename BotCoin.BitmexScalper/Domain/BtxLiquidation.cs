//https://www.reddit.com/r/BitMEX/comments/90xxdn/bitmex_cross_liquidation_formula/

namespace BotCoin.BitmexScalper.Domain
{
    internal class BtxLiquidation
    {
        readonly double _takerFee;
        readonly double MaintenanceMargin;

        public BtxLiquidation(double takerFee)
        {
            _takerFee = takerFee;
            MaintenanceMargin = 0.005 - _takerFee;
        }

        private double InitMargin(double leverage)
        {
            return (1 / leverage) - _takerFee * 2;
        }

        public double GetBuyLiqPrice(double leverage, double avgEntryPrice, double fundingRatePcnt)
        {
            var bankrupt = avgEntryPrice / (1 + InitMargin(leverage));
            return bankrupt + avgEntryPrice * (MaintenanceMargin + fundingRatePcnt / 100);
        }

        public double GetSellLiqPrice(double leverage, double avgEntryPrice, double fundingRatePcnt)
        {
            var bankrupt = avgEntryPrice / (1 - InitMargin(leverage));
            return bankrupt - avgEntryPrice * (MaintenanceMargin - fundingRatePcnt / 100);
        }
    }
}
