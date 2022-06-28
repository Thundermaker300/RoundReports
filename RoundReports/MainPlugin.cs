using Exiled.API.Interfaces;
using Exiled.API.Features;
using System.ComponentModel;

namespace RoundReports
{
    public class MainPlugin : Plugin<Config>
    {
    }

    public class Config : IConfig
    {
        [Description("Whether or not the plugin is active.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Provide a Discord webhook to send reports to.")]
        public string DiscordWebhook { get; set; } = string.Empty;
    }
}
