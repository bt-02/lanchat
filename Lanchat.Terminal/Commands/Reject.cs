using Lanchat.Terminal.Properties;
using Lanchat.Terminal.UserInterface;

namespace Lanchat.Terminal.Commands
{
    public class Reject : ICommand
    {
        public string Alias { get; set; } = "reject";
        public int ArgsCount { get; set; } = 1;

        public void Execute(string[] args)
        {
            var node = Program.Network.Nodes.Find(x => x.ShortId == args[0]);
            if (node == null)
            {
                Ui.Log.Add(Resources.Info_NotFound);
                return;
            }

            node.FilesExchange.RejectRequest();
        }
    }
}