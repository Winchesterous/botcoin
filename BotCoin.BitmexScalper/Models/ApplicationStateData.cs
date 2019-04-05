using System;
using System.Windows;

namespace BotCoin.BitmexScalper.Models
{
    internal enum ChartType { JapaneseCandle, Bars }

    internal class WindowStateData
    {
        public string WndName { set; get; }
        public double Top { set; get; }
        public double Left { set; get; }
        public double Width { set; get; }
        public double Height { set; get; }
        public WindowState WndState { set; get; }
        public DateTime? PriceChartDate1 { set; get; }
        public DateTime? PriceChartDate2 { set; get; }
        public ChartType Chart { set; get; }
        public int TimePeriodIndex { set; get; }
    }
}
