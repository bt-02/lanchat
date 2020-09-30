﻿using System;
using System.Net.Sockets;
using Lanchat.Core.Network;

namespace Lanchat.Probe.Handlers
{
    public class ClientEventsHandlers
    {
        private readonly Client client;

        public ClientEventsHandlers(Client client)
        {
            this.client = client;
            client.Connected += ClientOnClientConnected;
            client.Disconnected += ClientOnClientDisconnected;
            client.MessageReceived += ClientOnMessageReceived;
            client.SocketErrored += ClientOnSocketErrored;
        }

        private void ClientOnSocketErrored(object sender, SocketError e)
        {
            Console.WriteLine("Client error");
            Console.WriteLine($"{client.Id} / {e}");
            Console.WriteLine("");
        }

        private void ClientOnMessageReceived(object sender, string e)
        {
            Console.WriteLine("Message received");
            Console.WriteLine($"{client.Id} / {client.Endpoint}");
            Console.WriteLine(e);
            Console.WriteLine("");
        }

        private void ClientOnClientDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected from server");
            Console.WriteLine($"{client.Id} / {client.Endpoint}");
            Console.WriteLine("");
        }

        private void ClientOnClientConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected to server");
            Console.WriteLine($"{client.Id} / {client.Endpoint}");
            Console.WriteLine("");
        }
    }
}