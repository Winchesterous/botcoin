using System;
using System.Configuration;

namespace BotCoin.RiskCalculator
{
    internal class LtcInstrument : BotCoin.Instruments.LtcInstrument
    {
        public LtcInstrument(string instrument) : base(instrument)
        {
            SetCommission(new DataType.Exchange.BitmexInstrumentSettings
            {
                MakerFee = Convert.ToDouble(ConfigurationManager.AppSettings["LtcMakerFee"]),
                TakerFee = Convert.ToDouble(ConfigurationManager.AppSettings["LtcTakerFee"])
            });
        }
    }
}
