namespace RoundReports
{
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Configuration class for Discord embeds.
    /// </summary>
    public class DiscordEmbedConfig
    {
#pragma warning disable SA1600
        [Description("Set to false to disable sending an embed.")]
        public bool Enabled { get; set; } = true;

        [Description("The title of the embed.")]
        public string Title { get; set; } = "Round Report";

        [Description("The description of the embed.")]
        public string Description { get; set; } = "{WINNINGTEAM} Win\n[View Round Report]({REPORTLINK})";

        [Description("A list of fields in the embed.")]
        public List<EmbedField> Fields { get; set; } = new List<EmbedField>()
        {
            new()
            {
                Name = "Post Date",
                Value = "{POSTDATE}",
                Inline = true,
            },
            new()
            {
                Name = "Expiration Date",
                Value = "{EXPIREDATE}",
                Inline = true,
            },
        };

        [Description("The footer text of the embed.")]
        public string Footer { get; set; } = "{PLAYERCOUNT} players connected";

        [Description("Whether or not to use the winning team's color, or a custom color. Valid options: WinningTeam, Custom.")]
        public EmbedColorType EmbedColorType { get; set; } = EmbedColorType.WinningTeam;

        [Description("Custom color to use for embeds, if EmbedColorType is Custom. Takes an integer.")]
        public int CustomColor { get; set; } = -1;

        [Description("Whether or not to include timestamp in the embed footer.")]
        public bool IncludeTimestamp { get; set; } = true;
    }
#pragma warning restore SA1600
}
