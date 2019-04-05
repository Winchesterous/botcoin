using System;
using System.Configuration;

namespace BotCoin.RiskCalculator
{
    internal class EthInstrument : BotCoin.Instruments.EthInstrument
    {
        public EthInstrument() : base("ETHUSD")
        {
            SetCommission(new DataType.Exchange.BitmexInstrumentSettings
            {
                MakerFee = Convert.ToDouble(ConfigurationManager.AppSettings["EthMakerFee"]),
                TakerFee = Convert.ToDouble(ConfigurationManager.AppSettings["EthTakerFee"])
            });
        }
    }
}
