namespace RoundReports.Commands.MvpCommand
{
    using System;
    using System.Linq;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;

    /// <summary>
    /// Command to remove MVP points.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Remove : ICommand, IUsageProvider
    {
        /// <inheritdoc/>
        public string Command => "remove";

        /// <inheritdoc/>
        public string[] Aliases => new[] { "rm" };

        /// <inheritdoc/>
        public string Description => "Removes points from the provided player.";

        /// <inheritdoc/>
        public string[] Usage => new[] { "players", "SCP/Human", "amount", "reason" };

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("rr.mvp.remove"))
            {
                response = "Missing permission: rr.mvp.remove";
                return false;
            }

            if (!MainPlugin.Configs.MvpSettings.MvpEnabled)
            {
                response = "MVP system is disabled in server configs.";
                return false;
            }

            if (!Round.InProgress)
            {
                response = "Points can only be added during a round.";
                return false;
            }

            if (arguments.Count < 3)
            {
                response = "Invalid command usage. Required parameters: players, scp/human, amount";
                return false;
            }

            string team = arguments.At(1).ToLower();
            if (team is not "scp" && team is not "human")
            {
                response = "Invalid team provided. Team must be 'scp' or 'human'.";
                return false;
            }

            PointTeam pt = team is "scp" ? PointTeam.SCP : PointTeam.Human;

            if (!int.TryParse(arguments.At(2), out int amount))
            {
                response = "Invalid command usage. Amount of points must be an integer.";
                return false;
            }

            Player player = Player.Get(arguments.At(0));

            if (player is null)
            {
                response = "Invalid player provided.";
                return false;
            }

            if (player.DoNotTrack)
            {
                response = "This player has do not track enabled; they cannot be MVPs.";
                return false;
            }

            if (amount > 0) amount = -amount;

            string reason = string.Join(" ", arguments.Skip(3));
            MainPlugin.Handlers.IncrementPoints(player, amount, reason, pt, true);

            response = $"Removed {amount} MVP points from {player.Nickname}!";
            return true;
        }
    }
}
