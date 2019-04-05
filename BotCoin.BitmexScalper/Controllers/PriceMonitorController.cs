using BotCoin.ApiClient;
using BotCoin.BitmexScalper.Models;
using BotCoin.DataType;
using BotCoin.DataType.Database;
using BotCoin.DataType.Exchange;
using BotCoin.DataType.WebApi;
using BotCoin.Exchange;
using BotCoin.Logger;
using System;

namespace BotCoin.BitmexScalper.Controllers
{
    internal class PriceMonitorController : IDisposable
    {
        readonly RestApiClient2 _apiClient;
        readonly PriceMonitorWindow _wnd;
        BitstampExchange _bts;

        Func<PriceLevelRequest> createLevelRequest;
        Func<ChartRequest> createChartRequest;
        Func<LogRequest> createLogRequest;

        public PriceMonitorController(PriceMonitorWindow wnd)
        {
            _apiClient = new RestApiClient2(MainWindow.Config.Connections.GetElement("WebApi").Url);
            _wnd = wnd;
        }

        public void LogEvent(EventModel model)
        {
            createLogRequest().WriteScalperLog(((RestServiceEventLogger)_bts.Log).SessionId, model.Time, model.EventType, model.Message);
        }

        public void Logon()
        {
            _bts = new BitstampExchange(SettingRequest.Get(_apiClient, "Bitstamp"));
            _bts.Log = new RestServiceEventLogger(_apiClient, ServiceName.Desktop);
            _bts.OnBtcTrade += _wnd.OnBitstampTradeReceived;

            createLevelRequest = () => new PriceLevelRequest(_apiClient);
            createChartRequest = () => new ChartRequest(_apiClient);
            createLogRequest = () => new LogRequest(_apiClient);

            try
            {
                _bts.Logon();
            }
            catch (Exception ex)
            {
                _bts.Log.WriteError(String.Format("[{0}] {1} {2}", ex.GetType(), ex.Message, ex.StackTrace));
                throw ex;
            }
        }

        public void Logout()
        {
            if (_bts == null) return;
            try
            {
                _bts.OnBtcTrade -= _wnd.OnBitstampTradeReceived;
                _bts.Logout();
            }
            catch (Exception ex)
            {
                _bts.Log.WriteError(String.Format("[{0}] {1} {2}", ex.GetType(), ex.Message, ex.StackTrace));
                throw ex;
            }
        }

        public void Dispose()
        {
            Logout();
            _apiClient.Dispose();
        }

        public DbPriceLevel[] GetPriceLevels()
        {
            return createLevelRequest().GetPriceLevels();
        }

        public BitstampCandle[] GetDailyBitstampCandles(DateTime dtStart, CurrencyName instrument, int periodMinutes, DateTime? dtEnd = null)
        {
            return createChartRequest().GetDailyBitstampCandles(dtStart, instrument.ToString(), periodMinutes, dtEnd);
        }

        public BitstampCandle GetBitstampCandle(DateTime date, CurrencyName instrument, int tickPeriodSec)
        {
            return createChartRequest().GetBitstampCandle(date, instrument.ToString(), tickPeriodSec);
        }

        public BitstampTicker[] GetBitstampVwaps(DateTime date1, int period, DateTime? date2 = null)
        {
            return createChartRequest().GetBitstampVwaps(date1, period, date2);
        }

        public DateTime[] GetChartDates()
        {
            return createChartRequest().GetChartDates();
        }
    }
}