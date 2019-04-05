using BotCoin.BitmexScalper.Controllers;
using BotCoin.BitmexScalper.Helpers;
using BotCoin.BitmexScalper.Models;
using BotCoin.DataType;
using BotCoin.DataType.Database;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ZedGraph;

namespace BotCoin.BitmexScalper
{
    using Charts = Tuple<ChartType, string>;

    public partial class PriceMonitorWindow : Window
    {
        class TimePeriod
        {
            public string Period { set; get; }
            public int PeriodMinutes { set; get; }
        }

        readonly System.Windows.Forms.Timer _chartTimer, _timer;
        readonly PriceMonitorController _controller;
        readonly ZedGraphCrossHair _zedCrossHair;
        readonly MainWindow _mainWnd;
        readonly double XScaleMax = 80;
        readonly int MinPriceOffset = 50;
        readonly int MaxPriceOffset = 50;

        private PriceLevelWindow _levelWnd;
        private DateTime? _currentTime;                
        private int _tickCount;
        //private int _chartTickCount;

        public PriceMonitorWindow()
        {
            InitializeComponent();

            _zedCrossHair = new ZedGraphCrossHair(zedPrice);
            _chartTimer = new System.Windows.Forms.Timer();
            _timer = new System.Windows.Forms.Timer();

            foreach (var item in new TimePeriod[] {
                    new TimePeriod { Period = "1m", PeriodMinutes = 1 },
                    new TimePeriod { Period = "3m", PeriodMinutes = 3 },
                    new TimePeriod { Period = "5m", PeriodMinutes = 5 },
                    new TimePeriod { Period = "15m", PeriodMinutes = 15 },
                    new TimePeriod { Period = "30m", PeriodMinutes = 30 },
                    new TimePeriod { Period = "1h", PeriodMinutes = 60 },
                    new TimePeriod { Period = "2h", PeriodMinutes = 120 },
                    new TimePeriod { Period = "4h", PeriodMinutes = 240 },
                    new TimePeriod { Period = "6h", PeriodMinutes = 360 },
                    new TimePeriod { Period = "12h", PeriodMinutes = 720 },
                    new TimePeriod { Period = "1d", PeriodMinutes = 1440 },
            })
            {
                cbPeriod.Items.Add(item);
            }
            foreach (var item in new Charts[] {
                    new Charts(ChartType.JapaneseCandle, "Candles"),
                    new Charts(ChartType.Bars, "Bars")
            })
            {
                cbChartType.Items.Add(item);
            }
            date1.SelectedDate = CurrentDate;
            date2.SelectedDate = CurrentDate;
        }

        public PriceMonitorWindow(MainWindow wnd) : this()
        {
            _controller = new PriceMonitorController(this);
            _mainWnd = wnd;

            //_chartTimer.Interval = _tickPeriodMinutes * 60000;
            //_timer.Interval = 1000;

            _chartTimer.Tick += (s, e) => DrawPriceChart();
            //_timer.Tick += (s, e) => 
            //{
            //    if (_chartTickCount-- == 0) _chartTickCount = TickPeriodMinutes * 60;
            //    lblTimer.Content = String.Format("00.{0}", _chartTickCount.ToString("00"));
            //};
            InitZedGraphControl();
            Date2 = DateTime.UtcNow.Date.AddDays(-1);
        }

        private void UpdateZedControl()
        {
            zedPrice.AxisChange();
            zedPrice.Invalidate();
        }

        DateTime CurrentDate
        {
            get { return DateTime.UtcNow.Date; }
        }

        ChartType Chart
        {
            set; get;
        }

        DateTime Date1
        {            
            get { return _date1.Value; }
            set
            {
                _date1 = value.Date;
                date1.SelectedDate = _date1;
            }
        }
        DateTime? _date1;

        DateTime Date2
        {
            get { return _date2.Value; }
            set
            {
                _date2 = value.Date;
                date2.SelectedDate = _date2;
            }
        }
        DateTime? _date2;

        private TimePeriod TickPeriodMinutes
        {
            get { return _tickPeriod; }
            set
            {
                _tickPeriod = value;
                _chartTimer.Stop();
                _chartTimer.Interval = _tickPeriod.PeriodMinutes * 60000;
                //_chartTimer.Start();
            }
        }
        TimePeriod _tickPeriod;

