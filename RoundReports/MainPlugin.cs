using Exiled.API.Interfaces;
using Exiled.API.Features;
using System.ComponentModel;

using ServerEvents = Exiled.Events.Handlers.Server;
using PlayerEvents = Exiled.Events.Handlers.Player;
using Scp096Events = Exiled.Events.Handlers.Scp096;
using Scp106Events = Exiled.Events.Handlers.Scp106;
using Scp173Events = Exiled.Events.Handlers.Scp173;
using Scp330Events = Exiled.Events.Handlers.Scp330;
using Scp914Events = Exiled.Events.Handlers.Scp914;
using WarheadEvents = Exiled.Events.Handlers.Warhead;
using EBroadcast = Exiled.API.Features.Broadcast;
using System;

namespace RoundReports
{
    public class MainPlugin : Plugin<Config, Translation>
    {
        public override string Name => "RoundReports";
        public override string Author => "Thunder";
        public override Version Version => new Version(0, 2, 2);
        public override Version RequiredExiledVersion => new Version(5, 2, 2);


        public static Reporter Reporter { get; set; }
        public static MainPlugin Singleton { get; private set; }
        public static EventHandlers Handlers { get; private set; }

        public static Translation Translations => Singleton.Translation;

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
            PlayerEvents.Dying += Handlers.OnDying;
            PlayerEvents.UsedItem += Handlers.OnUsedItem;
            PlayerEvents.InteractingDoor += Handlers.OnInteractingDoor;
            PlayerEvents.Escaping += Handlers.OnEscaping;
            PlayerEvents.DroppingItem += Handlers.OnDroppingItem;

            Scp096Events.Charging += Handlers.OnScp096Charge;
            Scp096Events.Enraging += Handlers.OnScp096Enrage;
            Scp106Events.Containing += Handlers.OnContaining106;
            Scp106Events.Teleporting += Handlers.OnScp106Teleport;
            Scp173Events.Blinking += Handlers.OnScp173Blink;

            Scp330Events.InteractingScp330 += Handlers.OnInteractingScp330;

            Scp914Events.Activating += Handlers.OnActivatingScp914;
            Scp914Events.UpgradingItem += Handlers.On914UpgradingItem;
            Scp914Events.UpgradingInventoryItem += Handlers.On914UpgradingInventoryItem;

            // Warhead
            PlayerEvents.ActivatingWarheadPanel += Handlers.OnActivatingWarheadPanel;
            WarheadEvents.Starting += Handlers.OnWarheadStarting;
            WarheadEvents.Detonated += Handlers.OnWarheadDetonated;

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
            PlayerEvents.Dying -= Handlers.OnDying;
            PlayerEvents.UsedItem -= Handlers.OnUsedItem;
            PlayerEvents.InteractingDoor -= Handlers.OnInteractingDoor;
            PlayerEvents.Escaping -= Handlers.OnEscaping;
            PlayerEvents.DroppingItem -= Handlers.OnDroppingItem;

            Scp096Events.Charging -= Handlers.OnScp096Charge;
            Scp096Events.Enraging -= Handlers.OnScp096Enrage;
            Scp106Events.Containing -= Handlers.OnContaining106;
            Scp106Events.Teleporting -= Handlers.OnScp106Teleport;
            Scp173Events.Blinking -= Handlers.OnScp173Blink;

            Scp330Events.InteractingScp330 -= Handlers.OnInteractingScp330;

            Scp914Events.Activating -= Handlers.OnActivatingScp914;
            Scp914Events.UpgradingItem -= Handlers.On914UpgradingItem;
            Scp914Events.UpgradingInventoryItem -= Handlers.On914UpgradingInventoryItem;

            // Warhead
            PlayerEvents.ActivatingWarheadPanel -= Handlers.OnActivatingWarheadPanel;
            WarheadEvents.Starting -= Handlers.OnWarheadStarting;
            WarheadEvents.Detonated -= Handlers.OnWarheadDetonated;

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
        [Description("Send report links in server console when compiled?")]
        public bool SendInConsole { get; set; } = true;
        [Description("Broadcast to show at the end of the round.")]
        public EBroadcast EndingBroadcast { get; set; } = new EBroadcast("View the end-of-round report on our Discord server!");

        [Description("Name of the server, without any formatting tags, as it will be shown in the report.")]
        public string ServerName { get; set; } = string.Empty;

        [Description("Determines the format of timestamps.")]
        public string TimeFormat { get; set; } = "MMMM dd, yyyy hh:mm:ss tt";
    }
}
