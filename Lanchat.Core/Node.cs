﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using Lanchat.Core.Chat;
using Lanchat.Core.Encryption;
using Lanchat.Core.FileTransfer;
using Lanchat.Core.Models;
using Lanchat.Core.Network;
using Lanchat.Core.NetworkIO;
using Lanchat.Core.System;

namespace Lanchat.Core
{
    public class Node : IDisposable, INotifyPropertyChanged, INodeState
    {
        internal readonly Encryptor Encryptor;

        /// <summary>
        ///     File sending.
        /// </summary>
        public readonly FileReceiver FileReceiver;

        /// <summary>
        ///     File receiving.
        /// </summary>
        public readonly FileSender FileSender;

        /// <summary>
        ///     Messages sending and receiving.
        /// </summary>
        public readonly Messaging Messaging;

        /// <summary>
        ///     TCP session.
        /// </summary>
        public readonly INetworkElement NetworkElement;

        internal readonly NetworkInput NetworkInput;
        internal readonly INetworkOutput NetworkOutput;

        private string nickname;
        private string previousNickname;
        private Status status;
        private readonly IConfig config;

        /// <summary>
        ///     Initialize node.
        /// </summary>
        /// <param name="networkElement">TCP client or session.</param>
        /// <param name="config">Lanchat config.</param>
        public Node(INetworkElement networkElement, IConfig config)
        {
            this.config = config;
            NetworkElement = networkElement;
            NetworkOutput = new NetworkOutput(NetworkElement, this);
            Encryptor = new Encryptor();
            Messaging = new Messaging(NetworkOutput, Encryptor);
            FileReceiver = new FileReceiver(NetworkOutput, Encryptor, config);
            FileSender = new FileSender(NetworkOutput, Encryptor);

            NetworkInput = new NetworkInput(this);
            NetworkInput.ApiHandlers.Add(new InitializationApiHandlers(this, config));
            NetworkInput.ApiHandlers.Add(new NodeApiHandlers(this, config));
            NetworkInput.ApiHandlers.Add(new MessagingApiHandlers(Messaging, config));
            NetworkInput.ApiHandlers.Add(FileReceiver);
            NetworkInput.ApiHandlers.Add(new FileTransferHandler(FileReceiver, FileSender));

            NetworkElement.Disconnected += OnDisconnected;
            NetworkElement.DataReceived += NetworkInput.ProcessReceivedData;
            NetworkElement.SocketErrored += (s, e) => SocketErrored?.Invoke(s, e);

            if (NetworkElement.IsSession) SendHandshakeAndWait();

            // Check is connection established successful after timeout.
            Task.Delay(5000).ContinueWith(_ =>
            {
                if (!Ready && !UnderReconnecting) NetworkElement.Close();
            });
        }

        /// <summary>
        ///     Node is under reconnecting.
        /// </summary>
        public bool UnderReconnecting { get; private set; }

        /// <summary>
        ///     Node nickname.
        /// </summary>
        public string Nickname
        {
            get => $"{nickname}#{ShortId}";
            set
            {
                if (value == nickname) return;
                previousNickname = nickname;
                nickname = value;
                OnPropertyChanged(nameof(Nickname));
            }
        }

        /// <summary>
        ///     Nickname before last change.
        /// </summary>
        public string PreviousNickname => $"{previousNickname}#{ShortId}";

        /// <summary>
        ///     Short ID.
        /// </summary>
        public string ShortId => Id.GetHashCode().ToString().Substring(1, 4);

        /// <summary>
        ///     User status.
        /// </summary>
        public Status Status
        {
            get => status;
            set
            {
                if (value == status) return;
                status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        ///     Close connection with node and dispose.
        /// </summary>
        public void Dispose()
        {
            NetworkElement.Close();
            Encryptor.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     ID of TCP client or session.
        /// </summary>
        public Guid Id => NetworkElement.Id;

        /// <summary>
        ///     Node ready. If set to false node won't send or receive messages.
        /// </summary>
        public bool Ready { get; internal set; }

        /// <summary>
        ///     Invoked for properties like nickname or status.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Node successful connected and ready.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        ///     Node disconnected. Trying reconnect.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        ///     Node disconnected. Cannot reconnect.
        /// </summary>
        public event EventHandler HardDisconnect;

        /// <summary>
        ///     Raise when connection try failed.
        /// </summary>
        public event EventHandler CannotConnect;

        /// <summary>
        ///     TCP session or client for this node returned error.
        /// </summary>
        public event EventHandler<SocketError> SocketErrored;

        /// <summary>
        ///     Disconnect from node.
        /// </summary>
        public void Disconnect()
        {
            NetworkOutput.SendSystemData(new ConnectionControl
            {
                Status = ConnectionControlStatus.RemoteClose
            });
            Dispose();
        }

        // Network elements events.
        private void OnDisconnected(object sender, bool hardDisconnect)
        {
            UnderReconnecting = !hardDisconnect;

            // Raise event only if node was ready before.
            if (hardDisconnect && !Ready)
            {
                Trace.WriteLine($"Cannot connect {Id}");
                CannotConnect?.Invoke(this, EventArgs.Empty);
            }
            else if (hardDisconnect && Ready)
            {
                HardDisconnect?.Invoke(this, EventArgs.Empty);
            }
            else if (Ready)
            {
                Disconnected?.Invoke(this, EventArgs.Empty);
            }

            Ready = false;
        }

        internal void SendHandshakeAndWait()
        {
            var handshake = new Handshake
            {
                Nickname = config.Nickname,
                Status = config.Status,
                PublicKey = Encryptor.ExportPublicKey()
            };

            NetworkOutput.SendSystemData(handshake);
        }

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }
    }
}