namespace RoundReports.Patches
{
    using Exiled.API.Features;
    using HarmonyLib;

    using TeslaGate = TeslaGate;

    /// <summary>
    /// Patch tesla gates to fix Tesla Shocks stat.
    /// </summary>
    [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.ServerSideCode))]
    public static class TeslaShocksPatch
    {
        public static bool Prefix(TeslaGate __instance)
        {
            if (!__instance.InProgress && Round.InProgress)
            {
                var stats = MainPlugin.Handlers.GetStat<MiscStats>();

                stats.TeslaShocks++;

                MainPlugin.Handlers.Hold(stats);
            }

            return true;
        }
    }
}
