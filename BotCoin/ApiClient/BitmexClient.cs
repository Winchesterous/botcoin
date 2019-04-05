using BotCoin.DataType;
using BotCoin.DataType.Exchange;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;

namespace BotCoin.ApiClient
{
    public class BitmexClient : RestApiClient
    {
        public BitmexClient(ExchangeSettingsData setting) : base(setting)
        {
        }

        private int XRateLimitReset { set; get; }
        public int XRateLimit { private set; get; }
        public int XRateLimitRemaining { private set; get; }
        public int RetryAfterSeconds { private set; get; }

        private long GetNonce()
        {
            int expirationPeriod = 10000;
            return DateTimeOffset.UtcNow.AddMilliseconds(expirationPeriod).ToUnixTimeSeconds();
        }

        Dictionary<string, string> CreateParameters(HttpMethod method, string function, string paramData, out string url)
        {
            url = "/api/v1" + function + ((method == HttpMethod.Get && !String.IsNullOrEmpty(paramData)) ? "?" + paramData : "");
            string postData = (method != HttpMethod.Get) ? paramData : "";

            string expires = GetNonce().ToString();
            string message = string.Format("{0}{1}{2}{3}", method.ToString(), url, expires, postData);

            return new Dictionary<string, string>
            {
                {"api-expires", expires},
                {"api-key", PublicKey},
                {"api-signature", CreateSignature256(message)}
            };
        }

        public BitmexTradeData[] GetTrades(string symbol, DateTime time1, DateTime time2, int count, int startPoint)
        {
            var args = new Dictionary<string, string>();
            BitmexTradeData[] data = null;

            args["columns"] = "timestamp,symbol,side,size,price,tickDirection,grossValue";
            args["symbol"] = symbol;
            args["reverse"] = "false";
            args["count"] = count.ToString();
            args["start"] = startPoint.ToString();
            args["startTime"] = time1.ToString();
            args["endTime"] = time2.ToString();

            var strArgs = UrlEncode(args);
            var json = GetQuery("/api/v1/trade?" + strArgs);            
            try
            {
                data = JsonConvert.DeserializeObject<BitmexTradeData[]>(json);
            }
            catch (Exception ex)
            {
                var msg = string.Format("[Bitmex] GetTrades error: {0}. {1} ({2})", ex.Message, json, strArgs);
                throw new InvalidOperationException(msg);
            }
            return data;
        }

        public BitmexOrderBookL2Data[] GetOrderBook(string symbol, int depth = 1)
        {
            var json = GetQuery(String.Format("/api/v1/orderBook/L2?symbol={0}&depth={1}", symbol, depth));
            return JsonConvert.DeserializeObject<BitmexOrderBookL2Data[]>(json);
        }

        public string CreateAuthWebsocketRequest()
        {
            var nonce = GetNonce();
            var signature = CreateSignature256("GET/realtime" + nonce);

            return JsonConvert.SerializeObject(new BitmexWsCommand
            {
                Command = "authKeyExpires",
                Agruments = new object[] { PublicKey, nonce, signature }
            });
        }

        public BitmexUser GetAccount()
        {
            string url;
            var args = CreateParameters(HttpMethod.Get, "/user", "", out url);
            var query = UserQuery(url, HttpMethod.Get, args);

            return JsonConvert.DeserializeObject<BitmexUser>(query.Content);
        }

        public BitmexInstrumentSettings[] GetInstrumentSettings()
        {
            string url;
            var param = "columns=symbol%2CtickSize%2CmakerFee%2CtakerFee%2CsettlementFee&start=0&count=500";
            var args  = CreateParameters(HttpMethod.Get, "/instrument", param, out url);
            var query = UserQuery(url, HttpMethod.Get, args);
            var instruments = JsonConvert.DeserializeObject<BitmexInstrumentSettings[]>(query.Content);
            int idx         = 0;

            foreach (var inst in instruments)
            {
                BitmexInstrumentSettings item = null;
                for (; idx < instruments.Length; idx++)
                {
                    if (instruments[idx].Symbol == inst.Symbol)
                    {
                        item = instruments[idx];
                        break;
                    }
                }
                if (item == null)
                    throw new InvalidOperationException(String.Format("Instrument '{0}' wasn't found", inst.Symbol));

                inst.Index = idx;
            }
            return instruments;
        }

