namespace RoundReports
{
    /// <summary>
    /// Determines each stat type.
    /// </summary>
    public enum StatType
    {
#pragma warning disable SA1602
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
        MostTalkativePlayer,
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

        // Misc Stats
        SpawnWaves,
        TotalRespawned,
        Respawns,

        TotalRooms,
        RoomsByZone,
        TotalCameras,
        TotalDoors,

        TotalTeslaGates,
        TeslaShocks,
        TeslaDamage,

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
        AverageShotsPerFirearm,
        ShotsByFirearm,

        // SCP Stats
        Scp049Revives,
        Scp079Tier,
        Scp079CamerasUsed,
        Scp079MostUsedCamera,
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
        AverageDamagePerPlayer,
        DamageByPlayer,
        DamageByType,

#pragma warning restore SA1602
    }
}