        private void InitZedGraphControl(bool yAxisAsUsd = false)
        {
            var pane = zedPrice.GraphPane;

            pane.Legend.IsVisible = false;
            pane.Y2Axis.IsVisible = true;
            pane.X2Axis.IsVisible = true;

            pane.Title.Text                   = "";
            pane.XAxis.Type                   =
            pane.X2Axis.Type                  = AxisType.Date;
            pane.XAxis.Title.Text             =
            pane.X2Axis.Title.Text            = "";
            pane.YAxis.Title.Text             =
            pane.Y2Axis.Title.Text            = "";
            pane.XAxis.Scale.FontSpec.Family  =
            pane.X2Axis.Scale.FontSpec.Family = "Verdana";
            pane.YAxis.Scale.FontSpec.Family  =
            pane.Y2Axis.Scale.FontSpec.Family = "Verdana";
            pane.XAxis.Scale.FontSpec.Size    =
            pane.X2Axis.Scale.FontSpec.Size   = 6;
            pane.YAxis.Scale.FontSpec.Size    =
            pane.Y2Axis.Scale.FontSpec.Size   = 6;
            pane.XAxis.Scale.Format           =
            pane.X2Axis.Scale.Format          = "HH:mm";

            if (yAxisAsUsd)
                pane.YAxis.Scale.Format = "0.00";

            pane.X2Axis.ScaleFormatEvent += ConvertUniversalTimeToLocal;
            DrawGrid();
        }

        private string ConvertUniversalTimeToLocal(GraphPane pane, Axis axis, double val, int index)
        {
            var time = (int)val;
            int hour = time / 60;
            int min  = time - hour * 60;
            return String.Format("{0}:{1}", (hour + DateTimeOffset.Now.Offset.Hours).ToString("00"), min.ToString("00"));
        }

        private void DrawGrid()
        {
            var pane = zedPrice.GraphPane;

            pane.XAxis.MajorGrid.IsVisible   = true;
            pane.XAxis.MajorGrid.DashOff     = 0.5f;
            pane.XAxis.MajorGrid.Color       = Color.LightGray;
            pane.YAxis.MajorGrid.IsVisible   = 
            pane.Y2Axis.MajorGrid.IsVisible  = true;
            pane.YAxis.MajorGrid.DashOff     =
            pane.Y2Axis.MajorGrid.DashOff    = 0.5f;
            pane.YAxis.MajorGrid.Color       = 
            pane.Y2Axis.MajorGrid.Color      = Color.LightGray;
        }

        private void SetPriceAxes(double min, double max)
        {
            var pane = zedPrice.GraphPane;

            pane.YAxis.Scale.MinorStep  =
            pane.Y2Axis.Scale.MinorStep = 2;
            pane.YAxis.Scale.MajorStep  = 
            pane.Y2Axis.Scale.MajorStep = 10;
            pane.YAxis.Scale.Min        = 
            pane.Y2Axis.Scale.Min       = min;
            pane.YAxis.Scale.Max        = 
            pane.Y2Axis.Scale.Max       = max;
        }

        private void DrawPriceLevel(DbPriceLevel level)
        {
            var points = new PointPairList();
            zedPrice.GraphPane.AddCurve(level.Id, points, Color.Red, SymbolType.None);

            var dt1 = level.ConfirmedDates[0];
            var dt2 = level.Date2;

            while (dt1 < dt2)
            {
                points.Add(new XDate(dt1), level.Price);
                dt1 = dt1.AddMinutes(1);
            }
        }

        private void SetJapaneseCandles(StockPointList points)
        {
            if (points == null)
            {
                var candles = zedPrice.GraphPane.CurveList["candle"] as JapaneseCandleStickItem;
                if (candles != null)
                {
                    points = (StockPointList)candles.Points;
                }
                else
                {
                    var bars = zedPrice.GraphPane.CurveList["candle"] as OHLCBarItem;
                    if (bars == null) throw new InvalidOperationException("Other bars doensn't implemented");
                    
                    points = (StockPointList)bars.Points;
                }
            }
            RemovePriceChart();

            var stick = zedPrice.GraphPane.AddJapaneseCandleStick("candle", points).Stick;
            stick.FallingFill = new Fill(Color.Red);
            stick.RisingFill = new Fill(Color.LightGreen);
            stick.IsAutoSize = true;
        }

