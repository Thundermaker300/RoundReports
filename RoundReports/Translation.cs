namespace RoundReports
{
    using System.ComponentModel;
    using Exiled.API.Interfaces;

#pragma warning disable SA1600 // Elements should be documented
    public class Translation : ITranslation
    {
        // General Text
        public string RoundReport { get; set; } = "Round Report";

        public string RoundRemarks { get; set; } = "Round Remarks";

        [Description("Indicates a stat that was not collected - eg. if no candy was taken, no kills, etc")]
        public string NoData { get; set; } = "No Data";

        [Description("Used in place of no data stats if the statistic is a player.")]
        public string Nobody { get; set; } = "Nobody";

        [Description("User's name is replaced with this if they have Do Not Track enabled.")]
        public string DoNotTrack { get; set; } = "[DO NOT TRACK USER]";

        public string ScpTeam { get; set; } = "SCPs";

        public string InsurgencyTeam { get; set; } = "Insurgency";

        public string MtfTeam { get; set; } = "Mobile Task Force";

        public string Stalemate { get; set; } = "Stalemate";

        public string Yes { get; set; } = "Yes";

        public string No { get; set; } = "No";

        [Description("Only shown on Discord embed if expiration is set to \"Never\" in configs.")]
        public string Never { get; set; } = "Never";

        [Description("Generally should never show, but JUST in case.")]
        public string Unknown { get; set; } = "Unknown";

        // Webhook Text
        public string WinText { get; set; } = "{TEAM} Win";

        // Door Stats
        public string DoorStatsTitle { get; set; } = "Door Statistics";

        public string DoorsOpened { get; set; } = "Doors Opened";

        public string DoorsClosed { get; set; } = "Doors Closed";

        public string DoorsDestroyed { get; set; } = "Doors Destroyed";

        public string PlayerDoorsOpened { get; set; } = "Player Doors Opened";

        public string PlayerDoorsClosed { get; set; } = "Player Doors Closed";

        // Final Stats
        public string FinalStatsTitle { get; set; } = "Final Statistics";

        public string EndofRoundSummary { get; set; } = "End-of-Round Summary";

        public string WinningTeam { get; set; } = "Winning Team";

        public string EndTime { get; set; } = "End Time";

        public string RoundTime { get; set; } = "Round Time";

        public string TotalDeaths { get; set; } = "Total Deaths";

        public string TotalKills { get; set; } = "Total Kills";

        public string ScpKills { get; set; } = "SCP Kills";

        public string DClassKills { get; set; } = "Class-D Kills";

        public string ScientistKills { get; set; } = "Scientist Kills";

        public string MtfKills { get; set; } = "MTF Kills";

        public string ChaosKills { get; set; } = "Chaos Kills";

        [Description("This text will only be visible if the Serpents Hand plugin is also installed.")]
        public string SerpentsHandKills { get; set; } = "Serpents Hand Kills";

        [Description("This text will only be visible if the UIU plugin is also installed.")]
        public string UiuKills { get; set; } = "UIU Kills";

        [Description("This text will only be visible if at least one tutorial (not Serpents Hand or UIU) kills a player in the round.")]
        public string TutorialKills { get; set; } = "Tutorial Kills";

        public string SurvivingPlayers { get; set; } = "Surviving Players";

        public string MostTalkativePlayer { get; set; } = "Most Talkative Player";

        public string TotalInteractions { get; set; } = "Total Interactions";

        // Item Stats
        public string ItemStatsTitle { get; set; } = "Item Statistics";

        public string ItemTransfersTitle { get; set; } = "Item Transfers";

        public string ItemUsesTitle { get; set; } = "Item Uses";

        public string TotalDrops { get; set; } = "Total Drops";

        public string Drops { get; set; } = "Drops";

        public string PlayerDrops { get; set; } = "Player Drops";

        public string KeycardScans { get; set; } = "Keycard Scans";

        public string PainkillersConsumed { get; set; } = "Painkillers Consumed";

        public string MedkitsConsumed { get; set; } = "Medkits Consumed";

        public string AdrenalinesConsumed { get; set; } = "Adrenalines Consumed";

        public string Scp500sConsumed { get; set; } = "SCP-500s Consumed";

        // Firearm Stats
        public string FirearmTitle { get; set; } = "Firearm Stats";

        public string TotalShotsFired { get; set; } = "Total Shots Fired";

        public string TotalReloads { get; set; } = "Total Reloads";

        public string AverageShotsPerFirearm { get; set; } = "Avg. Shots Per Firearm";

        public string ShotsByFirearm { get; set; } = "Shots by Firearm";

        // Organized Damage Stats
        public string OrganizedDamageTitle { get; set; } = "Damage Dealt";

        public string TotalDamage { get; set; } = "Total Damage";

        public string PlayerDamage { get; set; } = "Player Damage";

        public string AverageDamagePerPlayer { get; set; } = "Avg. Damage Per Player";

        public string DamageByPlayer { get; set; } = "Damage By Player";

        public string DamageByType { get; set; } = "Damage By Type";

        // Organized Kill Stats
        public string OrganizedKillsTitle { get; set; } = "Kills";

        public string KillsByPlayer { get; set; } = "Kills By Player";

        public string KillsbyType { get; set; } = "Kills By Type";

        public string KillsByZone { get; set; } = "Kills By Zone";

        public string PlayerKills { get; set; } = "Player Kills";

        // SCP-330 Stats
        public string Scp330Title { get; set; } = "SCP-330 Statistics";

        public string FirstUse { get; set; } = "First Use";

        public string FirstUser { get; set; } = "First User";

        public string TotalCandiesTaken { get; set; } = "Total Candies Taken";

        public string SeveredHands { get; set; } = "Severed Hands";

        public string CandiesTaken { get; set; } = "Candies Taken";

        public string CandiesByPlayer { get; set; } = "Candies By Player";

        // SCP-914 Stats
        public string Scp914Title { get; set; } = "SCP-914 Statistics";

        public string FirstActivation { get; set; } = "First Activation";

        public string FirstActivator { get; set; } = "First Activator";

        public string TotalActivations { get; set; } = "Total Activations";

        public string TotalItemUpgrades { get; set; } = "Total Item Upgrades";

        public string KeycardUpgrades { get; set; } = "Keycard Upgrades";

        public string FirearmUpgrades { get; set; } = "Firearm Upgrades";

        public string Activations { get; set; } = "Activations";

        public string Upgrades { get; set; } = "Upgrades";

        // SCP Item Stats
        public string ScpTitle { get; set; } = "SCP Statistics";

        public string ScpItemTitle { get; set; } = "SCP Item Statistics";

        public string Scp018Thrown { get; set; } = "SCP-018s Thrown";

        public string Scp207Drank { get; set; } = "SCP-207s Drank";

        public string Scp268Uses { get; set; } = "SCP-268 Uses";

        public string Scp1853Uses { get; set; } = "SCP-1853 Uses";

        // SCP Stats
        [Description("The following SCP-related stats only show on the report if each is activated at least once.")]
        public string Scp049Revives { get; set; } = "SCP-049 Revives";

        public string Scp079Tier { get; set; } = "SCP-079 Tier";

        public string Scp079CamerasUsed { get; set; } = "SCP-079 Cameras Used";

        public string Scp079MostUsedCamera { get; set; } = "SCP-079 Most Used Camera";

        public string Scp096Charges { get; set; } = "SCP-096 Charges";

        public string Scp096Enrages { get; set; } = "SCP-096 Enrages";

        public string Scp106Teleports { get; set; } = "SCP-106 Teleports";

        public string Scp173Blinks { get; set; } = "SCP-173 Blinks";

        public string Scp173Tantrums { get; set; } = "SCP-173 Tantrums";

        public string Scp939Lunges { get; set; } = "SCP-939 Lunges";

        public string Scp939Clouds { get; set; } = "SCP-939 Amnestic Clouds";

        // Starting Stats
        public string StartingTitle { get; set; } = "Starting Statistics";

        public string StartTime { get; set; } = "Start Time";

        public string PlayersAtStart { get; set; } = "Players At Start";

        public string ClassDPersonnel { get; set; } = "Class-D Personnel";

        public string Scps { get; set; } = "SCPs";

        public string Scientists { get; set; } = "Scientists";

        public string FacilityGuards { get; set; } = "Facility Guards";

        public string Players { get; set; } = "Players";

        // Warhead Stats
        public string WarheadStatsTitle { get; set; } = "Warhead Statistics";

        public string ButtonUnlocked { get; set; } = "Button Unlocked";

        public string ButtonUnlocker { get; set; } = "Button Unlocker";

        public string Detonated { get; set; } = "Detonated";

        public string DetonationTime { get; set; } = "Detonation Time";

        // Miscellaneous Stats
        public string MiscTitle { get; set; } = "Miscellaneous Stats";

        public string RespawnTitle { get; set; } = "Respawns";

        public string SpawnWaves { get; set; } = "Spawn Waves";

        public string TotalRespawnedPlayers { get; set; } = "Total Respawned Players";

        public string Respawns { get; set; } = "Respawns";

        public string RespawnLog { get; set; } = "{PLAYER} respawned as {ROLE}.";

        public string MapTitle { get; set; } = "Map Stats";

        public string TotalRooms { get; set; } = "Total Rooms";

        public string RoomsByZone { get; set; } = "Rooms By Zone";

        public string TotalCameras { get; set; } = "Total Cameras";

        public string TotalDoors { get; set; } = "Total Doors";

        public string TeslaTitle { get; set; } = "Tesla Gates";

        public string TotalTeslaGates { get; set; } = "Total Tesla Gates";

        public string TeslaShocks { get; set; } = "Total Tesla Gate Shocks";

        public string TeslaDamage { get; set; } = "Total Tesla Gate Damage";

        // MVPs
        public string MVPTitle { get; set; } = "Round MVPs";

        public string HumanMVP { get; set; } = "Human MVP";

        public string SCPMVP { get; set; } = "SCP MVP";

        public string HumanPoints { get; set; } = "Human Points";

        public string SCPPoints { get; set; } = "SCP Points";

        // Remarks
        public string HiddenUsersNotice { get; set; } = "[Notice] Due to server settings, some users and their statistics have been hidden from this report.";

        public string DoorRemark { get; set; } = "{PLAYER} [{ROLE}] was the first to open a door ({MILLISECOND}ms)!";

        public string EscapeRemark { get; set; } = "{PLAYER} [{ROLE}] was the first to escape ({MINUTE}m{SECOND}s)!";

        public string KillRemark { get; set; } = "{PLAYER} [{ROLE}] killed first! They killed {TARGET} [{TARGETROLE}].";

        public string UpgradeRemark { get; set; } = "The first item to be upgraded in SCP-914 was an {ITEM} on {MODE}.";

        // MVP Points reasons
        public string AddPointsLog { get; set; } = "{PLAYER} [{ROLE}] gained {AMOUNT} points. Reason: {REASON}";

        public string RemovePointsLog { get; set; } = "{PLAYER} [{ROLE}] lost {AMOUNT} points. Reason: {REASON}";

        [Description("The following translations are for reasons regarding to add & removing MVP points.")]

        //-- Positive
        public string HurtSCP { get; set; } = "Significantly Hurt SCP";

        public string KilledScientist { get; set; } = "Killed Scientist";

        public string KilledEnemy { get; set; } = "Killed Enemy";

        public string RecontainScp079 { get; set; } = "Recontain SCP-079";

        public string Escaped { get; set; } = "Escaped";

        public string OpenedWarheadPanel { get; set; } = "Opened Warhead Panel";

        public string UnlockedGenerator { get; set; } = "Unlocked Generator";

        public string GrabbedPlayer { get; set; } = "Grabbed Player";

        public string LeveledUp { get; set; } = "Leveled Up";

        public string AssistKill { get; set; } = "Assist Kill";

        public string OpenedDoor { get; set; } = "Opened Door";

        public string TeslaKill { get; set; } = "Tesla Gate Kill";

        //-- Negative
        public string Death { get; set; } = "Death";

        public string Took3Candies { get; set; } = "Took 3 Candies";

        public string KilledTeammate { get; set; } = "Killed Teammate";
    }
#pragma warning restore SA1600 // Elements should be documented
}
