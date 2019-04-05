using BotCoin.DataType.RemoteCommand;
using MessagePack;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace BotCoin.Service
{
    public class RemoteService
    {
        readonly TcpService _tcp;
        protected IPEndPoint _endPoint;

        protected RemoteService()
        {
            _tcp = new TcpService();
        }

        protected void SendMessage<T1>(Func<T1> createCommand, Func<RemoteCommandType> getCmdType = null)
        {
            T1 cmd = createCommand();

            var sendBytes = MessagePackSerializer.Serialize<T1>(cmd);
            if (getCmdType != null)
                sendBytes = PrepareMessage(sendBytes, getCmdType());

            _tcp.InvokeServer(_endPoint, sendBytes);
        }

        protected T2 SendReceiveMessage<T1, T2>(Func<T1> createCommand, Func<RemoteCommandType> getCmdType)
        {
            T1 cmd = createCommand();

            var sendBytes = MessagePackSerializer.Serialize<T1>(cmd, MessagePack.Resolvers.StandardResolver.Instance);
            sendBytes = PrepareMessage(sendBytes, getCmdType());
            T2 response = default(T2);

            _tcp.InvokeServer(_endPoint, sendBytes,
                (ns, bytes) => response = MessagePackSerializer.Deserialize<T2>(bytes)
                );

            return response;
        }

        protected byte[] PrepareMessage(byte[] bytes, RemoteCommandType cmdType)
        {
            var buffer = new byte[bytes.Length + 1];

            Array.Copy(BitConverter.GetBytes((int)cmdType), buffer, 1);
            Array.Copy(bytes, 0, buffer, 1, buffer.Length - 1);

            return buffer;
        }

        public static IPAddress GetAddressByHost(string domainName)
        {
            return Dns.GetHostEntry(domainName)
                      .AddressList
                      .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                      .First();
        }
    }
}
