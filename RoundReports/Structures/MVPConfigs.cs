namespace RoundReports
{
#pragma warning disable SA1600
    using System.Collections.Generic;
    using System.ComponentModel;
    using Exiled.API.Features;
    using PlayerRoles;

    /// <summary>
    /// Configuration class for MVP settings.
    /// </summary>
    public class MVPConfigs
    {
        [Description("Whether or not the MVP system is enabled.")]
        public bool MvpEnabled { get; set; } = true;

        // TODO Make this config work.
        [Description("Roles that cannot gain or lose points. Plugin-based roles are not affected by this setting.")]
        public List<RoleTypeId> RoleBlacklist { get; set; } = new() { RoleTypeId.Tutorial };

        [Description("Minimum amount of points per round that a user can have.")]
        public int MinPoints { get; set; } = -500;

        [Description("Maximum amount of points per round that a user can have.")]
        public int MaxPoints { get; set; } = 500;

        [Description("Hint to show to a player when they gain points.")]
        public Hint AddHint { get; set; } = new Hint
        {
            Content = "+{POINTS} points\nReason: {REASON}",
            Duration = 3f,
            Show = true,
        };

        [Description("Hint to show to a player when they lose points.")]
        public Hint RemoveHint { get; set; } = new Hint
        {
            Content = "{POINTS} points\nReason: {REASON}",
            Duration = 3f,
            Show = true,
        };

        [Description("Determines the amount of points that are gained/lost for a certain interaction.")]
        public MVPPointsConfigs Points { get; set; } = new();
    }
#pragma warning restore SA1600
}
