using Exiled.API.Interfaces;
using Exiled.API.Features;
using System.ComponentModel;

using ServerEvents = Exiled.Events.Handlers.Server;
using PlayerEvents = Exiled.Events.Handlers.Player;
using Scp330Events = Exiled.Events.Handlers.Scp330;
using Scp914Events = Exiled.Events.Handlers.Scp914;
using WarheadEvents = Exiled.Events.Handlers.Warhead;
using System;

namespace RoundReports
{
    public class MainPlugin : Plugin<Config>
    {
        public override string Name => "RoundReports";
        public override string Author => "Thunder";
        public override Version Version => new Version(0, 1, 0);
        public override Version RequiredExiledVersion => new Version(5, 2, 2);


        public static Reporter Reporter { get; set; }
        public static MainPlugin Singleton { get; private set; }
        public static EventHandlers Handlers { get; private set; }

        public override void OnEnabled()
        {
            if (string.IsNullOrEmpty(Config.PasteKey))
            {
                Log.Warn("Missing paste.ee key! RoundReports cannot function without a valid paste.ee key.");
                return;
            }
            if (string.IsNullOrEmpty(Config.DiscordWebhook))
            {
                if (!Config.SendInConsole)
                {
                    Log.Warn("Missing Discord webhook, and console sending is disabled. RoundReports cannot function without a Discord webhook and with console sending disabled.");
                    return;
                }
                Log.Warn($"Missing Discord webhook! RoundReports will still function, but only users with access to the server console/server logs will receive the report link.");
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
            PlayerEvents.Escaping += Handlers.OnEscaping;
            Scp330Events.InteractingScp330 += Handlers.OnInteractingScp330;

            Scp914Events.Activating += Handlers.OnActivatingScp914;
            Scp914Events.UpgradingItem += Handlers.On914UpgradingItem;
            Scp914Events.UpgradingInventoryItem += Handlers.On914UpgradingInventoryItem;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            // Handle reporter
            ServerEvents.WaitingForPlayers -= Handlers.OnWaitingForPlayers;
            ServerEvents.RestartingRound -= Handlers.OnRestarting;

            // Server Events
            ServerEvents.RoundStarted -= Handlers.OnRoundStarted;
            ServerEvents.RoundEnded -= Handlers.OnRoundEnded;

            // Player Events
            PlayerEvents.Left -= Handlers.OnLeft;
            PlayerEvents.Hurting -= Handlers.OnHurting;
            PlayerEvents.Died -= Handlers.OnDied;
            PlayerEvents.UsedItem -= Handlers.OnUsedItem;
            PlayerEvents.InteractingDoor -= Handlers.OnInteractingDoor;
            PlayerEvents.Escaping -= Handlers.OnEscaping;
            Scp330Events.InteractingScp330 -= Handlers.OnInteractingScp330;

            Scp914Events.Activating -= Handlers.OnActivatingScp914;
            Scp914Events.UpgradingItem -= Handlers.On914UpgradingItem;
            Scp914Events.UpgradingInventoryItem -= Handlers.On914UpgradingInventoryItem;

            if (Reporter is not null)
                Reporter.Kill();

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
        [Description("Send report links in server console when compiled.")]
        public bool SendInConsole { get; set; } = true;

        [Description("Name of the server, without any formatting tags, as it will be shown in the report.")]
        public string ServerName { get; set; } = string.Empty;
    }
}
