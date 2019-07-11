using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LockNess.Communication.Core.SocketFac
{
    public class DefaultSocketBuilder : ISocketBuilder
    {
        public DefaultSocketBuilder()
        {
        }
        private Socket _listenSocket;
        public Socket Create()
        {
            try
            {
                var listenSocket = _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.LingerState = new LingerOption(false, 0);
                listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                return listenSocket;
            }
            catch (Exception e)
            {

                return null;
            }

        }


        internal static Socket CreateSocket(EndPoint endpoint)
        {
            var addressFamily = endpoint.AddressFamily;
            if (addressFamily == AddressFamily.Unspecified && endpoint is DnsEndPoint)
            {   // default DNS to ipv4 if not specified explicitly
                addressFamily = AddressFamily.InterNetwork;
            }

            var protocolType = addressFamily == AddressFamily.Unix ? ProtocolType.Unspecified : ProtocolType.Tcp;
            var socket = new Socket(addressFamily, SocketType.Stream, protocolType);
            SocketConnection.SetRecommendedClientOptions(socket);
            //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, false);
            return socket;
        }
    }
}
