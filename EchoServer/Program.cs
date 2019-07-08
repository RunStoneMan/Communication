using LockNess.Communication.Core.SocketFac;
using LockNess.Communication.Servers;
using System;
using System.Threading;

namespace EchoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketServerHost socketServerHost = new SocketServerHost( new DefaultSocketBuilder());
            socketServerHost.StartAsync( new CancellationToken()).Wait();
            Console.ReadKey();
        }
    }
}
