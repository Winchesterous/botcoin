using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace BotCoin.Logger
{
    public class DatabaseLogger
    {
        SqlConnection _connection;
        bool _reconnect = true;

        protected StringBuilder _str;
        protected object _obj;

        public DatabaseLogger()
        {
            _connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Botcoin"].ConnectionString);
            _str = new StringBuilder();
            _obj = new object();
        }

        protected string FormatTime(DateTime dt)
        {
            var date = dt.Date;
            var ts = dt.TimeOfDay;
            return String.Format("'{0}-{1}-{2} {3:00}:{4:00}:{5:00}.{6:000}'", date.Year, date.Month, date.Day, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        }

        private SqlConnection GetConnection()
        {
            lock (_obj)
            {
                if (_connection.State != ConnectionState.Open)
                {
                    if (_reconnect)
                        Run();
                }
            }
            return _connection;
        }

        public void Run()
        {
            _connection.Open();
        }

        public void Stop()
        {
            lock (_obj) _reconnect = false;
        }

        public async void WriteLineAsync(string sqlQuery)
        {
            lock (_obj)
            {
                if (!_reconnect)
                {
                    if (_connection.State != ConnectionState.Open)
                        _connection.Close();
                    return;
                }
            }

            var cmd = new SqlCommand(sqlQuery, GetConnection());
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
