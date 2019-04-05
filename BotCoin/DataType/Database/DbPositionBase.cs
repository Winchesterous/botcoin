using System;

namespace BotCoin.DataType.Database
{
    public class DbPositionBase
    {        
        public long OrderQty { set; get; }
        public string PositionId { set; get; }
        public string AccountId { set; get; }
        public string Instrument { set; get; }

        public string ShortPositionId
        {
            get { return PositionId.Split('-')[0]; }
        }        
        protected string ToLocalTime(DateTime time)
        {
            // from UTC to local time
            return (time + DateTimeOffset.Now.Offset).ToString("dd MMM, HH:mm:ss");
        }
    }
}
