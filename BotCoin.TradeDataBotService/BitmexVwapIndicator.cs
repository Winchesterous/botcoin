using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using BotCoin.Exchange;
using BotCoin.Logger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BotCoin.TradeDataBotService
{
    class BitmexVwapIndicator : VwapIndicator
    {
        readonly BitmexExchange _bitmex;
        readonly List<string> _contracts;

        public BitmexVwapIndicator(BitmexExchange ex, List<string> contracts, ServiceEventLogger log) : base(log)
        {
            _contracts = contracts;
            _bitmex = ex;
        }

        protected override string GetExchangeName()
        {
            return "Bitmex";
        }

        protected override DateTime? GetLastVwapTimePeriod(DateTime time, string periodName)
        {
            return _dbRepo.GetLastVwapTimePeriod(time, periodName, GetExchangeName());
        }

        public void LoadTrades()
        {
            var df   = new DateTimeFormatInfo { LongDatePattern = "yyyy-MM-ddDHH:mm:ss.ffffff" };
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BitmexData");

            Directory.SetCurrentDirectory(path);

            foreach (var file in new DirectoryInfo(path).GetFiles("*.csv"))
            {
                Console.Write("Loading {0}... ", file.Name);

                var trades = new List<BitmexTradeData>();
                var lines = File.ReadAllLines(file.Name);

                for (int i = 1; i < lines.Length; i++)
                {
                    var row = lines[i].Split(',');
                    trades.Add(new BitmexTradeData
                    {
                        TickDirection = row[5],
                        GrossValue    = Convert.ToInt64(row[7]),
                        Timestamp     = DateTime.ParseExact(row[0].Substring(0, row[0].Length - 3), "D", df),                        
                        Size          = Convert.ToInt64(row[3]),
                        Price         = Convert.ToDouble(row[4]),
                        Symbol        = row[1],
                        Side          = row[2]
                    });
                }
                _dbRepo.SaveTrade(new TradeEventArgs(new BitmexEventArgs { BtxTrades = trades.ToArray() }));
                Console.WriteLine("done");
                File.Move(file.Name, Path.Combine("Done", file.Name));

                var date = Date(trades[0].Timestamp.Year, trades[0].Timestamp.Month, trades[0].Timestamp.Day);
                SaveVwapHistory(date, date);
            }
        }

        protected override void OnTimerTick(List<Tuple<DateTime, DateTime>> dates, string periodName, bool httpLoading = true)
        {
            var pairs = new List<string[]>();
            string[] contracts = null;
                        
            if (httpLoading)
            {
                contracts = _contracts.Where(c => !(c.StartsWith("ETH") && Char.IsDigit(c.Last()))) // ignore ETH futures
                                      .Where(c => !(c.StartsWith("XBT") && Char.IsDigit(c.Last()))) // ignore BTC futures
                                      .ToArray();

                foreach (var contract in contracts)
                {
                    foreach (var date in dates)
                    {
                        try
                        {
                            BitmexTradeData[] trades = null;
                            int startPoint = 0, pageSize = 500;
                            do
                            {
                                trades = _bitmex.Exchange.GetTrades(contract, date.Item1, date.Item2, pageSize, startPoint);

                                System.Threading.Thread.Sleep(1000);
                                _dbRepo.SaveTrade(new TradeEventArgs(new BitmexEventArgs { BtxTrades = trades }));

                                startPoint += pageSize;
                            }
                            while (trades.Length == pageSize);
                        }
                        catch (Exception ex)
                        {
                            HandleException(ex);
                            System.Threading.Thread.Sleep(2000);
                        }
                    }
                    pairs.Add(new string[] { contract, null });
                }
            }
            else
            {
                contracts = _contracts.ToArray();

                foreach (var contract in _contracts)
                    pairs.Add(new string[] { contract, null });
            }

            if (pairs.Count == 0)
                return;

            _dbRepo.SaveVwapIndicator(GetExchangeName(), dates, periodName, pairs);
            pairs.Clear();

            UpdateContractsForVwapRatio(contracts, pairs);
            _dbRepo.SaveVwapRatio(GetExchangeName(), pairs, dates.Select(d => d.Item1).ToArray(), periodName);
        }
    }
}