        public BitmexOrderData[] CancelOrders(string[] ids)
        {
            var args = HttpUtility.UrlEncode(String.Join(",", ids));
            return CancelOrder(args);
        }

        private bool ContainsError(ApiResult response)
        {
            return response.Content.Contains("\"HTTPError\"") ||
                   response.Content.Contains("\"ValidationError\"") ||
                   response.Content.Contains("\"WebsocketError\"") ||
                   response.Content.Contains("\"Error\"");
        }

        public BitmexOrderData[] CancelOrder(string id)
        {
            string url;
            var content = "orderID=" + id;
            var headers = CreateParameters(HttpMethod.Delete, "/order", content, out url);
            var response = UserQuery(url, HttpMethod.Delete, headers, content, false);

            if (ContainsError(response))
            {
                return new BitmexOrderData[]
                {
                    new BitmexOrderData { Error = JsonConvert.DeserializeObject<BitmexResponse>(response.Content).Error.Message }
                };
            }
            return JsonConvert.DeserializeObject<BitmexOrderData[]>(response.Content);
        }

        public bool CancelAllOrders(string symbol)
        {
            string url;

            var postData = "symbol=" + symbol;
            var param    = CreateParameters(HttpMethod.Delete, "/order/all", postData, out url);
            var result   = UserQuery(url, HttpMethod.Delete, param, postData);

            return result.StatusCode == (int)HttpStatusCode.OK;
        }

        public void SetLeverage(double value, string symbol)
        {
            string url;

            var postData = JsonConvert.SerializeObject(new { symbol = symbol, leverage = value.ToString("0.00") });
            var args = CreateParameters(HttpMethod.Post, "/position/leverage", postData, out url);
            var response = UserQuery(url, HttpMethod.Post, args, postData, true);

            JsonConvert.DeserializeObject<BitmexResponse>(response.Content);
        }

        public BitmexOrderData CreateLimitOrder(string symbol, string side, long qty, double price, bool reduceOnly, string text)
        {
            var param = new Dictionary<string, string>();
            var execInst = "ParticipateDoNotInitiate";      // Post-Only

            param["timeInForce"] = "GoodTillCancel";
            param["ordType"]     = "Limit";
            param["price"]       = price.ToString();
            param["text"]        = reduceOnly ? null : text;
            
            if (reduceOnly)
                execInst = execInst + ",ReduceOnly";

            param["execInst"] = execInst;

            return PostOrder(param, side, qty, symbol);
        }

        public BitmexOrderData CreateMarketOrder(string symbol, string side, long qty, string text)
        {
            var param = new Dictionary<string, string>();

            param["ordType"] = "Market";
            param["text"]    = text;

            return PostOrder(param, side, qty, symbol);
        }

        public BitmexOrderData CreateCloseMarketOrder(string symbol, string side, long? qty)
        {
            var param = new Dictionary<string, string>();

            param["ordType"]  = "Market";
            param["execInst"] = "Close";

            return PostOrder(param, side, qty, symbol);
        }

        public BitmexOrderData CreateTrailingStopOrder(string symbol, string side, long qty, double offsetValue)
        {
            var param = new Dictionary<string, string>();

            param["pegOffsetValue"] = offsetValue.ToString();
            param["pegPriceType"]   = "TrailingStopPeg";
            param["ordType"]        = "Stop";
            param["execInst"]       = "Close,LastPrice";

            return PostOrder(param, side, qty, symbol);
        }

