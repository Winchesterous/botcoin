using System;

namespace BotCoin.BitmexScalper.Helpers
{
    public static class StringExtensions
    {
        public static bool Null(this string str)
        {
            return String.IsNullOrEmpty(str);
        }

        public static bool NotNull(this string str)
        {
            return !String.IsNullOrEmpty(str);
        }

        public static string GetOrderId(this string id)
        {
            return id.Split('-')[0];
        }
    }
}
