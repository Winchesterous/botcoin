using System;
using System.Net;

namespace BotCoin.BitmexBotService
{
    internal class SelfHost
    {
        readonly BitmexController _btx;

        public SelfHost()
        {
            _btx = new BitmexController();
        }

        private string FormatMessage(string msg)
        {
            return String.Format("{0} [{1}]", msg, Dns.GetHostName());
        }

        public void Start()
        {
            try
            {
                _btx.Start();
                _btx.Log.WriteInfo(FormatMessage("Service started"));
            }
            catch (Exception ex)
            {
                _btx.Log.WriteError(ex);
            }
        }

        public void Stop()
        {
            try
            {
                _btx.Stop();
                _btx.Log.WriteInfo(FormatMessage("Service stopped"));
            }
            catch (Exception ex)
            {
                _btx.Log.WriteError(ex);
            }
        }
    }
}
