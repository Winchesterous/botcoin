using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace BotCoin.Service
{
    public class ClockSyncService
    {
        static string[] Hosts = 
        {
            "time.nist.gov", "time-nw.nist.gov", "time-a.nist.gov",
            "time-b.nist.gov", "tick.mit.edu", "time.windows.com",
        };

        public static bool SetInternetTime()
        {
            var startDT = DateTime.Now;
            var error = String.Empty;
            var sb = new StringBuilder();

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.ReceiveTimeout = 10 * 1000;
                foreach (string strHost in Hosts)
                {
                    try
                    {
                        var ipe = new IPEndPoint(Dns.GetHostEntry(strHost).AddressList[0], 13);
                        socket.Connect(ipe);

                        if (socket.Connected)
                            break;
                    }
                    catch
                    { }
                }
                if (!socket.Connected)
                    return false;

                var recvBuffer = new byte[1024];                            
                var encoding = Encoding.UTF8;
                int nBytes, nTotalBytes = 0;

                while ((nBytes = socket.Receive(recvBuffer, 0, 1024, SocketFlags.None)) > 0)
                {
                    nTotalBytes += nBytes;
                    sb.Append(encoding.GetString(recvBuffer, 0, nBytes));
                }
            }

            string[] str = sb.ToString().Split(' '); 
            var ts = (TimeSpan)(DateTime.Now - startDT);
            var dt = Convert.ToDateTime(str[1] + " " + str[2]).Subtract(-ts);

            //Disposal at +8 Beijing time  
            dt = dt.AddHours(8);

            var st = new SystemTime();
            st.FromDateTime(dt);

            return Win32API.SetLocalTime(ref st);
        }
    }

    internal struct SystemTime
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;

        /// <summary>  
        /// Conversion from System.DateTime.   
        /// </summary>  
        /// <param name="time">System.DateTime type of time. </param>  
        public void FromDateTime(DateTime time)
        {
            wYear = (ushort)time.Year;
            wMonth = (ushort)time.Month;
            wDayOfWeek = (ushort)time.DayOfWeek;
            wDay = (ushort)time.Day;
            wHour = (ushort)time.Hour;
            wMinute = (ushort)time.Minute;
            wSecond = (ushort)time.Second;
            wMilliseconds = (ushort)time.Millisecond;
        }
        /// <summary>  
        /// Conversion to type System.DateTime.   
        /// </summary>  
        /// <returns></returns>  
        public DateTime ToDateTime()
        {
            return new DateTime(wYear, wMonth, wDay, wHour, wMinute, wSecond, wMilliseconds);
        }
        /// <summary>  
        /// Static method. Conversion to type System.DateTime.   
        /// </summary>  
        /// <param name="time">SYSTEMTIME type of time. </param>  
        /// <returns></returns>  
        public static DateTime ToDateTime(SystemTime time)
        {
            return time.ToDateTime();
        }
    }

    internal class Win32API
    {
        [DllImport("Kernel32.dll")]
        public static extern bool SetLocalTime(ref SystemTime Time);

        [DllImport("Kernel32.dll")]
        public static extern void GetLocalTime(ref SystemTime Time);
    }
}
