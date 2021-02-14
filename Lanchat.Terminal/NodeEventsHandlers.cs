﻿using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using Lanchat.Core;
using Lanchat.Core.FilesTransfer;
using Lanchat.Core.Models;
using Lanchat.Terminal.Properties;
using Lanchat.Terminal.UserInterface;

namespace Lanchat.Terminal
{
    public class NodeEventsHandlers
    {
        private readonly Node node;

        public NodeEventsHandlers(Node node)
        {
            this.node = node;
            node.NetworkInput.MessageReceived += OnMessageReceived;
            node.NetworkInput.PrivateMessageReceived += OnPrivateMessageReceived;
            node.NetworkInput.PongReceived += OnPongReceived;
            
            node.FilesTransfer.FileReceived += OnFileReceived;
            node.FilesTransfer.FileExchangeError += OnFileTransferError;
            node.FilesTransfer.FileExchangeRequestReceived += OnFilesTransferRequestReceived;
            node.FilesTransfer.FileExchangeRequestAccepted += OnFileTransferRequestAccepted;
            node.FilesTransfer.FileExchangeRequestRejected += OnFileTransferRequestRejected;
            
            node.Connected += OnConnected;
            node.Disconnected += OnDisconnected;
            node.HardDisconnect += OnHardDisconnected;
            node.SocketErrored += OnSocketErrored;
            node.CannotConnect += OnCannotConnect;
            node.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Status":
                    var status = node.Status switch
                    {
                        Status.Online => "online",
                        Status.AwayFromKeyboard => "afk",
                        Status.DoNotDisturb => "dnd",
                        _ => ""
                    };
                    Ui.Log.Add(string.Format(Resources._StatusChange, node.Nickname, status));
                    break;

                case "Nickname":
                    if (!node.Ready) return;
                    Ui.Log.Add(string.Format(Resources._NicknameChanged, node.PreviousNickname, node.Nickname));
                    break;
            }
        }

        private void OnConnected(object sender, EventArgs e)
        {
            Ui.Log.Add(string.Format(Resources._Connected, node.Nickname));
            Ui.NodesCount.Text = Program.Network.Nodes.Count.ToString();
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            Ui.Log.Add(string.Format(Resources._Reconnecting, node.Nickname));
            Ui.NodesCount.Text = Program.Network.Nodes.Count.ToString();
        }

        private void OnHardDisconnected(object sender, EventArgs e)
        {
            Ui.Log.Add(string.Format(Resources._Disconnected, node.Nickname));
            Ui.NodesCount.Text = Program.Network.Nodes.Count.ToString();
        }

        private void OnMessageReceived(object sender, string e)
        {
            Ui.Log.AddMessage(e, node.Nickname);
        }

        private void OnPrivateMessageReceived(object sender, string e)
        {
            Ui.Log.AddPrivateMessage(e, node.Nickname);
        }

        private void OnSocketErrored(object sender, SocketError e)
        {
            Ui.Log.Add(string.Format(Resources._ConnectionError, node.Endpoint.Address, e));
        }

        private void OnCannotConnect(object sender, EventArgs e)
        {
            Ui.Log.Add(string.Format(Resources._CannotConnect, node.Endpoint));
        }

        private void OnPongReceived(object sender, TimeSpan? e)
        {
            if (e != null)
                Ui.Log.Add(string.Format(Resources._Ping, node.Nickname, e.Value.Milliseconds));
        }

        private void OnFilesTransferRequestReceived(object sender, FileTransferRequest e)
        {
            Ui.Log.Add(string.Format(Resources._FileRequest, node.Nickname, e.FileName));
        }

        private void OnFileReceived(object sender, FileTransferRequest e)
        {
            Ui.Log.Add(string.Format(Resources._FileReceived, node.Nickname, Path.GetFullPath(e.FilePath)));
        }

        private void OnFileTransferError(object sender, Exception e)
        {
            Ui.Log.Add(string.Format(Resources._FileExchangeError, e.Message));
        }

        private void OnFileTransferRequestAccepted(object sender, EventArgs e)
        {
            Ui.Log.Add(string.Format(Resources._FileRequestAccepted, node.Nickname));
        }
        
        private void OnFileTransferRequestRejected(object sender, EventArgs e)
        {
            Ui.Log.Add(string.Format(Resources._FileRequestRejected, node.Nickname));
        }
    }
}