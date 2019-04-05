using BotCoin.BitmexScalper.Controllers;
using BotCoin.BitmexScalper.Helpers;
using BotCoin.DataType.Database;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BotCoin.BitmexScalper
{
    public partial class PriceLevelWindow : Window
    {
        readonly PriceLevelController _controller;
        readonly DbPriceLevel NullLevel;
        readonly MainWindow _mainWnd;
        readonly string _timeframe;
        System.Windows.Forms.Timer _timer;
        DbPriceLevel _currentLevel;

        public event EventHandler PriceCreated;
        public event EventHandler PriceRemoved;

        public PriceLevelWindow()
        {
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 60000;
            _timer.Tick += OnTimerTick;

            InitializeComponent();

            NullLevel = new DbPriceLevel { Id = "nil", PriceStr = "<..>" };
            _controller = new PriceLevelController();
        }
                
        public PriceLevelWindow(MainWindow mainWnd, string timeframe) : this()
        {
            _timeframe = timeframe;
            _mainWnd = mainWnd;

            SetDates();            

            lbActiveLevels.Items.Add(NullLevel);
            lbInactiveLevels.Items.Add(NullLevel);                       
        }

        private void SetDates()
        {
            var now = DateTime.Now;
            StartDate = now;
            EndDate = now.AddHours(6);
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (_mainWnd != null)
                _mainWnd.ChangeControl(() => SetDates());
        }

        internal DbPriceLevel PriceLevel
        {
            private set;
            get;
        }

        DateTime StartDate
        {
            set
            {
                dateStart.SelectedDate = value.Date;
                tbStartHour.Text       = value.Hour.ToString("00");
                tbStartMinute.Text     = value.Minute.ToString("00");
            }
            get
            {
                var d = dateStart.SelectedDate.Value;
                return new DateTime(d.Year, d.Month, d.Day, Convert.ToInt32(tbStartHour.Text), Convert.ToInt32(tbStartMinute.Text), 0);
            }
        }

        DateTime EndDate
        {
            set
            {
                dateEnd.SelectedDate = value.Date;
                tbEndHour.Text       = value.Hour.ToString("00");
                tbEndMinute.Text     = value.Minute.ToString("00");
            }
            get
            {
                var d = dateEnd.SelectedDate.Value;
                return new DateTime(d.Year, d.Month, d.Day, Convert.ToInt32(tbEndHour.Text), Convert.ToInt32(tbEndMinute.Text), 0);
            }
        }

        #region Handlers
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var level in _controller.GetPriceLevels())
            {
                if (level.IsActual) lbActiveLevels.Items.Add(level);
                else lbInactiveLevels.Items.Add(level);
            }

            ApplicationState.Load(this);
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            _timer.Stop();
            ApplicationState.Save(this);
            _controller.Logout();
        }

        private void OnAddBreakdownClick(object sender, RoutedEventArgs e)
        {
            if (_currentLevel == null) { MainWindow.Warning("Select active level."); return; }
            var isFalseBreakdown = rbBreakdown.IsChecked.Value;

            var btn = (Button)sender;
            btn.IsEnabled = false;

            _controller.CreateBreakDownAsync(_currentLevel.Id, isFalseBreakdown, StartDate.ToUniversalTime(), _mainWnd, () => btn.IsEnabled = true);
        }

        private void OnAddLevelClick(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(tbPriceLevel.Text)) { MainWindow.Warning("Enter level price."); return; }
            var price = Convert.ToDouble(tbPriceLevel.Text);
            var isLevelUp = rbUpLevel.IsChecked.Value;

            var btn = (Button)sender;
            btn.IsEnabled = false;

            _controller.CreatePriceLevelAsync(price, isLevelUp, _timeframe, StartDate.ToUniversalTime(), EndDate.ToUniversalTime(), _mainWnd, level =>
                {
                    PriceLevel = level;
                    PriceCreated(this, null);
                    lbActiveLevels.Items.Add(level);
                },
                () =>
                {
                    btn.IsEnabled = true;
                    tbPriceLevel.Text = String.Empty;
                });
        }

        private void OnRemoveLevelClick(object sender, RoutedEventArgs e)
        {
            if (_currentLevel == null)
            {
                MainWindow.Warning("Select active level.");
                return;
            }
            if (cbDelete.IsChecked.Value)
            {
                var result = MessageBox.Show("Are you confirm permanently removing price from the database?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                    return;
            }
            var btn = (Button)sender;
            btn.IsEnabled = false;

            _controller.RemovePriceLevelAsync(_currentLevel.Id, _mainWnd, cbDelete.IsChecked.Value, () =>
                {
                    var level = _currentLevel;
                    PriceLevel = _currentLevel;
                    PriceRemoved(this, null);
                    lbActiveLevels.Items.Remove(_currentLevel);
                    if (!cbDelete.IsChecked.Value)
                    {
                        lbInactiveLevels.Items.Add(level);
                    }
                },
                () =>
                {
                    cbDelete.IsChecked = false;
                    btn.IsEnabled = true;
                    _currentLevel = null;
                });
        }

        private void OnRestoreLevelClick(object sender, RoutedEventArgs e)
        {
            if (_currentLevel == null)
            {
                MainWindow.Warning("Select active or inactive level.");
                return;
            }
            var btn = (Button)sender;
            btn.IsEnabled = false;

            _controller.RestorePriceLevelAsync(_currentLevel.Id, _mainWnd, () =>
                {
                    PriceLevel = _currentLevel;
                    PriceCreated(this, null);
                    lbActiveLevels.Items.Add(_currentLevel);
                    lbInactiveLevels.Items.Remove(_currentLevel);
                },
                () =>
                {
                    btn.IsEnabled = true;
                    _currentLevel = null;
                });
        }
                
        private void OnPriceLevelSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                lbLevelDates.Items.Clear();
                _currentLevel = null;
                return;
            }

            _currentLevel = (DbPriceLevel)e.AddedItems[0];
            btnRestoreLevel.IsEnabled = 
            btnRemoveLevel.IsEnabled = true;

            if (_currentLevel.Id == "nil")
            {
                lbLevelDates.Items.Clear();
                btnRestoreLevel.IsEnabled = 
                btnRemoveLevel.IsEnabled = false;
            }
            else
            {
                DbPriceLevel obj = null;
                MainWindow.HandleException(() => obj = _controller.GetPriceLevelById(_currentLevel.Id));

                if (obj != null)
                {
                    lbLevelDates.Items.Clear();

                    foreach (var date in obj.ConfirmedDates)
                        lbLevelDates.Items.Add(date.ToLocalTime().ToString("dd MMM yyyy, HH:mm"));
                }
            }
        }

        private void OnActivateTimerChecked(object sender, RoutedEventArgs e)
        {
            _timer.Start();
        }

        private void OnActivateTimerUnchecked(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }        
        #endregion
    }
}
