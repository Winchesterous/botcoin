using BotCoin.Logger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BotCoin.TradeDataBotService
{
    class BinanceVwapIndicator : VwapIndicator
    {
        public BinanceVwapIndicator(ServiceEventLogger log) : base(log)
        {
        }

        protected override string GetExchangeName()
        {
            return "Binance";
        }

        protected override DateTime? GetLastVwapTimePeriod(DateTime time, string periodName)
        {
            return _dbRepo.GetLastVwapTimePeriod(time, periodName, GetExchangeName());
        }
        
        protected override void OnTimerTick(List<Tuple<DateTime, DateTime>> dates, string periodName, bool httpLoading = true)
        {
            var contracts = new string[] { "BTC", "ETH", "XRP", "LTC", "EOS" };
            var pairs = new List<string[]>();

            pairs.Add(new string[] { "BTC", "USD" });
            pairs.Add(new string[] { "ETH", "USD" });
            pairs.Add(new string[] { "XRP", "USD" });
            pairs.Add(new string[] { "EOS", "USD" });
            pairs.Add(new string[] { "LTC", "USD" });
            pairs.Add(new string[] { "ADA", "USD" });
            pairs.Add(new string[] { "TRX", "USD" });
            pairs.Add(new string[] { "BCHABC", "USD" });
            _dbRepo.SaveVwapIndicator(GetExchangeName(), dates, periodName, pairs);

            pairs.Clear();

            UpdateContractsForVwapRatio(contracts, pairs);
            _dbRepo.SaveVwapRatio(GetExchangeName(), pairs, dates.Select(d => d.Item1).ToArray(), periodName);
        }
    }
}
