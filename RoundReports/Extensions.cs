namespace RoundReports
{
    using Exiled.API.Features;
    using PlayerRoles;

    /// <summary>
    /// Extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Whether or not this player has a valid role to record stats.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>True or false if role is ok.</returns>
        /// <remarks>This is not a replacement method for <see cref="EventHandlers.ECheck(Player)"/>, use both.</remarks>
        public static bool IsValidRole(this Player player)
            => !player.IsDead && player.Role.Type is not RoleTypeId.Filmmaker && player.Role.Type is not RoleTypeId.Overwatch;
    }
}
