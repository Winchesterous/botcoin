using System.Configuration;
using System.Windows;

namespace BotCoin.RiskCalculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ltcCtrl.Instrument = new LtcInstrument(ConfigurationManager.AppSettings["LtcContract"]);
            xbtCtrl.Instrument = new XbtInstrument();
            ethCtrl.Instrument = new EthInstrument();
        }
    }
}
