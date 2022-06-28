using Exiled.API.Interfaces;
using Exiled.API.Features;
using System.ComponentModel;

namespace RoundReports
{
    public class MainPlugin : Plugin<Config>
    {
        public static Reporter Reporter { get; set; }
        public static MainPlugin Singleton { get; private set; }

        public override void OnEnabled()
        {
            Singleton = this;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Singleton = null;
            base.OnDisabled();
        }
    }

    public class Config : IConfig
    {
        [Description("Whether or not the plugin is active.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Provide a Discord webhook to send reports to.")]
        public string DiscordWebhook { get; set; } = string.Empty;

        [Description("Name of the server, without any formatting tags, as it will be shown in the report.")]
        public string ServerName { get; set; } = string.Empty;
    }
}
