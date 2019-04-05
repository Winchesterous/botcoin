namespace BotCoin.DataType.Database
{
    public class DbPositionState
    {
        public int Id { set; get; }
        public string StopOrderId { set; get; }
        public double? StopLoss { set; get; }
        public double? StopSlip { set; get; }
        public double? StopPrice { set; get; }
        public double? StartWatchPrice { set; get; }
        public int? LongPosition { set; get; }
        public short Opened { set; get; }
        public string StateName { set; get; }
        public string HostName { set; get; }
        public string Instrument { set; get; }
        public long? OrderQty { set; get; }
    }
}
