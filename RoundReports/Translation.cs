﻿using Exiled.API.Interfaces;
using System.ComponentModel;

namespace RoundReports
{
    public class Translation : ITranslation
    {
        // General Text
        public string RoundReport { get; set; } = "Round Report";
        public string RoundRemarks { get; set; } = "Round Remarks";
        [Description("Indicates a stat that was not collected - eg. if no candy was taken, no kills, etc")]
        public string NoData { get; set; } = "No Data";

        [Description("User's name is replaced with this if they have Do Not Track enabled.")]
        public string DoNotTrack { get; set; } = "[DO NOT TRACK USER]";
        public string ScpTeam { get; set; } = "SCPs";
        public string InsurgencyTeam { get; set; } = "Insurgency";
        public string MtfTeam { get; set; } = "Mobile Task Force";
        public string Stalemate { get; set; } = "Stalemate";
        public string Yes { get; set; } = "Yes";
        public string No { get; set; } = "No";

        [Description("Generally should never show, but JUST in case.")]
        public string Unknown { get; set; } = "Unknown";

        // Webhook Text
        public string PostDate { get; set; } = "Post Date";
        public string ExpireDate { get; set; } = "Expire Date";
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

        // Medical Stats
        public string MedicalStatsTitle { get; set; } = "Medical Statistics";
        public string PainkillersConsumed { get; set; } = "Painkillers Consumed";
        public string MedkitsConsumed { get; set; } = "Medkits Consumed";
        public string AdrenalinesConsumed { get; set; } = "Adrenalines Consumed";
        public string Scp500sConsumed { get; set; } = "SCP-500s Consumed";

        // Organized Damage Stats
        public string OrganizedDamageTitle { get; set; } = "Damage Dealt";
        public string TotalDamage { get; set; } = "Total Damage";
        public string PlayerDamage { get; set; } = "Player Damage";
        public string DamageByPlayer { get; set; } = "Damage By Player";
        public string DamageByType { get; set; } = "Damage By Type";

        // Organized Kill Stats
        public string OrganizedKillsTitle { get; set; } = "Kills";
        public string KillsByPlayer { get; set; } = "Kills By Player";
        public string KillsbyType { get; set; } = "Kills By Type";
        public string PlayerKills { get; set; } = "Player Kills";

        // SCP-330 Stats
        public string Scp330Title { get; set; } = "SCP-330 Stats";
        public string FirstUse { get; set; } = "First Use";
        public string FirstUser { get; set; } = "First User";
        public string TotalCandiesTaken { get; set; } = "Total Candies Taken";
        public string SeveredHands { get; set; } = "Severed Hands";
        public string CandiesTaken { get; set; } = "Candies Taken";
        public string CandiesByPlayer { get; set; } = "Candies By Player";

        // SCP-914 Stats
        public string Scp914Title { get; set; } = "SCP-914 Stats";
        public string FirstActivation { get; set; } = "First Activation";
        public string FirstActivator { get; set; } = "First Activator";
        public string TotalActivations { get; set; } = "Total Activations";
        public string TotalItemUpgrades { get; set; } = "Total Item Upgrades";
        public string KeycardUpgrades { get; set; } = "Keycard Upgrades";
        public string FirearmUpgrades { get; set; } = "Firearm Upgrades";
        public string Activations { get; set; } = "Activations";
        public string Upgrades { get; set; } = "Upgrades";

        // SCP Item Stats
        public string ScpItemTitle { get; set; } = "SCP Item Statistics";
        public string Scp018Thrown { get; set; } = "SCP-018s Thrown";
        public string Scp207Drank { get; set; } = "SCP-207s Drank";
        public string Scp268Uses { get; set; } = "SCP-268 Uses";
        public string Scp1853Uses { get; set; } = "SCP-1853 Uses";

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

        // Remarks
        public string DoorRemark { get; set; } = "{PLAYER} [{ROLE}] was the first to open a door ({MILLISECOND}ms)!";
        public string EscapeRemark { get; set; } = "{PLAYER} [{ROLE}] was the first to escape ({MINUTE}m{SECOND}s)!";
        public string KillRemark { get; set; } = "{PLAYER} [{ROLE}] killed first! They killed {TARGET} [{TARGETROLE}].";
        public string UpgradeRemark { get; set; } = "The first item to be upgraded in SCP-914 was an {ITEM} on {MODE}.";
    }
}