using Lanchat.Core.Api;
using Lanchat.Core.Chat;
using Lanchat.Core.Chat.Handlers;
using Lanchat.Core.Config;
using Lanchat.Core.Encryption.Handlers;
using Lanchat.Core.FileSystem;
using Lanchat.Core.FileTransfer;
using Lanchat.Core.Network.Handlers;

namespace Lanchat.Core.Network
{
    internal static class HandlersSetup
    {
        internal static void RegisterHandlers(IResolver resolver, Node node, IConfig config, IStorage storage)
        {
            resolver.RegisterHandler(new HandshakeHandler(
                node.PublicKeyEncryption,
                node.SymmetricEncryption,
                node.Output, node));
            resolver.RegisterHandler(new KeyInfoHandler(node.SymmetricEncryption, node));
            resolver.RegisterHandler(new ConnectionControlHandler(node.Host));
            resolver.RegisterHandler(new UserStatusUpdateHandler(node));
            resolver.RegisterHandler(new NicknameUpdateHandler(node));
            resolver.RegisterHandler(new MessageHandler(node.Messaging));
            resolver.RegisterHandler(new FilePartHandler(node.FileReceiver, storage));
            resolver.RegisterHandler(new FileReceiveRequestHandler(node.FileReceiver, storage));
            resolver.RegisterHandler(new FileTransferControlHandler(node.FileReceiver, node.FileSender));
        }
    }
}