using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using EBroadcast = Exiled.API.Features.Broadcast;

namespace RoundReports
{
    public enum StatType
    {
        StartTime,
        StartClassD,
        StartScientist,
        StartSCP,
        StartFacilityGuard,
        StartPlayers,
    }

    public class Config : IConfig
    {
        [Description("Whether or not the plugin is active.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Your Paste.ee key. Get this from https://paste.ee/account/api after creating a paste.ee account. The plugin cannot function without a valid Pastee key!")]
        public string PasteKey { get; set; } = "";

        [Description("Time until reports expire. Valid values: Never, 1D, 7D, 14D, 1M, 3M, 6M")]
        public string ExpiryTime { get; set; } = "1M";

        [Description("Provide a Discord webhook to send reports to.")]
        public string DiscordWebhook { get; set; } = string.Empty;
        [Description("Send report links in server console when compiled?")]
        public bool SendInConsole { get; set; } = true;
        [Description("Broadcast to show at the end of the round.")]
        public EBroadcast EndingBroadcast { get; set; } = new EBroadcast("View the end-of-round report on our Discord server!");

        [Description("Name of the server, without any formatting tags, as it will be shown in the report.")]
        public string ServerName { get; set; } = string.Empty;

        [Description("Determines the footer text, as shown on the Discord embed. Valid arguments: {PLAYERCOUNT}, {ROUNDTIME}, {TOTALKILLS}, {TOTALDEATHS}")]
        public string FooterText { get; set; } = "{PLAYERCOUNT} players connected";

        [Description("Determines the format of timestamps.")]
        public string FullTimeFormat { get; set; } = "MMMM dd, yyyy hh:mm:ss tt";
        public string ShortTimeFormat { get; set; } = "HH:mm:ss";
        [Description("Determine which statistics are included in the report.")]
        public Dictionary<StatType, bool> AllowedStats { get; set; } = (System.Enum.GetValues(typeof(StatType)) as StatType[]).ToDictionary(stat => stat, stat => true);
    }
}
