using BotCoin.Logger;
using BotCoin.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Timers;

namespace BotCoin.TradeDataBotService
{
    class VwapIndicator
    {
        protected readonly DbRepositoryService _dbRepo;
        readonly Tuple<int, string>[] Periods;
        readonly Dictionary<int, Timer> _initTimers;
        readonly Dictionary<int, Timer> _timers;
        readonly ServiceEventLogger _log;

        Dictionary<int, List<DateTime>> _frames;
        
        public VwapIndicator(ServiceEventLogger log)
        {
            _initTimers = new Dictionary<int, Timer>();
            _timers     = new Dictionary<int, Timer>();
            _frames     = new Dictionary<int, List<DateTime>>();
            _dbRepo     = (DbRepositoryService)log.DbRepository;
            _log        = log;

            Periods = new Tuple<int, string>[]
            {
                new Tuple<int, string>(3, "3m")
                //new Tuple<int, string>(5, "5m"),
                //new Tuple<int, string>(15, "15m"),
                //new Tuple<int, string>(30, "30m"),
                //new Tuple<int, string>(45, "45m"),
                //new Tuple<int, string>(60, "1h"),
                //new Tuple<int, string>(120, "2h"),
                //new Tuple<int, string>(240, "4h"),
                //new Tuple<int, string>(480, "8h")
            };
        }

        protected virtual void OnTimerTick(List<Tuple<DateTime, DateTime>> dates, string periodName, bool httpLoading = true)
        {
        }

        protected void HandleException(Exception ex)
        {
            _log.WriteError(String.Format("[{0}] {1} {2}", ex.GetType(), ex.Message, ex.StackTrace));
        }

        private void CreateFrames(DateTime dt)
        {
            foreach (var p in Periods)
                _frames[p.Item1] = CreateTimeFrames(p.Item1, dt);
        }

        public DateTime Date(int year, int month, int day)
        {
            return new DateTime(year, month, day, 0, 0, 0);
        }

        public DateTime Today
        {
            get
            {
                var now = CurrentTime;
                return new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            }
        }

        protected DateTime CurrentTime
        {
            get { return DateTime.UtcNow; }
        }

        protected virtual string GetExchangeName()
        {
            throw new NotSupportedException();
        }

        private void TimerAction(Tuple<int,string> period, DateTime signalTime)
        {
            lock (_log)
            {
                DateTime time1, time2;
                GetIndex(period.Item1, signalTime, out time1, out time2);

                OnTimerTick(new List<Tuple<DateTime, DateTime>> { new Tuple<DateTime, DateTime>(time1, time2) }, period.Item2);
            }
        }

        public void TimerAction(bool start)
        {
            var syncVwaps = Int32.Parse(ConfigurationManager.AppSettings["SyncVwapsOnLaunch"]);
            if (start)
            {                
                CreateFrames(Today);

                foreach (var p in Periods)
                {
                    int startPeriodIndex;
                    var interval = GetTimerInterval(_frames[p.Item1], out startPeriodIndex);

                    _initTimers[p.Item1] = new Timer(interval);
                    _initTimers[p.Item1].Elapsed += (s, e) =>
                    {
                        _initTimers[p.Item1].Stop();

                        _timers[p.Item1] = new Timer(p.Item1 * 60000);
                        _timers[p.Item1].Elapsed += (s1, e1) => TimerAction(p, e1.SignalTime.ToUniversalTime());
                        _timers[p.Item1].Start();

                        TimerAction(p, e.SignalTime.ToUniversalTime());
                    };

                    if (syncVwaps == 1)
                        Task.Run(() => OnInitTimerTick(_frames[p.Item1][startPeriodIndex], p));
                    else
                        _initTimers[p.Item1].Start();
                }
            }
            else
            {
                foreach (var timer in _timers)
                    timer.Value.Stop();

                foreach (var timer in _initTimers)
                    timer.Value.Stop();
                
                _initTimers.Clear();
                _timers.Clear();
            }
        }

