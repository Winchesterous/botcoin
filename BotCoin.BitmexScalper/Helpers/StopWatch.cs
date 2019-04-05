using System;
using System.Diagnostics;

namespace BotCoin.BitmexScalper.Helpers
{
    static class StopWatch
    {
        public static long Get(Action action)
        {
            var sw = new Stopwatch();
            sw.Start();
            action();
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }
    }
}
