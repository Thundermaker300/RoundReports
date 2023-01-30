namespace RoundReports
{
    using PlayerRoles;

    /// <summary>
    /// Custom version of <see cref="Team"/> that includes custom teams the plugin supports.
    /// </summary>
    public enum CustomTeam
    {
#pragma warning disable SA1602
        // Base Game
        SCPs,
        FoundationForces,
        ChaosInsurgency,
        Scientists,
        ClassD,
        Dead,
        OtherAlive,

        // Custom
        SH,
        UIU,
#pragma warning restore SA1602
    }
}
