namespace RoundReports
{
    using System;
    using System.Reflection;

    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Interfaces;
    using Exiled.Loader;
    using HarmonyLib;
    using PlayerEvents = Exiled.Events.Handlers.Player;
    using Scp049Events = Exiled.Events.Handlers.Scp049;
    using Scp079Events = Exiled.Events.Handlers.Scp079;
    using Scp096Events = Exiled.Events.Handlers.Scp096;
    using Scp106Events = Exiled.Events.Handlers.Scp106;
    using Scp173Events = Exiled.Events.Handlers.Scp173;
    using Scp330Events = Exiled.Events.Handlers.Scp330;
    using Scp914Events = Exiled.Events.Handlers.Scp914;
    using Scp939Events = Exiled.Events.Handlers.Scp939;
    using ServerEvents = Exiled.Events.Handlers.Server;
    using WarheadEvents = Exiled.Events.Handlers.Warhead;

    /// <inheritdoc/>
    public class MainPlugin : Plugin<Config, Translation>
    {
        /// <summary>
        /// Gets the Serpent's Hand <see cref="Assembly"/>, if it is installed.
        /// </summary>
        public static Assembly SerpentsHandAssembly { get; private set; }

        /// <summary>
        /// Gets the UIU <see cref="Assembly"/>, if it is installed.
        /// </summary>
        public static Assembly UIURescueSquadAssembly { get; private set; }

        /// <summary>
        /// Gets or sets the currently active reporter.
        /// </summary>
        public static Reporter Reporter { get; set; }

        /// <summary>
        /// Gets the <see cref="MainPlugin"/> singleton.
        /// </summary>
        public static MainPlugin Singleton { get; private set; }

        /// <summary>
        /// Gets the <see cref="EventHandlers"/> singleton.
        /// </summary>
        public static EventHandlers Handlers { get; private set; }

        /// <summary>
        /// Gets the <see cref="HarmonyLib.Harmony"/> singleton.
        /// </summary>
        public static Harmony Harmony { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the round is currently restarting.
        /// </summary>
        public static bool IsRestarting { get; set; } = false;

        /// <summary>
        /// Gets the <see cref="Translation"/> singleton.
        /// </summary>
        public static Translation Translations => Singleton.Translation;

        /// <summary>
        /// Gets the <see cref="Config"/> singleton.
        /// </summary>
        public static Config Configs => Singleton.Config;

        /// <inheritdoc/>
        public override string Name => "RoundReports";

        /// <inheritdoc/>
        public override string Author => "Thunder";

        /// <inheritdoc/>
        public override Version Version => new(1, 0, 2);

        /// <inheritdoc/>
        public override Version RequiredExiledVersion => new(8, 8, 0);

        /// <inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Last;

        /// <summary>
        /// Checks if a stat is enabled.
        /// </summary>
        /// <param name="type">The stat to check.</param>
        /// <returns>If it is enabled.</returns>
        public static bool Check(StatType type) => !Configs.IgnoredStats.Contains(type);

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            if (string.IsNullOrEmpty(Config.PasteKey))
            {
                Log.Warn("Missing paste.ee key! RoundReports cannot function without a valid paste.ee key.");
                return;
            }

            if (Config.DiscordWebhooks.IsEmpty())
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
            PlayerEvents.InteractingElevator += Handlers.OnInteractingElevator;
            PlayerEvents.InteractingLocker += Handlers.OnInteractingLocker;
            PlayerEvents.Escaping += Handlers.OnEscaping;
            PlayerEvents.DroppingItem += Handlers.OnDroppingItem;
            PlayerEvents.EnteringPocketDimension += Handlers.OnEnteringPocketDimension;
            PlayerEvents.VoiceChatting += Handlers.OnVoiceChatting;

            PlayerEvents.Shooting += Handlers.OnShooting;
            PlayerEvents.ReloadingWeapon += Handlers.OnReloadingWeapon;
            PlayerEvents.UnlockingGenerator += Handlers.OnUnlockingGenerator;
            PlayerEvents.PlayerDamageWindow += Handlers.OnDamagingWindow;

            Scp049Events.FinishingRecall += Handlers.OnScp049Recalling;

            Scp079Events.GainingLevel += Handlers.OnScp079GainingLevel;
            Scp079Events.TriggeringDoor += Handlers.OnScp079TriggeringDoor;
            Scp079Events.InteractingTesla += Handlers.OnScp079InteractTesla;
            Scp079Events.ChangingCamera += Handlers.OnScp079ChangingCamera;

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

            // Harmony patching
            try
            {
                Harmony = new Harmony($"thunder.roundreports.{DateTime.UtcNow.Ticks}");
                Harmony.PatchAll();
                Log.Info("Harmony patching success!");
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patching failed! {e}");
            }

            ScriptedEventsIntegration.AddCustomActions();
            base.OnEnabled();
        }

        /// <inheritdoc/>
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
            PlayerEvents.InteractingElevator -= Handlers.OnInteractingElevator;
            PlayerEvents.InteractingLocker -= Handlers.OnInteractingLocker;
            PlayerEvents.Escaping -= Handlers.OnEscaping;
            PlayerEvents.DroppingItem -= Handlers.OnDroppingItem;
            PlayerEvents.EnteringPocketDimension -= Handlers.OnEnteringPocketDimension;
            PlayerEvents.VoiceChatting -= Handlers.OnVoiceChatting;

            PlayerEvents.Shooting -= Handlers.OnShooting;
            PlayerEvents.ReloadingWeapon -= Handlers.OnReloadingWeapon;
            PlayerEvents.UnlockingGenerator -= Handlers.OnUnlockingGenerator;
            PlayerEvents.PlayerDamageWindow -= Handlers.OnDamagingWindow;

            Scp049Events.FinishingRecall -= Handlers.OnScp049Recalling;

            Scp079Events.GainingLevel -= Handlers.OnScp079GainingLevel;
            Scp079Events.TriggeringDoor -= Handlers.OnScp079TriggeringDoor;
            Scp079Events.InteractingTesla -= Handlers.OnScp079InteractTesla;
            Scp079Events.ChangingCamera -= Handlers.OnScp079ChangingCamera;

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

            Reporter?.Kill();
            Harmony.UnpatchAll();

            Singleton = null;
            Handlers = null;
            Harmony = null;

            ScriptedEventsIntegration.UnregisterCustomActions();
            base.OnDisabled();
        }
    }
}
