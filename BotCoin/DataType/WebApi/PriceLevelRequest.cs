using BotCoin.ApiClient;
using BotCoin.DataType.Database;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace BotCoin.DataType.WebApi
{
    public class PriceLevelRequest
    {
        [JsonIgnore]
        readonly RestApiClient2 _api;

        [JsonProperty("id")]
        public string LevelId { set; get; }
        [JsonProperty("timeframe")]
        public string Timeframe { set; get; }
        [JsonProperty("price")]
        public double Price { set; get; }
        [JsonProperty("date_from")]
        public DateTime Date1 { set; get; }
        [JsonProperty("date_to")]
        public DateTime Date2 { set; get; }
        [JsonProperty("real_remove")]
        public bool RealRemove { set; get; }
        [JsonProperty("level_up")]
        public bool IsLevelUp { set; get; }
        [JsonProperty("false_breakdown")]
        public bool IsFalseBreakdown { set; get; }

        public PriceLevelRequest(RestApiClient2 api)
        {
            _api = api;
        }

        public DbPriceLevel[] GetPriceLevels(bool onlyActive = true)
        {
            var json = _api.GetQuery("/v1/pricelevels", null, "only_active=" + onlyActive);
            return JsonConvert.DeserializeObject<DbPriceLevel[]>(json);
        }

        public DbPriceLevel CreateLevel(double price, bool isLevelUp, string timeframe, DateTime dt1, DateTime dt2)
        {
            Timeframe = timeframe;
            IsLevelUp = isLevelUp;
            Price = price;
            Date1 = dt1;
            Date2 = dt2;

            var json = _api.UserQuery("/v1/pricelevel", HttpMethod.Post, JsonConvert.SerializeObject(this), true);
            return JsonConvert.DeserializeObject<DbPriceLevel>(json.Content);
        }

        public void CreateBreakDown(string levelId, bool isFalseBreakdown, DateTime dt)
        {
            IsFalseBreakdown = isFalseBreakdown;
            LevelId = levelId;
            Date1 = dt;

            _api.UserQuery("/v1/breakdown", HttpMethod.Put, JsonConvert.SerializeObject(this), true);
        }

        public DbPriceLevel RestoreLevel(string levelId)
        {
            LevelId = levelId;

            var json = _api.UserQuery("/v1/pricelevel", HttpMethod.Put, JsonConvert.SerializeObject(this), true);
            return JsonConvert.DeserializeObject<DbPriceLevel>(json.Content);
        }

        public void RemoveLevel(string id, bool removeFromDb)
        {
            RealRemove = removeFromDb;
            LevelId = id;

            _api.UserQuery("/v1/pricelevel", HttpMethod.Delete, JsonConvert.SerializeObject(this), true);
        }

        public DbPriceLevel GetPriceLevelById(string id)
        {
            var json = _api.GetQuery("/v1/pricelevel", null, "level_id=" + id);
            return JsonConvert.DeserializeObject<DbPriceLevel>(json);
        }
    }
}
