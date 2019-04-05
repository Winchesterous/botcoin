using BotCoin.DataType.Database;
using System;
using System.Linq;
using System.Windows.Controls;

namespace BotCoin.BitmexScalper
{
    public partial class TradeHistoryControl : UserControl
    {
        public class VwapGain
        {
            public string Time { set; get; }
            public string XBT { set; get; }
            public string ETH { set; get; }
            public string LTC { set; get; }
            public string EOS { set; get; }
            public string XRP { set; get; }
            public string ADA { set; get; }
            public string TRX { set; get; }
            public string BCH { set; get; }
        }

        public TradeHistoryControl()
        {
            InitializeComponent();
        }

        public void InsertPosition(DbPosition position)
        {
            gridPositions.Items.Insert(0, position);
        }

        public void InsertTrade(DbTrade trade)
        {
            gridTrades.Items.Insert(0, trade);            
        }

        public void InsertVwapGain(VwapGain item)
        {
            gridVwaps.Items.Insert(0, item);
        }

        public void ClearVwaps()
        {
            if (gridVwaps.Items.Count > 0)
                gridVwaps.Items.Clear();
        }

        public VwapGain CreateVwapGain(DateTime date, DbIndicatorVwapLite[] vwaps)
        {
            var obj = new TradeHistoryControl.VwapGain { Time = date.ToString("dd MMMM  HH:mm") };
            foreach (var item in vwaps.Where(i => i.Timestamp == date))
            {
                var value = Math.Round(item.VwapGainRatio15Min, 2).ToString();
                if (item.ExtremClosePrice != 0) value = String.Format("{0} ({1})", value, item.ExtremClosePrice);

                if (item.Instrument == "XBTUSD" || 
                    item.Instrument.StartsWith("BTC"))      obj.XBT = value;
                else if (item.Instrument == "ETHUSD")       obj.ETH = value;
                else if (item.Instrument.StartsWith("LTC")) obj.LTC = value;
                else if (item.Instrument.StartsWith("EOS")) obj.EOS = value;
                else if (item.Instrument.StartsWith("XRP")) obj.XRP = value;
                else if (item.Instrument.StartsWith("ADA")) obj.ADA = value;
                else if (item.Instrument.StartsWith("TRX")) obj.TRX = value;
                else if (item.Instrument.StartsWith("BCH")) obj.BCH = value;
            }
            return obj;
        }
    }
}
