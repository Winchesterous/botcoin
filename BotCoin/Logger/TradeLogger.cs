using System.Collections.Generic;

namespace BotCoin.Logger
{
    public class TradeLogger : DatabaseLogger
    {
        public void LogAsync(LogTradeData data)
        {
            lock (_obj)
            {
                BuildQuery(data);
                WriteLineAsync(_str.ToString());

                _str.Clear();
            }
        }

        public void LogAsync(List<LogTradeData> data)
        {
            lock (_obj)
            {
                foreach (var item in data)
                {
                    BuildQuery(item);
                    WriteLineAsync(_str.ToString());

                    _str.Clear();
                }
            }
        }

        public void BuildQuery(LogTradeData data)
        {
            _str.AppendFormat("INSERT INTO dbo.Trade{0}(OrderId,CreatedAt,InstrumentId,Price,Amount,OrderType", data.Exchange);
            if (data.BuyOrderId.HasValue)
                _str.Append(",BuyOrderId");
            if (data.SellOrderId.HasValue)
                _str.Append(",SellOrderId");
            if (data.Timestamp.HasValue)
                _str.Append(",Timestamp");
            _str.Append(")VALUES("); 
            _str.AppendFormat("{0}", data.OrderId);
            _str.AppendFormat("," + FormatTime(data.CreatedAt));
            _str.AppendFormat(",{0}", (int)data.Instrument);
            _str.AppendFormat(",{0}", data.Price);
            _str.AppendFormat(",{0}", data.Amount);
            _str.AppendFormat(",'{0}'", data.OrderType);
            if (data.BuyOrderId.HasValue)
                _str.AppendFormat(",{0}", data.BuyOrderId.Value);
            if (data.SellOrderId.HasValue)
                _str.AppendFormat(",{0}", data.SellOrderId.Value);
            if (data.Timestamp.HasValue)
                _str.AppendFormat("," + FormatTime(data.Timestamp.Value));
            _str.Append(")");
            _str.AppendLine();
        }
    }
}
