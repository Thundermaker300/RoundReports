using CommandSystem;
using System;
using Exiled.Permissions.Extensions;
using Exiled.API.Features;
using System.Linq;

namespace RoundReports.Commands.MvpCommand
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Remove : ICommand, IUsageProvider
    {
        public string Command => "remove";

        public string[] Aliases => new[] { "rm" };

        public string Description => "Removes points from the provided player.";

        public string[] Usage => new[] { "players", "amount", "reason" };

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

            if (arguments.Count < 2)
            {
                response = "Invalid command usage. Required parameters: players, amount";
                return false;
            }

            if (!int.TryParse(arguments.At(1), out int amount))
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

            string reason = string.Join(" ", arguments.Skip(2));
            MainPlugin.Handlers.RemovePoints(player, amount, reason);

            response = $"Removed {amount} MVP points from {player.Nickname}!";
            return true;
        }
    }
}
