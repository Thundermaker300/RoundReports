using Exiled.API.Interfaces;
using Exiled.API.Features;
using System.ComponentModel;

using ServerEvents = Exiled.Events.Handlers.Server;

namespace RoundReports
{
    public class MainPlugin : Plugin<Config>
    {
        public static Reporter Reporter { get; set; }
        public static MainPlugin Singleton { get; private set; }
        public static EventHandlers Handlers { get; private set; }

        public override void OnEnabled()
        {
            if (string.IsNullOrEmpty(Config.PastebinKey))
            {
                Log.Warn("Missing pastebin key!");
                return;
            }
            Singleton = this;
            Handlers = new EventHandlers();

            ServerEvents.WaitingForPlayers += Handlers.OnWaitingForPlayers;
            ServerEvents.RestartingRound += Handlers.OnRestarting;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            ServerEvents.WaitingForPlayers -= Handlers.OnWaitingForPlayers;
            ServerEvents.RestartingRound -= Handlers.OnRestarting;

            Reporter = null;
            Singleton = null;
            Handlers = null;
            base.OnDisabled();
        }
    }

    public class Config : IConfig
    {
        [Description("Whether or not the plugin is active.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Your Pastebin key. Get this from https://pastebin.com/doc_api after creating a pastebin account. Does not give access to your pastebin account.")]
        public string PastebinKey { get; set; } = "";

        [Description("Provide a Discord webhook to send reports to.")]
        public string DiscordWebhook { get; set; } = string.Empty;

        [Description("Name of the server, without any formatting tags, as it will be shown in the report.")]
        public string ServerName { get; set; } = string.Empty;
    }
}
