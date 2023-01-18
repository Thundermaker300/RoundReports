namespace RoundReports
{
    public enum StatType
    {
        // Start Stats
        StartTime,
        StartClassD,
        StartScientist,
        StartSCP,
        StartFacilityGuard,
        StartPlayers,

        // MVP Stats
        HumanMVP,
        SCPMVP,
        HumanPoints,
        SCPPoints,
        PointLogs,

        // Final Stats
        WinningTeam,
        EndTime,
        RoundTime,
        TotalDeaths,
        TotalKills,
        SCPKills,
        DClassKills,
        ScientistKills,
        MTFKills,
        ChaosKills,
        SerpentsHandKills,
        UIUKills,
        TutorialKills,
        SurvivingPlayers,
        TotalInteractions,

        //-- Warhead
        ButtonUnlocked,
        ButtonUnlocker,
        FirstWarheadActivator,
        Detonated,
        DetonationTime,

        //-- Doors
        DoorsOpened,
        DoorsClosed,
        DoorsDestroyed,
        PlayerDoorsOpened,
        PlayerDoorsClosed,

        // Respawn Stats
        SpawnWaves,
        TotalRespawned,
        Respawns,

        // Item Stats
        TotalDrops,
        Drops,
        PlayerDrops,
        KeycardScans,
        PainkillersConsumed,
        MedkitsConsumed,
        AdrenalinesConsumed,
        SCP500sConsumed,

        //-- Firearm
        TotalShotsFired,
        TotalReloads,

        // SCP Stats
        Scp096Charges,
        Scp096Enrages,
        Scp106Teleports,
        Scp173Blinks,
        Scp173Tantrums,
        Scp939Lunges,
        Scp939Clouds,

        Scp018sThrown,
        Scp207sDrank,
        Scp268Uses,
        Scp1853Uses,

        //-- SCP-330
        First330Use,
        First330User,
        TotalCandiesTaken,
        SeveredHands,
        CandiesTaken,
        CandiesByPlayer,

        //-- SCP-914
        First914Activation,
        First914Activator,
        Total914Activations,
        TotalItemUpgrades,
        KeycardUpgrades,
        FirearmUpgrades,
        AllActivations,
        AllUpgrades,

        // Kill Stats
        KillsByPlayer,
        KillsByType,
        KillsByZone,
        PlayerKills,

        // Damage Stats
        TotalDamage,
        PlayerDamage,
        DamageByPlayer,
        DamageByType,
    }
}