        private void SetOHLCBars(StockPointList points)
        {
            if (points == null)
            {
                var bars = zedPrice.GraphPane.CurveList["candle"] as OHLCBarItem;
                if (bars != null)
                {
                    points = (StockPointList)bars.Points;
                }
                else
                {
                    var candles = zedPrice.GraphPane.CurveList["candle"] as JapaneseCandleStickItem;
                    if (candles == null) throw new InvalidOperationException("Other bars doensn't implemented");

                    points = (StockPointList)candles.Points;
                }
            }
            RemovePriceChart();
            //
            // https://csharp.hotexamples.com/ru/examples/ZedGraph/GraphPane/AddOHLCBar/php-graphpane-addohlcbar-method-examples.html
            //
            var myFill = new Fill(new Color[] { Color.Red, Color.Black })
            {
                RangeMin = 1,
                RangeMax = 2,
                Type = FillType.GradientByColorValue,
                SecondaryValueGradientColor = Color.Empty
            };

            var items = zedPrice.GraphPane.AddOHLCBar("candle", points, Color.Empty);
            items.Bar.GradientFill = myFill;
            items.Bar.IsAutoSize = true;
            items.Bar.Width = 2;
        }

        private void CreateChart(StockPointList points = null)
        {
            switch (Chart)
            {
            case ChartType.JapaneseCandle:
                {
                    SetJapaneseCandles(points);                    
                    break;
                }
            case ChartType.Bars:
                {                    
                    SetOHLCBars(points);                    
                    break;
                }
            }
        }

        private void InitPriceChart()
        {
            zedPrice.AxisX      = AxisType.DateAsOrdinal;
            zedPrice.MinScaleX  = 0;
            zedPrice.MaxScaleX  = XScaleMax;
            zedPrice.MajorUnitX = zedPrice.MinorUnitX = DateUnit.Minute;

            SetTimeAxisScales();

            zedPrice.PanModifierKeys = System.Windows.Forms.Keys.None;
            zedPrice.AxisChange();
        }

        private void DrawPriceLine(double price)
        {
            var idx = zedPrice.GraphPane.CurveList.IndexOf("price");
            if (idx != -1)
                zedPrice.GraphPane.CurveList.RemoveAt(idx);

            var points = new PointPairList();
            zedPrice.GraphPane.AddCurve("price", points, Color.Green, SymbolType.None);

            var dt1 = Date1;
            var dt2 = Date2.AddDays(1);

            while (dt1 < dt2)
            {
                points.Add(new XDate(dt1), price);
                dt1 = dt1.AddMinutes(1);
            }
            UpdateZedControl();
        }
                
        private void DrawPriceChart(TradeEventArgs e = null)
        {
            var candles = zedPrice.GraphPane.CurveList["candle"];
            if (candles == null) return;

            StockPt point = null;
            var points = (StockPointList)candles.Points;

            if (e == null)
            {
                _currentTime = CurrentDate;

                var date = CurrentDate;
                var c    = _controller.GetBitstampCandle(date, CurrencyName.BTC, TickPeriodMinutes.PeriodMinutes);
                point    = new StockPt(new XDate(date).XLDate, c.High, c.Low, c.Open, c.Close, 0);
            }
            else
            {
                if (points.Count == 0)
                    return;

                var pt = points.Last();
                points.RemoveAt(points.Count - 1);

                if (_currentTime.HasValue)
                    _currentTime = CurrentDate;

                if (e.Price > pt.High) pt.High = e.Price;
                if (e.Price < pt.Low)  pt.Low = e.Price;

                point = new StockPt(new XDate(_currentTime.Value).XLDate, pt.High, pt.Low, pt.Open, e.Price, 0);
            }
                                    
            points.Add(point);
            Scale xScale = zedPrice.GraphPane.XAxis.Scale;

            if (_tickCount > xScale.Max - xScale.MajorStep)
            {
                xScale.Max = _tickCount + xScale.MajorStep;
                xScale.Min = xScale.Max - XScaleMax;
            }
            _tickCount++;

            //var elapsedMinutes = DateTime.UtcNow.Hour * 60 + DateTime.UtcNow.Minute;    // temp!!
            //SetTimeAxis(elapsedMinutes - 30);

            SetPriceAxes(point.High - MinPriceOffset, point.High + MaxPriceOffset);
            DrawPriceLine(point.Close);
            UpdateZedControl();
        }