        public BitmexOrderData CreateStopLimitOrder(string symbol, string side, int qty, double price, double stopPrice, bool closeOnTrigger, string text)
        {
            var param = new Dictionary<string, string>();
            var execInst = "ParticipateDoNotInitiate,LastPrice";

            if (String.Compare(side, "Buy", true) == 0)
            {
                if (price < stopPrice)
                    throw new ArgumentException("StopLimit: price < stopPrice");
            }
            else
            {
                if (price > stopPrice)
                    throw new ArgumentException("StopLimit: price > stopPrice");
            }

            param["ordType"] = "StopLimit";
            param["price"]   = price.ToString();
            param["stopPx"]  = stopPrice.ToString();
            
            if (closeOnTrigger)
                execInst = execInst + ",Close";

            if (!String.IsNullOrEmpty(text))
                param["text"] = text;
            
            param["execInst"] = execInst;
            return PostOrder(param, side, qty, symbol);
        }

        public BitmexOrderData CreateStopMarketOrder(string symbol, string side, long qty, double stopPrice, bool closeOnTrigger = true, string text = null)
        {
            var param = new Dictionary<string, string>();
            var execInst = "LastPrice";

            param["ordType"] = "Stop";            
            param["stopPx"]  = stopPrice.ToString();

            if (closeOnTrigger)
                execInst = execInst + ",Close";
            if (text != null)
                param["text"] = text;

            param["execInst"] = execInst;

            return PostOrder(param, side, qty, symbol);
        }

        public BitmexOrderData UpdateLimitOrder(string orderId, long? qty = null, double? price = null, double? stopPrice = null)
        {
            var response = PutOrder(orderId, qty, price, stopPrice);
            return JsonConvert.DeserializeObject<BitmexOrderData>(response.Content);
        }

        private ApiResult PutOrder(string orderId, long? qty = null, double? price = null, double? stopPrice = null)
        {
            var param = new Dictionary<string, string>();
            param["orderID"] = orderId;

            if (qty.HasValue)
                param["orderQty"] = qty.Value.ToString();
            if (price.HasValue)
                param["price"] = price.Value.ToString();
            if (stopPrice.HasValue)
                param["stopPx"] = stopPrice.Value.ToString();

            string url;
            var postData = UrlEncode(param);
            var args = CreateParameters(HttpMethod.Put, "/order", postData, out url);

            return UserQuery(url, HttpMethod.Put, args, postData);
        }

        private BitmexOrderData PostOrder(Dictionary<string, string> param, string side, long? qty, string symbol)
        {
            param["symbol"] = symbol;
            param["side"]   = side;

            if (qty.HasValue)
                param["orderQty"] = qty.Value.ToString();

            string url;
            var postData = UrlEncode(param);
            var args     = CreateParameters(HttpMethod.Post, "/order", postData, out url);
            var response = UserQuery(url, HttpMethod.Post, args, postData);

            if (ContainsError(response))
            {
                var btxResponse = JsonConvert.DeserializeObject<BitmexResponse>(response.Content);
                return new BitmexOrderData { Error = btxResponse.Error.Message, Text = btxResponse.Error.Name };
            }
            return JsonConvert.DeserializeObject<BitmexOrderData>(response.Content);
        }

        protected override void ParseResponseHeaders(xNet.HttpResponse response)
        {
            var iterator = response.EnumerateHeaders();
            while (iterator.MoveNext())
            {
                var header = iterator.Current;
                if (String.Compare(header.Key, "X-RateLimit-Limit", true) == 0)
                {
                    XRateLimit = Convert.ToInt32(header.Value);
                }
                else if (String.Compare(header.Key, "X-RateLimit-Reset", true) == 0)
                {
                    XRateLimitReset = Convert.ToInt32(header.Value);
                }
                else if (String.Compare(header.Key, "X-RateLimit-Remaining", true) == 0)
                {                    
                    XRateLimitRemaining = Convert.ToInt32(header.Value);
                }
                else if (String.Compare(header.Key, "Retry-After", true) == 0)
                {
                    RetryAfterSeconds = Convert.ToInt32(header.Value);
                }
            }
        }        
    }
}
