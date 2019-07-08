using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace LockNess.Communication.SocketFac
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
    }
}
