using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using System;

namespace RoundReports.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class AddRemark : ICommand, IUsageProvider
    {
        public string Command => "addremark";

        public string[] Aliases => new[] { "remark" };

        public string Description => "Adds a staff remark to the round remarks.";

        public string[] Usage => new[] { "remark" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("rr.remark"))
            {
                response = "Missing permission: rr.remark";
                return false;
            }
            if (Round.IsEnded)
            {
                response = "This command is unavailable after the end of the round.";
                return false;
            }
            if (MainPlugin.Reporter is null)
            {
                response = "Reporter is not active this round.";
                return false;
            }
            if (arguments.Count == 0)
            {
                response = "Missing argument: remark";
                return false;
            }
            MainPlugin.Reporter.AddRemark("[Staff Remark] " + string.Join(" ", arguments));
            response = "Added remark!";
            return true;
        }
    }
}
