using LockNess.Communication.Core;
using LockNess.Communication.Core.SocketFac;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LockNess.Communication.Servers
{
    public class SocketServerHost : IHostedService
    {
        private ISocketBuilder _socketBuilder;
        private Socket _listenSocket;
        private TaskCompletionSource<bool> _stopTaskCompletionSource;
        private CancellationTokenSource _cancellationTokenSource;

        public SocketServerHost(ISocketBuilder socketBuilder )
        {
            _socketBuilder = socketBuilder;
            _cancellationTokenSource = new CancellationTokenSource();
        }
        private Task<bool> StartListen()
        {
            Start();
            return Task.FromResult(true);
        }
        public bool Start()
        {
            try
            {
                var listenSocket = _listenSocket = _socketBuilder.Create();
                listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 5000));
                listenSocket.Listen(5000);
                _= PrcoessConnect(listenSocket);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private  async Task PrcoessConnect(Socket listenSocket)
        {

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var socket = await listenSocket.AcceptAsync();
                await OnNewClientAccept(socket);
            }
        }

        private async Task OnNewClientAccept(Socket socket)
        {
            var tcpConnection = new TcpConnection(socket);
            await tcpConnection.StartAsync();
        }

        public Task StopAsync()
        {
            var listenSocket = _listenSocket;

            if (listenSocket == null)
                return Task.Delay(0);

            _stopTaskCompletionSource = new TaskCompletionSource<bool>();

            _cancellationTokenSource.Cancel();
            listenSocket.Close();

            return _stopTaskCompletionSource.Task;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await StartListen();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await StopAsync();
        }
    }
}
