﻿using System.ComponentModel;

namespace RoundReports
{
    public class MVPPointsConfigs
    {
        // Add points
        [Description("Stats to add points.")]
        public int KillScp { get; set; } = 10;
        public int KillEnemy { get; set; } = 2;
        public int KillScientist { get; set; } = 3;
        public int OpenWarheadPanel { get; set; } = 2;
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
}