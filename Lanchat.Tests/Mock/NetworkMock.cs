using System;
using System.Net;
using System.Net.Sockets;
using Lanchat.Core.Network;

namespace Lanchat.Tests.Mock
{
    public class NetworkMock : INetworkElement
    {
        public IPEndPoint Endpoint { get; }
        public Guid Id { get; }
        public bool EnableReconnecting { get; set; }
        public bool IsSession { get; }

        public void SendAsync(string text)
        {
            DataReceived?.Invoke(this, text);
        }

        public void Close()
        { }

        public event EventHandler Connected;
        public event EventHandler<bool> Disconnected;
        public event EventHandler<SocketError> SocketErrored;
        public event EventHandler<string> DataReceived;
    }
}