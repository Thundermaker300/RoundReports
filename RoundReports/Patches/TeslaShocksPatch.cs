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
        /// <summary>
        /// Prefix patch.
        /// </summary>
        /// <param name="__instance">The <see cref="TeslaGate"/> instance.</param>
        /// <returns>Whether or not to execute the original method - always true.</returns>
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter.
        public static bool Prefix(TeslaGate __instance)
#pragma warning restore SA1313
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
