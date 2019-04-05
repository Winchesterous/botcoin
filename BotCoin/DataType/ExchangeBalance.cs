using System.Collections.Generic;

namespace BotCoin.DataType
{
    public class ExchangeBalance
    {
        Dictionary<CurrencyName, Account> _accounts = new Dictionary<CurrencyName, Account>();

        public class Account
        {
            public double Value { set; get; }
            public CurrencyName Currency { set; get; }
        }

        public Account GetAccount(CurrencyName currency)
        {
            return _accounts[currency];
        }

        public void AddAccount(Account account, CurrencyName currency)
        {
            _accounts[currency] = account;
        }
    }
}
