namespace RoundReports
{
    using PlayerRoles;

    /// <summary>
    /// Custom version of <see cref="RoleTypeId"/> that includes custom roles the plugin supports.
    /// </summary>
    public enum CustomRT
    {
#pragma warning disable SA1602
        None = -1,

        // Scp Roles
        Scp008,
        Scp035,
        Scp049,
        Scp0492,
        Scp079,
        Scp096,
        Scp106,
        Scp173,
        Scp939,

        // Starting Roles
        ClassD,
        Scientist,
        FacilityGuard,

        // NTF Roles
        NtfCaptain,
        NtfSergeant,
        NtfSpecialist,
        NtfPrivate,

        // CI Roles
        ChaosMarauder,
        ChaosRepressor,
        ChaosRifleman,
        ChaosConscript,

        // Custom Respawn roles
        SerpentsHand,
        UIU,

        // Other
        Tutorial,
        Spectator,
        Overwatch,
        CustomRole,

#pragma warning restore SA1602
    }
}
