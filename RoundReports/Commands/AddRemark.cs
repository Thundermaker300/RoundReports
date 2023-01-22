namespace RoundReports.Commands
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;

    /// <summary>
    /// Command to add a remark to the remarks section.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class AddRemark : ICommand, IUsageProvider
    {
        /// <inheritdoc/>
        public string Command => "addremark";

        /// <inheritdoc/>
        public string[] Aliases => new[] { "remark" };

        /// <inheritdoc/>
        public string Description => "Adds a staff remark to the round remarks.";

        /// <inheritdoc/>
        public string[] Usage => new[] { "remark" };

        /// <inheritdoc/>
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
