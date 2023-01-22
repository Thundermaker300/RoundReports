namespace RoundReports.Commands
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;

    /// <summary>
    /// Command to turn off round reports for this round.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class PauseReportCommand : ICommand
    {
        /// <inheritdoc/>
        public string Command => "pausereport";

        /// <inheritdoc/>
        public string[] Aliases => Array.Empty<string>();

        /// <inheritdoc/>
        public string Description => "Pause round reporting.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("rr.pause"))
            {
                response = "Missing permission: rr.pause";
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

            Log.Debug($"Kill request sent by {sender.LogName}.");
            MainPlugin.Reporter.Kill();
            response = "Reporter paused for this round. Reporter will resume next round.";
            return true;
        }
    }
}
