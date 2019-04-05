using BotCoin.Core;
using BotCoin.DataType;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BotCoin.Service
{
    public class TcpService
    {
        readonly CancellationPattern _cancellation;
        private TcpListener _server;

        public TcpService()
        {
            _cancellation = new CancellationPattern();
        }

        public void InvokeServer(IPEndPoint endPoint, byte[] bytes, Action<NetworkStream, byte[]> onRead = null)
        {
            using (var socket = new TcpClient())
            {
                try
                {
                    socket.Connect(endPoint);
                    using (var stream = socket.GetStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                        if (onRead != null)
                        {
                            var readBuffer = new byte[10240];
                            stream.Read(readBuffer, 0, readBuffer.Length);
                            onRead(stream, readBuffer);
                        }
                    }
                }
                catch { }
            }
        }

        public void StartServer(int port)
        {
            var ipHostInfo = Dns.Resolve(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            _server = new TcpListener(new IPEndPoint(ipAddress, port));

            _server.Start();
            _cancellation.Run();
        }

        public void StopServer()
        {
            _cancellation.Cancel();
        }

        public Task StartListener(ServiceName service, Func<byte[], byte[]> onRead, Action<NetworkStream, byte[]> onWrite, Action<ServiceEventData> onException)
        {
            return Task.Run(() =>
            {
                _cancellation.ThreadActivity(service,
                    () =>
                    {
                        if (_server.Pending()) HandleRequestAsync(onRead, onWrite);
                        else Thread.Sleep(50);
                    },
                    data => onException(data),
                    () => _server.Stop()
                    );
            },
            _cancellation.Token);
        }

        private async void HandleRequestAsync(Func<byte[], byte[]> onRead, Action<NetworkStream, byte[]> onWrite = null)
        {
            var client = await _server.AcceptTcpClientAsync();
            using (var stream = client.GetStream())
            {
                var bytes = new byte[1024];
                stream.Read(bytes, 0, bytes.Length);
                bytes = onRead(bytes);
                if (onWrite != null) onWrite(stream, bytes);
            }
            client.Close();
        }
    }
}
