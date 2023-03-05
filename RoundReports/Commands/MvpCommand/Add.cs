namespace RoundReports.Commands.MvpCommand
{
    using System;
    using System.Linq;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;

    /// <summary>
    /// Command to add MVP points.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Add : ICommand, IUsageProvider
    {
        /// <inheritdoc/>
        public string Command => "add";

        /// <inheritdoc/>
        public string[] Aliases => new[] { "a" };

        /// <inheritdoc/>
        public string Description => "Adds points to the provided player.";

        /// <inheritdoc/>
        public string[] Usage => new[] { "players", "SCP/Human", "amount", "reason" };

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("rr.mvp.add"))
            {
                response = "Missing permission: rr.mvp.add";
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
                if (sender.CheckPermission(PlayerPermissions.PlayersManagement))
                    response = "This player has Do Not Track (DNT) enabled; they cannot be MVPs.";
                else
                    response = "An unknown error has occurred when adding points to this player.";
                return false;
            }

            if (amount <= 0)
            {
                response = "Zero or less than zero points have been provided. Player's point values have not changed.";
                return false;
            }

            try
            {
                string reason = string.Join(" ", arguments.Skip(3));
                MainPlugin.Handlers.IncrementPoints(player, amount, reason, pt, true);
            }
            catch (Exception ex)
            {
                response = $"An error occurred when adding points to this player. {ex}";
                return false;
            }

            response = $"Added {amount} MVP points to {player.Nickname}!";
            return true;
        }
    }
}
