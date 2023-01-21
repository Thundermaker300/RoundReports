using CommandSystem;
using Exiled.API.Features.Pools;
using System;
using System.Text;

namespace RoundReports.Commands.MvpCommand
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Mvp : ParentCommand
    {
        public Mvp() => LoadGeneratedCommands();

        public override string Command => "mvp";

        public override string[] Aliases => Array.Empty<string>();

        public override string Description => "Base command for MVP-related controls";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new Add());
            RegisterCommand(new Remove());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder sb = StringBuilderPool.Pool.Get();

            sb.AppendLine("Available commands:");
            sb.AppendLine("- MVP ADD <PLAYERS> <SCP/HUMAN> <AMOUNT> <REASON> - Adds specified points to the player(s).");
            sb.AppendLine("- MVP RM <PLAYERS> <SCP/HUMAN> <AMOUNT> <REASON> - Removes points from the player(s).");

            response = StringBuilderPool.Pool.ToStringReturn(sb);
            return false;
        }
    }
}
