using BotCoin.ApiClient;
using BotCoin.Core;
using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using System;
using System.Linq;

namespace BotCoin.Exchange
{
    [Exchange(Name = "HitBtc")]
    public class HitBtcRestExchange : BaseRestExchange
    {
        readonly HitBtcClient _client;

        public HitBtcRestExchange(ExchangeSettingsData setting) : base(setting)
        {
            _client = new HitBtcClient(setting);
        }

        public override ExchangeName GetExchangeName()
        {
            return ExchangeName.HitBtc;
        }

        public override UserAccount GetBalances()
        {
            HitBtcAccount[] account = null;
            Func<string, double> getValue = symbol => account.Where(b => String.Compare(b.Currency, symbol, true) == 0).Single().Available;

            account = _client.GetBalances();
            return new UserAccount
            {
                Exchange    = ExchangeName.HitBtc,
                Balance     = getValue("USD"),
                BtcBalance  = getValue("BTC"),
                EthBalance  = getValue("ETH"),
                BchBalance  = getValue("BCH"),
                LtcBalance  = getValue("LTC"),
                XrpBalance  = getValue("XRP"),
                DashBalance = getValue("DASH")
            };
        }

        public override bool CancelOrder(string orderId, string symbol = null)
        {
            var result = _client.CancelOrder(orderId);
            throw new NotImplementedException();
        }
    }
}
