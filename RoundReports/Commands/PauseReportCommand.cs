using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using System;

namespace RoundReports.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class PauseReportCommand : ICommand
    {
        public string Command => "pausereport";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "Pause round reporting.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("rr.pause"))
            {
                response = "Missing permission: rr.pause";
                return false;
            }
            if (MainPlugin.Reporter is null)
            {
                response = "Reporter is not active this round.";
                return false;
            }
            MainPlugin.Reporter.Kill();
            response = "Reporter paused for this round. Reporter will resume next round.";
            return true;
        }
    }
}
