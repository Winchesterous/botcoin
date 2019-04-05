using System;

namespace BotCoin.BitmexScalper.Models
{
    internal class EventModel
    {
        public EventModel()
        {
            Time = DateTime.UtcNow;
            TimeStr = Time.ToLocalTime().ToString("HH:mm:ss");
        }

        public string EventType { set; get; }
        public string Message { set; get; }
        public DateTime Time { private set; get; }
        public string TimeStr { private set; get; }
    }
}
