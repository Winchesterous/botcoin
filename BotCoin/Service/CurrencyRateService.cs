using BotCoin.ApiClient;
using BotCoin.DataType;
using Newtonsoft.Json;
using System.Linq;
using System;
using System.Collections.Generic;

namespace BotCoin.Service
{
    public class CurrencyRateService
    {
        readonly RestApiClient _uahCurrencyService;
        readonly RestApiClient _currencyService;

        public CurrencyRateService()
        {
            _uahCurrencyService = new RestApiClient("https://bank.gov.ua/NBUStatService/v1/", "bankgovua.crt");
            _currencyService = new RestApiClient("https://api.fixer.io/", "fixerio.crt");
        }

        public CurrencyRate[] GetRates(params CurrencyName[] currencies)
        {
            var result = new List<CurrencyRate>();
            var date = DateTime.Now.Date;
            var uri = String.Format("statdirectory/exchange?date={0:0000}{1:00}{2:00}&json", date.Year, date.Month, date.Day);

            var json = _uahCurrencyService.GetQuery(_uahCurrencyService.BaseUri + uri);
            var rates = JsonConvert.DeserializeObject<UahCurrencyRate[]>(json);
            result.Add(new CurrencyRate { Currency = CurrencyName.UAH, Rate = rates.Where(r => r.Cc == "USD").Select(r => r.Rate).Single() });

            //json = _currencyService.GetQuery(_currencyService.BaseUri + "latest?base=USD");
            //var currency = JsonConvert.DeserializeObject<ForeignCurrencyRate>(json);

            //result.Add(new CurrencyRate { Currency = CurrencyName.CAD, Rate = currency.Rates.CAD });
            //result.Add(new CurrencyRate { Currency = CurrencyName.JPY, Rate = currency.Rates.JPY });
            //result.Add(new CurrencyRate { Currency = CurrencyName.PLN, Rate = currency.Rates.PLN });

            return result.ToArray();
        }
    }
}
