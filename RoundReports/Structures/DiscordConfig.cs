namespace RoundReports
{
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Configuration class for basic Discord settings.
    /// </summary>
    public class DiscordConfig
    {
#pragma warning disable SA1600
        [Description("The username of the webhook posting the round reports.")]
        public string Username { get; set; } = "Round Reports";

        [Description("The avatar url of the webhook posting the round report.")]
        public string AvatarUrl { get; set; } = string.Empty;

        [Description("The content text of the round report. Roles can be pinged here.")]
        public string Content { get; set; } = string.Empty;

        [Description("Settings for the Discord embed.")]
        public DiscordEmbedConfig Embed { get; set; } = new();
    }
#pragma warning restore SA1600
}
