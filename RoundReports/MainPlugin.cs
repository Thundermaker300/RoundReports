using Exiled.API.Interfaces;
using Exiled.API.Features;

using ServerEvents = Exiled.Events.Handlers.Server;
using PlayerEvents = Exiled.Events.Handlers.Player;
using Scp079Events = Exiled.Events.Handlers.Scp079;
using Scp096Events = Exiled.Events.Handlers.Scp096;
using Scp106Events = Exiled.Events.Handlers.Scp106;
using Scp173Events = Exiled.Events.Handlers.Scp173;
using Scp330Events = Exiled.Events.Handlers.Scp330;
using Scp914Events = Exiled.Events.Handlers.Scp914;
using Scp939Events = Exiled.Events.Handlers.Scp939;
using WarheadEvents = Exiled.Events.Handlers.Warhead;
using System;
using Exiled.Loader;
using System.Reflection;
using Exiled.API.Enums;

namespace RoundReports
{
    public class MainPlugin : Plugin<Config, Translation>
    {
        public override string Name => "RoundReports";
        public override string Author => "Thunder";
        public override Version Version => new(0, 5, 5);
        public override Version RequiredExiledVersion => new(6, 0, 0);
        public override PluginPriority Priority => PluginPriority.Last;


        public static Reporter Reporter { get; set; }
        public static MainPlugin Singleton { get; private set; }
        public static EventHandlers Handlers { get; private set; }
        public static bool IsRestarting { get; set; } = false;

        public static Assembly SerpentsHandAssembly;
        public static Assembly UIURescueSquadAssembly;

        public static Translation Translations => Singleton.Translation;
        public static Config Configs => Singleton.Config;

        public static bool Check(StatType type) => !Configs.IgnoredStats.Contains(type);

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
            ServerEvents.RespawningTeam += Handlers.OnRespawningTeam;

            // Player Events
            PlayerEvents.Left += Handlers.OnLeft;
            PlayerEvents.Spawned += Handlers.OnSpawned;
            PlayerEvents.Hurting += Handlers.OnHurting;
            PlayerEvents.Dying += Handlers.OnDying;
            PlayerEvents.UsedItem += Handlers.OnUsedItem;
            PlayerEvents.InteractingDoor += Handlers.OnInteractingDoor;
            PlayerEvents.InteractingLocker += Handlers.OnInteractingLocker;
            PlayerEvents.Escaping += Handlers.OnEscaping;
            PlayerEvents.DroppingItem += Handlers.OnDroppingItem;
            PlayerEvents.EnteringPocketDimension += Handlers.OnEnteringPocketDimension;

            PlayerEvents.Shooting += Handlers.OnShooting;
            PlayerEvents.ReloadingWeapon += Handlers.OnReloadingWeapon;

            Scp079Events.GainingLevel += Handlers.OnScp079GainingLevel;
            //Scp079Events.LockingDown += Handlers.OnScp079Lockdown;
            Scp079Events.TriggeringDoor += Handlers.OnScp079TriggeringDoor;
            Scp079Events.InteractingTesla += Handlers.OnScp079InteractTesla;

            Scp096Events.Charging += Handlers.OnScp096Charge;
            Scp096Events.Enraging += Handlers.OnScp096Enrage;

            Scp106Events.Teleporting += Handlers.OnScp106Teleport;

            Scp173Events.Blinking += Handlers.OnScp173Blink;
            Scp173Events.PlacingTantrum += Handlers.OnScp173Tantrum;

            Scp330Events.InteractingScp330 += Handlers.OnInteractingScp330;

            Scp914Events.Activating += Handlers.OnActivatingScp914;
            Scp914Events.ChangingKnobSetting += Handlers.OnChangingKnobSetting;
            Scp914Events.UpgradingPickup += Handlers.On914UpgradingPickup;
            Scp914Events.UpgradingInventoryItem += Handlers.On914UpgradingInventoryItem;

            Scp939Events.Lunging += Handlers.OnScp939Lunge;
            Scp939Events.PlacingAmnesticCloud += Handlers.OnScp939Cloud;

            // Warhead
            PlayerEvents.ActivatingWarheadPanel += Handlers.OnActivatingWarheadPanel;
            WarheadEvents.Starting += Handlers.OnWarheadStarting;
            WarheadEvents.Detonated += Handlers.OnWarheadDetonated;

            // Load SH and UIU assemblies
            // Credit to RespawnTimer for this code
            foreach (IPlugin<IConfig> plugin in Loader.Plugins)
            {
                if (plugin.Name == "SerpentsHand" && plugin.Config.IsEnabled)
                {
                    Log.Debug("Serpent's Hand Detected");
                    SerpentsHandAssembly = plugin.Assembly;
                }

                if (plugin.Name == "UIURescueSquad" && plugin.Config.IsEnabled)
                {
                    Log.Debug("UIU Rescue Squad");
                    UIURescueSquadAssembly = plugin.Assembly;
                }
            }


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
            ServerEvents.RespawningTeam -= Handlers.OnRespawningTeam;

            // Player Events
            PlayerEvents.Left -= Handlers.OnLeft;
            PlayerEvents.Hurting -= Handlers.OnHurting;
            PlayerEvents.Spawned -= Handlers.OnSpawned;
            PlayerEvents.Dying -= Handlers.OnDying;
            PlayerEvents.UsedItem -= Handlers.OnUsedItem;
            PlayerEvents.InteractingDoor -= Handlers.OnInteractingDoor;
            PlayerEvents.InteractingLocker -= Handlers.OnInteractingLocker;
            PlayerEvents.Escaping -= Handlers.OnEscaping;
            PlayerEvents.DroppingItem -= Handlers.OnDroppingItem;
            PlayerEvents.EnteringPocketDimension -= Handlers.OnEnteringPocketDimension;

            PlayerEvents.Shooting -= Handlers.OnShooting;
            PlayerEvents.ReloadingWeapon -= Handlers.OnReloadingWeapon;

            Scp079Events.GainingLevel -= Handlers.OnScp079GainingLevel;
            //Scp079Events.LockingDown -= Handlers.OnScp079Lockdown;
            Scp079Events.TriggeringDoor -= Handlers.OnScp079TriggeringDoor;
            Scp079Events.InteractingTesla -= Handlers.OnScp079InteractTesla;

            Scp096Events.Charging -= Handlers.OnScp096Charge;
            Scp096Events.Enraging -= Handlers.OnScp096Enrage;

            Scp106Events.Teleporting -= Handlers.OnScp106Teleport;

            Scp173Events.Blinking -= Handlers.OnScp173Blink;
            Scp173Events.PlacingTantrum -= Handlers.OnScp173Tantrum;

            Scp330Events.InteractingScp330 -= Handlers.OnInteractingScp330;

            Scp914Events.Activating -= Handlers.OnActivatingScp914;
            Scp914Events.ChangingKnobSetting -= Handlers.OnChangingKnobSetting;
            Scp914Events.UpgradingPickup -= Handlers.On914UpgradingPickup;
            Scp914Events.UpgradingInventoryItem -= Handlers.On914UpgradingInventoryItem;

            Scp939Events.Lunging -= Handlers.OnScp939Lunge;
            Scp939Events.PlacingAmnesticCloud -= Handlers.OnScp939Cloud;

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
}
