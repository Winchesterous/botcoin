using System;

namespace BotCoin.DataType.Exchange
{
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class ExchangeSettingsData
    {
        public string ExchangeName { set; get; }
        public string CurrencyName { set; get; }
        public string RestUrl { set; get; }
        public string WebsocketUrl { set; get; }
        public string PublicKey { set; get; }
        public string PusherKey { set; get; }
        public string SecretKey { set; get; }
        public string ClientId { set; get; }
        public string CertificateName { set; get; }
        public double TradeFee { set; get; }
        public double MakerFee { set; get; }

        public ExchangeName Exchange
        {
            get { return (ExchangeName)Enum.Parse(typeof(ExchangeName), ExchangeName, true); }
        }

        public CurrencyName Currency
        {
            get { return (CurrencyName)Enum.Parse(typeof(CurrencyName), CurrencyName, true); }
        }
    }
}