        private void SetTimeAxis()
        {
            //var dt = Date2 - Date1;
            //var now = DateTime.UtcNow;
            //var elapsedMinutes = dt.TotalMinutes;

            //var pane = zedPrice.GraphPane;
            //pane.XAxis.Scale.Max += elapsedMinutes;
            //pane.XAxis.Scale.Min += elapsedMinutes;
        }

        private void LoadPriceChart(DateTime startTime, int period, DateTime? endTime)
        {
            var points       = new StockPointList();
            double lastPrice = 0;
            double axesPrice = 0;

            var candles = _controller.GetDailyBitstampCandles(startTime, CurrencyName.BTC, period, endTime);
            if (candles.Length > 0)
            {
                lastPrice = candles.Last(x => x.Close > 0).Close;
                axesPrice = candles.First(x => x.Open > 0).Open;
            }
            foreach (var c in candles)
            {
                var xdate = new XDate(c.Time);
                points.Add(new StockPt(xdate, c.High, c.Low, c.Open, c.Close, 0));
            }

            InitPriceChart();
            CreateChart(points);
            SetPriceAxes(lastPrice - MinPriceOffset, lastPrice + MaxPriceOffset);
            SetTimeAxis();
            DrawPriceLine(lastPrice);
            UpdateZedControl();

            //_timer.Start();
        }

        private void DrawVwap(DateTime startDate, int period, DateTime? endDate)
        {
            if (!date1.SelectedDate.HasValue) { MainWindow.Warning("Select start date."); return; }
            if (TickPeriodMinutes == null) { MainWindow.Warning("Period doesn't selected."); return; }

            var points = new PointPairList();
            var date = startDate;

            zedPrice.GraphPane.AddCurve("vwap", points, Color.Blue, SymbolType.None);

            foreach (var ticker in _controller.GetBitstampVwaps(startDate, period, endDate))
            {
                points.Add(new XDate(ticker.Time.Value), ticker.Vwap);
                date = date.AddMinutes(period);
            }
            UpdateZedControl();
        }

        private void RemoveLine(string lineId)
        {
            var idx = zedPrice.GraphPane.CurveList.IndexOf(lineId);
            if (idx != -1)
            {
                zedPrice.GraphPane.CurveList.RemoveAt(idx);
                UpdateZedControl();
            }
        }
                
        private void RemovePriceChart()
        {
            var idx = zedPrice.GraphPane.CurveList.IndexOf("candle");
            if (idx != -1)
            {
                zedPrice.GraphPane.CurveList.RemoveAt(idx);
            }
        }
                
        private void LoadPriceLevels()
        {
            foreach (var item in _controller.GetPriceLevels())
            {
                DrawPriceLevel(item);
            }
            UpdateZedControl();
        }

        private void RefreshPeriod()
        {
            if (!date1.SelectedDate.HasValue)
            {
                MainWindow.Warning("Select start date.");
                cbPeriod.SelectionChanged -= OnPeriodSelected;
                cbPeriod.SelectedItem = null;
                cbPeriod.SelectionChanged += OnPeriodSelected;
                return;
            }
            if (TickPeriodMinutes == null)
                return;

            btnRefreshPeriod.Visibility = Visibility.Hidden;
            cbPeriod.IsEnabled = false;

            var startDate = Date1;
            var endDate = Date2;

            Task.Run(() =>
            {
                LoadPriceChart(startDate, TickPeriodMinutes.PeriodMinutes, endDate);
                _mainWnd.ChangeControl(() =>
                {
                    btnRefreshPeriod.Visibility = Visibility.Visible;
                    cbPeriod.IsEnabled = true;
                });
            });
        }

        private void RedrawLines()
        {
            var zedItemsList = zedPrice.GraphPane.CurveList;
            var removedItems = zedItemsList.Where(i => i.Label.Text != "candle").ToArray();

            foreach (var item in removedItems)
                zedItemsList.Remove(item);

            MainWindow.HandleException(() =>
            {
                if (cbVwap.IsChecked.Value)
                {
                    DrawVwap(Date1, TickPeriodMinutes.PeriodMinutes, Date2);
                }
                LoadPriceLevels();
            });
        }

