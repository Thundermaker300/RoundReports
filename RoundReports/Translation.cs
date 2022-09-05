using Exiled.API.Interfaces;

namespace RoundReports
{
    public class Translation : ITranslation
    {
        // General Text
        public string RoundReport { get; set; } = "Round Report";
        public string RoundRemarks { get; set; } = "Round Remarks";
        public string NoData { get; set; } = "No Data";
        public string ScpTeam { get; set; } = "SCPs";
        public string InsurgencyTeam { get; set; } = "Insurgency";
        public string MtfTeam { get; set; } = "Mobile Task Force";
        public string Stalemate { get; set; } = "Stalemate";

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
        public string SerpentsHandKills { get; set; } = "Serpent's Hand Kills";
        public string UiuKills { get; set; } = "UIU Kills";

        public string TutorialKills { get; set; } = "Tutorial Kills";

        public string SurvivingPlayers { get; set; } = "T";

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

    }
}
