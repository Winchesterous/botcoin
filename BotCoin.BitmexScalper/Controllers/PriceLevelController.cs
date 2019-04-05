using BotCoin.ApiClient;
using BotCoin.DataType.Database;
using BotCoin.DataType.WebApi;
using System;
using System.Threading.Tasks;

namespace BotCoin.BitmexScalper.Controllers
{
    internal class PriceLevelController
    {
        readonly Func<PriceLevelRequest> createLevelRequest;
        readonly Func<LogRequest> createLogRequest;
        readonly RestApiClient2 _apiClient;

        public PriceLevelController()
        {
            _apiClient = new RestApiClient2(MainWindow.Config.Connections.GetElement("WebApi").Url);

            createLevelRequest = () => new PriceLevelRequest(_apiClient);
            createLogRequest = () => new LogRequest(_apiClient);
        }

        public void Logout()
        {
            _apiClient.Dispose();
        }

        public DbPriceLevel[] GetPriceLevels()
        {
            return createLevelRequest().GetPriceLevels(false);
        }

        public DbPriceLevel GetPriceLevelById(string id)
        {
            return createLevelRequest().GetPriceLevelById(id);
        }

        public void RemovePriceLevelAsync(string levelId, MainWindow wnd, bool removeFromDb, Action action, Action finAction)
        {
            Task.Run(() =>
            {
                MainWindow.HandleException(() =>
                {
                    createLevelRequest().RemoveLevel(levelId, removeFromDb);
                    wnd.ChangeControl(() => action());
                },
                () => wnd.ChangeControl(() => finAction()));
            });
        }

        public void RestorePriceLevelAsync(string levelId, MainWindow wnd, Action action, Action finAction)
        {
            Task.Run(() =>
            {
                MainWindow.HandleException(() =>
                {
                    var obj = createLevelRequest().RestoreLevel(levelId);
                    wnd.ChangeControl(() => action());
                },
                () => wnd.ChangeControl(() => finAction()));
            });
        }

        public void CreatePriceLevelAsync(double price, bool isLevelUp, string timeframe, DateTime dt1, DateTime dt2, MainWindow wnd, Action<DbPriceLevel> action, Action finAction)
        {
            Task.Run(() =>
            {
                MainWindow.HandleException(() =>
                {
                    var obj = createLevelRequest().CreateLevel(price, isLevelUp, timeframe, dt1, dt2);
                    wnd.ChangeControl(() => action(obj));
                },
                () => wnd.ChangeControl(() => finAction()));
            });
        }

        public void CreateBreakDownAsync(string levelId, bool isFalseBreakDown, DateTime dt, MainWindow wnd, Action finAction)
        {
            Task.Run(() =>
            {
                MainWindow.HandleException(() =>
                {
                    createLevelRequest().CreateBreakDown(levelId, isFalseBreakDown, dt);
                },
                () => wnd.ChangeControl(() => finAction()));
            });
        }
    }
}