using Exiled.API.Interfaces;
using Exiled.API.Features;
using System.ComponentModel;

using ServerEvents = Exiled.Events.Handlers.Server;
using PlayerEvents = Exiled.Events.Handlers.Player;
using Scp914Events = Exiled.Events.Handlers.Scp914;
using WarheadEvents = Exiled.Events.Handlers.Warhead;

namespace RoundReports
{
    public class MainPlugin : Plugin<Config>
    {
        public static Reporter Reporter { get; set; }
        public static MainPlugin Singleton { get; private set; }
        public static EventHandlers Handlers { get; private set; }

        public override void OnEnabled()
        {
            if (string.IsNullOrEmpty(Config.PasteKey))
            {
                Log.Warn("Missing paste.ee key!");
                return;
            }
            if (string.IsNullOrEmpty(Config.DiscordWebhook))
            {
                Log.Warn($"Missing Discord webhook! The plugin will still function, but only users with access to the server console get the report link.");
            }
            Singleton = this;
            Handlers = new EventHandlers();

            // Handle reporter
            ServerEvents.WaitingForPlayers += Handlers.OnWaitingForPlayers;
            ServerEvents.RestartingRound += Handlers.OnRestarting;

            // Server Events
            ServerEvents.RoundStarted += Handlers.OnRoundStarted;
            ServerEvents.RoundEnded += Handlers.OnRoundEnded;

            // Player Events
            PlayerEvents.Left += Handlers.OnLeft;
            PlayerEvents.Hurting += Handlers.OnHurting;
            PlayerEvents.Died += Handlers.OnDied;
            PlayerEvents.UsedItem += Handlers.OnUsedItem;
            PlayerEvents.InteractingDoor += Handlers.OnInteractingDoor;
            PlayerEvents.InteractingScp330 += Handlers.OnInteractingScp330;
            PlayerEvents.Escaping += Handlers.OnEscaping;

            Scp914Events.Activating += Handlers.OnActivatingScp914;
            Scp914Events.UpgradingItem += Handlers.On914UpgradingItem;
            Scp914Events.UpgradingInventoryItem += Handlers.On914UpgradingInventoryItem;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            ServerEvents.WaitingForPlayers -= Handlers.OnWaitingForPlayers;
            ServerEvents.RestartingRound -= Handlers.OnRestarting;

            if (Reporter is not null)
                Reporter.ReturnLists();

            Reporter = null;
            Singleton = null;
            Handlers = null;
            base.OnDisabled();
        }
    }

    public class Config : IConfig
    {
        [Description("Whether or not the plugin is active.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Your Paste.ee key. Get this from https://paste.ee/account/api after creating a paste.ee account.")]
        public string PasteKey { get; set; } = "";

        [Description("Provide a Discord webhook to send reports to.")]
        public string DiscordWebhook { get; set; } = string.Empty;

        [Description("Name of the server, without any formatting tags, as it will be shown in the report.")]
        public string ServerName { get; set; } = string.Empty;
    }
}