        private void GetIndex(int period, DateTime signalTime, out DateTime time1, out DateTime time2)
        {
            if (signalTime.Hour == 0 && signalTime.Minute == 0)
            {
                if (signalTime.Date > _frames[period][0].Date)  // next day
                {
                    int length = _frames[period].Count;
                    time1 = _frames[period][length - 2];
                    time2 = _frames[period][length - 1];

                    CreateFrames(Date(signalTime.Year, signalTime.Month, signalTime.Day));
                    return;
                }
                throw new InvalidOperationException("GetIndex error 1");
            }
            for (int idx = 0; idx < _frames[period].Count; idx++)
            {
                var dt = _frames[period][idx];
                if (dt.Hour == signalTime.Hour && dt.Minute == signalTime.Minute)
                {
                    time1 = _frames[period][idx - 1];
                    time2 = _frames[period][idx];
                    return;
                }
            }
            throw new InvalidOperationException("GetIndex error 2");
        }

        protected virtual DateTime? GetLastVwapTimePeriod(DateTime time, string periodName)
        {
            throw new NotSupportedException();
        }

        private void OnInitTimerTick(DateTime endTime, Tuple<int, string> period)
        {            
            var startTime = GetLastVwapTimePeriod(Today, period.Item2);

            if (!startTime.HasValue)
                startTime = Today;
            else
                startTime = startTime.Value.AddMinutes(period.Item1);

            ForeachTimePeriod(period.Item1, startTime.Value, endTime, dates => OnTimerTick(dates, period.Item2));
        }

        public void SaveVwapHistory(DateTime date1, DateTime date2)
        {
            var consoleColor = Console.ForegroundColor;

            while (date1 <= date2)
            {
                foreach (var period in Periods)
                {
                    CreateFrames(date1);

                    var time = GetLastVwapTimePeriod(date1, period.Item2);
                    if (time != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(date1);
                        Console.ForegroundColor = consoleColor;
                        date1 = date1.AddDays(1);
                        continue;
                    }
                    time = date1;

                    ForeachTimePeriod(period.Item1, time.Value, time.Value.AddDays(1), dates => OnTimerTick(dates, period.Item2, false));
                    Console.WriteLine(date1);
                }
                date1 = date1.AddDays(1);
            }
        }

        private void ForeachTimePeriod(int period, DateTime startTime, DateTime endTime, Action<List<Tuple<DateTime, DateTime>>> action)
        {
            var dates = new List<Tuple<DateTime, DateTime>>();
            for (int i = 0; i < _frames[period].Count; i++)
            {
                var p = _frames[period][i];

                if (p <= startTime)
                    continue;
                if (p > endTime)
                    break;
                if (i > 0)
                    dates.Add(new Tuple<DateTime, DateTime>(_frames[period][i - 1], p));
            }
            if (dates.Count > 0)
                action(dates);
        }

        private double GetTimerInterval(List<DateTime> frames, out int idx)
        {
            var now = CurrentTime;
            idx = -1;

            for (int i = 0; i < frames.Count; i++)
            {
                if (now < frames[i])
                {
                    idx = i == 0 ? i : i - 1;
                    Console.WriteLine(frames[i] - now);
                    return (frames[i] - now).TotalMilliseconds;
                }
            }
            throw new InvalidOperationException();
        }

        private List<DateTime> CreateTimeFrames(int period, DateTime date)
        {
            var frames = new List<DateTime>();
            for (int i = 0; i < 24 * 60 / period + 1; i++)
            {
                frames.Add(date);
                date = date.AddMinutes(period);
            }
            return frames;
        }        

        protected void UpdateContractsForVwapRatio(string[] contracts, List<string[]> pairs)
        {
            var list = new List<string>(contracts);
            do
            {
                string symbol = null;
                foreach (var s1 in list)
                {
                    foreach (var s2 in list)
                    {
                        if (s1 == s2) continue;
                        pairs.Add(new string[] { s1, s2 });
                    }
                    symbol = s1;
                    break;
                }
                list.Remove(symbol);
            }
            while (list.Count != 1);
        }
    }
}
