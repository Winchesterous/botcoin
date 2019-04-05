using BotCoin.DataType;
using System;

namespace BotCoin.Logger
{
    public class OrderBookLogger : DatabaseLogger
    {
        readonly int _limit;
        
        public OrderBookLogger(int limit = 15)
        {
            _limit = limit;
        }

        public void LogAsync(LogOrderBookData data, bool? isDeleted)
        {
            int bidsCount = data.Bids != null ? data.Bids.Length : 0;
            int asksCount = data.Asks != null ? data.Asks.Length : 0;

            if (bidsCount > _limit) bidsCount = _limit;
            if (asksCount > _limit) asksCount = _limit;

            int maxCount = Math.Max(bidsCount, asksCount);
            string line = null;

            lock (_obj)
            {
                if (maxCount == 0)
                {
                    BuildQuery(data, 0, 0, 0, 0);
                }
                else
                {
                    for (int i = 0; i < maxCount; i++)
                    {
                        double bidPrice = 0;
                        double bidVolume = 0;
                        double askPrice = 0;
                        double askVolume = 0;

                        if (data.Bids != null && data.Bids.Length > 0)
                        {
                            if (i < data.Bids.Length)
                            {
                                bidPrice = data.Bids[i][0];
                                bidVolume = data.Bids[i][1];
                            }
                        }
                        if (data.Asks != null && data.Asks.Length > 0)
                        {
                            if (i < data.Asks.Length)
                            {
                                askPrice = data.Asks[i][0];
                                askVolume = data.Asks[i][1];
                            }
                        }

                        BuildQuery(data, bidPrice, askPrice, bidVolume, askVolume);
                    }
                }

                if (_str.Length > 0)
                {
                    line = _str.ToString();
                    _str.Clear();
                }

                if (line != null)
                    WriteLineAsync(line);
            }
        }

        private void BuildQuery(LogOrderBookData data, double bidPrice, double askPrice, double bidVolume, double askVolume)
        {
            _str.AppendFormat("INSERT INTO dbo.OrderBook{0}(CreatedAt,CryptoCurrencyId,CurrencyId,BidPrice,AskPrice,BidAmount,AskAmount", data.Exchange.ToString());
            if (data.OrderId.HasValue && data.OrderId.Value != 0)
                _str.Append(",OrderId");
            if (data.Timestamp.HasValue)
                _str.Append(",Timestamp");
            if (data.Exchange == ExchangeName.Bitstamp && data.IsDeleted.HasValue)
                _str.Append(",IsDeleted");
            _str.Append(")VALUES(");
            _str.AppendFormat(FormatTime(data.CreatedAt));
            _str.AppendFormat(",{0}", (int)data.CryptoCurrency);
            _str.AppendFormat(",{0}", (int)data.Currency);
            _str.AppendFormat(",{0}", bidPrice);
            _str.AppendFormat(",{0}", askPrice);
            _str.AppendFormat(",{0}", bidVolume);
            _str.AppendFormat(",{0}", askVolume);
            if (data.OrderId.HasValue && data.OrderId.Value != 0)
                _str.AppendFormat(",{0}", data.OrderId);
            if (data.Timestamp.HasValue)
            {
                _str.AppendFormat("," + FormatTime(data.Timestamp.Value));
            }
            if (data.Exchange == ExchangeName.Bitstamp && data.IsDeleted.HasValue)
                _str.AppendFormat(",{0}", data.IsDeleted.Value ? 1 : 0);
            _str.Append(")");
            _str.AppendLine();
        }
    }
}
