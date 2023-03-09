namespace RoundReports
{
#pragma warning disable SA1600
    using System.ComponentModel;

    /// <summary>
    /// Configuration class for MVP points settings.
    /// </summary>
    public class MVPPointsConfigs
    {
        [Description("The amount of damage required to be dealt to an SCP before the 'hurt_scp' points are added.")]
        public int HurtScpRequired { get; set; } = 600;

        // Add points
        [Description("Stats to add points.")]
        public int HurtScp { get; set; } = 5;

        public int KillEnemy { get; set; } = 2;

        public int KillScientist { get; set; } = 3;

        public int OpenWarheadPanel { get; set; } = 2;

        public int UnlockGenerator { get; set; } = 1;

        public int RecontainScp079 { get; set; } = 3;

        public int Escaped { get; set; } = 5;

        public int Scp079LeveledUp { get; set; } = 5;

        public int Scp079TeslaKill { get; set; } = 5;

        public int Scp079AssistKill { get; set; } = 1;

        public int Scp079OpenGate { get; set; } = 2;

        public int Scp079OpenKeycardDoor { get; set; } = 1;

        public int Scp106GrabPlayer { get; set; } = 1;

        // Remove points
        [Description("Stats to remove points.")]
        public int KillTeammate { get; set; } = -10;

        public int Took3Candies { get; set; } = -10;

        public int Died { get; set; } = -1;

        public int DiedDumb { get; set; } = -2;

        public int ScpDied { get; set; } = -5;

        public int ScpDiedDumb { get; set; } = -10;
    }
#pragma warning restore SA1600
}
