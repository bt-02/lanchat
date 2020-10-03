﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Lanchat.Core.Models;
using Lanchat.Core.Network;

namespace Lanchat.Core
{
    public class Node
    {
        private readonly INetworkElement networkElement;
        private readonly Output networkOutput;

        public Node(INetworkElement networkElement)
        {
            this.networkElement = networkElement;
            networkOutput = new Output(this);
            networkElement.Connected += OnConnected;
            networkElement.Disconnected += OnDisconnected;
            networkElement.DataReceived += OnDataReceived;
            networkElement.SocketErrored += OnSocketErrored;
        }
        
        // Node properties
        public string Nickname { get; private set; }
        public bool Ready { get; private set; }

        // Network element properties
        public Guid Id => networkElement.Id;
        public IPEndPoint Endpoint => networkElement.Endpoint;

        // Node events
        public event EventHandler<string> MessageReceived;
        public event EventHandler PingReceived;
        
        // Network element events
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler<SocketError> SocketErrored;

        // Events forwarding
        private void OnConnected(object sender, EventArgs e)
        {
            networkOutput.SendHandshake();
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private void OnSocketErrored(object sender, SocketError e)
        {
            SocketErrored?.Invoke(this, e);
        }

        // Network input
        private void OnDataReceived(object sender, string e)
        {
            try
            {
                var data = JsonSerializer.Deserialize<Wrapper>(e);
                
                // If node isn't ready ignore every messages except handshake
                if (!Ready && data.Type != DataTypes.Handshake)
                {
                    return;
                }
                
                switch (data.Type)
                {
                    case DataTypes.Message:
                        var message = JsonSerializer.Deserialize<Message>(data.Data.ToString());
                        MessageReceived?.Invoke(this, message.Content);
                        break;

                    case DataTypes.Ping:
                        PingReceived?.Invoke(this, EventArgs.Empty);
                        break;

                    case DataTypes.Handshake:
                        var handshake = JsonSerializer.Deserialize<Handshake>(data.Data.ToString());
                        Nickname = handshake.Nickname;
                        Ready = true;
                        Connected?.Invoke(this, EventArgs.Empty);
                        break;
                    
                    default:
                        Debug.WriteLine("Unknown type received");
                        break;
                }
            }
            
            // Input errors catching
            catch (Exception ex)
            {
                if (ex is JsonException || ex is ArgumentNullException)
                {
                    Debug.WriteLine("Invalid json received");
                }
                else
                {
                    throw;
                }
            }
        }

        // Node output
        public void SendMessage(string content) => networkOutput.SendMessage(content);
        public void SendPing() => networkOutput.SendPing();

        // Network element methods
        internal bool SendAsync(string text) => networkElement.SendAsync(text);
    }
}