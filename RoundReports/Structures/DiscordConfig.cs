using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public class DiscordConfig
    {
        [Description("The username of the webhook posting the round reports.")]
        public string Username { get; set; } = "Round Reports";

        [Description("The avatar url of the webhook posting the round report.")]
        public string AvatarUrl { get; set; } = string.Empty;

        [Description("The content text of the round report. Roles can be pinged here.")]
        public string Content { get; set; } = string.Empty;

        [Description("Settings for the Discord embed.")]
        public DiscordEmbedConfig Embed { get; set; } = new();
    }

    public class DiscordEmbedConfig
    {
        [Description("Set to false to disable sending an embed.")]
        public bool Enabled { get; set; } = true;

        [Description("The title of the embed.")]
        public string Title { get; set; } = "Round Report";

        [Description("The description of the embed.")]
        public string Description { get; set; } = "{WINNINGTEAM} Win\n[View Round Report]({REPORTLINK})";

        [Description("The footer text of the embed.")]
        public string Footer { get; set; } = "{PLAYERCOUNT} players connected";

        [Description("Whether or not to use the winning team's color, or a custom color. Valid options: WinningTeam, Custom.")]
        public EmbedColorType EmbedColorType { get; set; } = EmbedColorType.WinningTeam;

        [Description("Custom color to use for embeds, if EmbedColorType is Custom. Takes an integer.")]
        public int CustomColor { get; set; } = -1;

        [Description("Whether or not to include timestamp in the embed footer.")]
        public bool IncludeTimestamp { get; set; } = true;
    }
}
