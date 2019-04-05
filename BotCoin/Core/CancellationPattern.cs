using BotCoin.DataType;
using System;
using System.Net.Sockets;
using System.Threading;

namespace BotCoin.Core
{
    public class CancellationPattern
    {
        CancellationTokenSource _cts;

        public void Run()
        {
            _cts = new CancellationTokenSource();
        }

        public void Cancel()
        {
            if (_cts != null) _cts.Cancel();
        }

        public CancellationToken Token { get { return _cts.Token; } }

        public bool IsCancelled
        {
            get
            {
                if (_cts == null) return true;
                return _cts.IsCancellationRequested;
            }
        }

        public void ThreadActivity(ServiceName service, Action action, Action<ServiceEventData> onException, Action onStop = null)
        {
            Func<string, ServiceEventData> serviceEventData = message => new ServiceEventData
            {
                Message = message,
                ServiceName = service,
                EventType = ServiceEventType.Fail
            };

            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    action();
                }
                catch (SocketException sx)
                {
                    onException(serviceEventData(String.Format("{0}. {1}", sx.SocketErrorCode.ToString(), sx.Message)));
                }
                catch (Exception ex)
                {
                    onException(serviceEventData(ex.Message + ex.StackTrace));
                }
            }

            if (onStop != null)
                onStop();
        }
    }
}