        #region Handlers
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            int timePeriodIdx = 0;
            try
            {
                _controller.Logon();
                
                var dates = _controller.GetChartDates();
                if (dates != null)
                {
                    date1.DisplayDateStart = dates[0];
                    date2.DisplayDateStart = dates[0];
                    date1.DisplayDateEnd = dates[1];
                    date2.DisplayDateEnd = dates[1];
                }

                ApplicationState.Load(this, (dict, wndName) =>
                {
                    var d = dict[wndName];
                    var dt = d.PriceChartDate1;
                    Date1 = dt.HasValue ? dt.Value : CurrentDate;
                    dt = d.PriceChartDate2;
                    Date2 = dt.HasValue ? dt.Value : CurrentDate;
                    Chart = d.Chart;
                    timePeriodIdx = d.TimePeriodIndex;
                });
            }
            catch (Exception ex)
            {
                MainWindow.Error(ex.Message);
                this.Close();
                return;
            }

            //_chartTimer.Start();
            //_timer.Start();

            cbChartType.SelectionChanged -= OnChartTypeChanged;
            cbChartType.SelectedIndex     = (int)Chart;
            cbChartType.SelectionChanged += OnChartTypeChanged;
            cbPeriod.SelectedIndex        = timePeriodIdx;
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            _chartTimer.Stop();
            _timer.Stop();

            ApplicationState.Save(this, (dict, wndName) =>
            {
                var d = dict[wndName];
                d.PriceChartDate1 = Date1;
                d.PriceChartDate2 = Date2;
                d.TimePeriodIndex = cbPeriod.SelectedIndex;
                d.Chart = Chart;
            });

            _controller.Logout();

            if (_levelWnd != null)
                _levelWnd.Close();
        }

        private void OnVwapCheckClick(object sender, RoutedEventArgs e)
        {
            RemoveLine("vwap");

            if (((CheckBox)sender).IsChecked.Value)
                MainWindow.HandleException(() => DrawVwap(Date1, TickPeriodMinutes.PeriodMinutes, Date2));
        }

        private void SetTimeAxisScales()
        {
            GraphPane pane = zedPrice.GraphPane;

            pane.XAxis.Scale.MinorStep  =
            pane.X2Axis.Scale.MinorStep = 5;// * TickPeriodMinutes;
            pane.XAxis.Scale.MajorStep  =
            pane.X2Axis.Scale.MajorStep = 5;// * TickPeriodMinutes;
        }

        private void OnPeriodSelected(object sender, SelectionChangedEventArgs e)
        {
            TickPeriodMinutes = (TimePeriod)cbPeriod.SelectedItem;

            SetTimeAxisScales();
            RedrawLines();
            RefreshPeriod();
        }
                
        private void OnPeriodRefreshClick(object sender, RoutedEventArgs e)
        {
            RefreshPeriod();
        }

        internal void OnBitstampTradeReceived(object s, TradeEventArgs e)
        {
            //_mainWnd.ChangeControl(() => DrawPriceChart(e));
        }

        private void OnDate1DateChanged(object sender, SelectionChangedEventArgs e)
        {
            Date1 = (DateTime)e.AddedItems[0];
        }

        private void OnDate2DateChanged(object sender, SelectionChangedEventArgs e)
        {
            Date2 = (DateTime)e.AddedItems[0];
        }

        private void OnShowLevelsClick(object sender, RoutedEventArgs e)
        {
            _levelWnd = new PriceLevelWindow(_mainWnd, TickPeriodMinutes.Period);
            _levelWnd.PriceCreated += OnPriceLevelCreated;
            _levelWnd.PriceRemoved += OnPriceLevelRemoved;
            _levelWnd.Show();
        }

        private void OnPriceLevelRemoved(object sender, EventArgs e)
        {
            var level = ((PriceLevelWindow)sender).PriceLevel;
            RemoveLine(level.Id);
        }

        private void OnPriceLevelCreated(object sender, EventArgs e)
        {
            var level = ((PriceLevelWindow)sender).PriceLevel;
            DrawPriceLevel(level);
            UpdateZedControl();
        }

        private void OnChartTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            Chart = ((Charts)e.AddedItems[0]).Item1;
            CreateChart();
            UpdateZedControl();
        }
        #endregion
    }
}
