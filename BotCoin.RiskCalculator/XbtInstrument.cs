using System;
using System.Configuration;

namespace BotCoin.RiskCalculator
{
    internal class XbtInstrument : BotCoin.Instruments.XbtInstrument
    {
        public XbtInstrument() : base("XBTUSD")
        {
            SetCommission(new DataType.Exchange.BitmexInstrumentSettings
            {
                MakerFee = Convert.ToDouble(ConfigurationManager.AppSettings["XbtMakerFee"]),
                TakerFee = Convert.ToDouble(ConfigurationManager.AppSettings["XbtTakerFee"])
            });
        }
    }
}
