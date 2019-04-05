using BotCoin.Instruments;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BotCoin.RiskCalculator
{
    public partial class RiskCalculatorControl : UserControl
    {
        public static readonly DependencyProperty RoundDigitsProperty;

        static RiskCalculatorControl()
        {
            RoundDigitsProperty = DependencyProperty.Register("RoundDigits", typeof(int), typeof(RiskCalculatorControl));
        }

        public RiskCalculatorControl()
        {
            InitializeComponent();
        }

        public int RoundDigits
        {
            set { SetValue(RoundDigitsProperty, value); }
            get { return (int)GetValue(RoundDigitsProperty); }
        }

        public ICryptoIntrument Instrument { set; get; }

        private double GetValue(string value)
        {
            double res;
            if (!Double.TryParse(value, out res))
                return -1;
            return res;
        }

        public double OpenPriceFee
        {
            get { return cbOpenPrice.IsChecked.Value ? Instrument.GetMakerFee() : Instrument.GetTakerFee(); }
        }

        public double StopFee
        {
            get { return cbStop.IsChecked.Value ? Instrument.GetMakerFee() : Instrument.GetTakerFee(); }
        }

        public double ClosePriceFee
        {
            get { return cbClosePrice.IsChecked.Value ? Instrument.GetMakerFee() : Instrument.GetTakerFee(); }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            double priceOpen = 0, stopValue = 0, balance = 0, risk = 0, priceClose = 0;
            int position = 0;
            Action clear = () =>
            {
                if (lblPosition != null)
                    lblPosition.Content = String.Empty;
            };
            Action<double> setContent = realRisk =>
            {
                lblPosition.Content = position.ToString();
            };
            if (tbOpenPrice != null)
            {
                priceOpen = GetValue(tbOpenPrice.Text);
                if (priceOpen == -1) { clear(); return; }
            }
            if (tbStopValue != null)
            {
                stopValue = GetValue(tbStopValue.Text);
                if (stopValue == -1) { clear(); return; }
            }
            if (tbRisk != null)
            {
                risk = GetValue(tbRisk.Text);
                if (risk == -1) { clear(); return; }
            }
            if (tbBalance != null)
            {
                balance = GetValue(tbBalance.Text);
                if (balance == -1) { clear(); return; }
            }
            if (priceOpen == 0 || stopValue == 0 || risk == 0 || balance == 0)
            {
                clear();
                return;
            }
            position = GetPosition(priceOpen, stopValue, risk, balance);
            if (position == 0)
            {
                return;
            }
            for ( ; ; )
            {
                var realRisk = GetRealRisk(position, priceOpen, stopValue, risk, balance);
                if (Math.Abs(realRisk - risk) >= 0.01)
                {
                    if (realRisk > risk)
                        position -= 10;
                    else
                        position += 10;
                    continue;
                }
                setContent(realRisk);
                break;
            }
            if (tbClosePrice != null)
            {
                priceClose = GetValue(tbClosePrice.Text);
                if (priceClose == -1)
                {
                    lblProfitXbt.Content = string.Empty;
                }
                else
                    lblProfitXbt.Content = GetProfit(position, priceOpen, priceClose).ToString();
            }
        }

        private double GetProfit(long size, double priceOpen, double priceClose)
        {
            var posProfit = priceOpen <= priceClose ? Instrument.GetPnl(size, priceOpen, priceClose) : Instrument.GetPnl(size, priceClose, priceOpen); 
            var openPos   = Instrument.ConvertToPrice(size, priceOpen);
            var closePos  = openPos + posProfit;
            var fee1      = openPos * OpenPriceFee;
            var fee2      = closePos * ClosePriceFee;
            var profit    = posProfit + fee1 + fee2;
            return Math.Round(profit, RoundDigits);
        }

        private int GetPosition(double price, double stopValue, double risk, double balance)
        {
            var a1  = -1 * risk * balance;
            var a2  = 100 * Instrument.GetPnl(1, price + stopValue, price);
            var res = Math.Round(a1 / a2, 1);
            return (int)res;
        }

        private double GetRealRisk(int position, double price, double stopValue, double risk, double balance)
        {
            var posProfit = Instrument.GetPnl(position, price, price - stopValue);
            var openPos   = Instrument.ConvertToPrice(position, price);
            var closePos  = openPos + posProfit;
            var fee1      = openPos * OpenPriceFee;
            var fee2      = closePos * StopFee;
            var profit    = posProfit + fee1 + fee2;
            return Math.Round(profit / balance * 100, 3) * -1;
        }

        private void OnCheckBoxClick(object sender, RoutedEventArgs e)
        {
            OnTextChanged(null, null);
        }
    }
}
