namespace RoundReports
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.API.Features.Pools;
    using Exiled.API.Features.Roles;
    using Exiled.CustomRoles.API;
    using Exiled.CustomRoles.API.Features;
    using Exiled.Events;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Scp049;
    using Exiled.Events.EventArgs.Scp079;
    using Exiled.Events.EventArgs.Scp096;
    using Exiled.Events.EventArgs.Scp106;
    using Exiled.Events.EventArgs.Scp173;
    using Exiled.Events.EventArgs.Scp330;
    using Exiled.Events.EventArgs.Scp914;
    using Exiled.Events.EventArgs.Scp939;
    using Exiled.Events.EventArgs.Server;
    using Exiled.Events.EventArgs.Warhead;
    using MEC;
    using PlayerRoles;
    using PlayerRoles.PlayableScps.Scp079.Cameras;
    using PlayerRoles.PlayableScps.Scp079.Rewards;
    using PlayerRoles.Voice;
    using Scp914;
    using UnityEngine;
    using Camera = Exiled.API.Features.Camera;
    using Scp914Object = Exiled.API.Features.Scp914;

    /// <summary>
    /// Class responsible for responding to all game events.
    /// </summary>
    public class EventHandlers
    {
        /// <summary>
        /// Gets the MVP configs.
        /// </summary>
        public MVPConfigs MvpSettings => MainPlugin.Singleton.Config.MvpSettings;

        /// <summary>
        /// Gets or sets a <see cref="Dictionary{TKey, TValue}"/> of <see cref="PointTeam"/> and players' points in each.
        /// </summary>
        public Dictionary<PointTeam, Dictionary<Player, int>> Points { get; set; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether or not the first escape has occurred.
        /// </summary>
        public bool FirstEscape { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not the first SCP-914 upgrade has occurred.
        /// </summary>
        public bool FirstUpgrade { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not the first kill has occurred.
        /// </summary>
        public bool FirstKill { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not the first door has opened.
        /// </summary>
        public bool FirstDoor { get; set; } = false;

        /// <summary>
        /// Gets or sets the total amount of interactions that have occurred.
        /// </summary>
        public int Interactions { get; set; } = 0;

        /// <summary>
        /// Gets SCP-079 used cameras.
        /// </summary>
        public Dictionary<Camera, int> UsedCameras { get; } = new();

        /// <summary>
        /// Gets players that have talked.
        /// </summary>
        public Dictionary<Player, float> Talkers { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether or not final stats have been filled out.
        /// </summary>
        public bool FinalStatsFilledOut { get; set; } = false;

        private List<Player> IsSpeakingLock { get; } = new();

        /// <summary>
        /// Returns <see cref="CustomRT"/> of role. Will return SerpentsHand or UIU for respective roles.
        /// </summary>
        /// <param name="player">Player.</param>
        /// <returns>Player's role.</returns>
        public static CustomRT GetRole(Player player)
        {
            if (player is null)
                return CustomRT.None;
            if (player.SessionVariables.ContainsKey("IsSH"))
                return CustomRT.SerpentsHand;
            else if (player.SessionVariables.ContainsKey("IsUIU"))
                return CustomRT.UIU;

            ReadOnlyCollection<CustomRole> customRoles = player.GetCustomRoles();
            if (customRoles.Any(r => r.Name == "SCP-008"))
                return CustomRT.Scp008;
            else if (customRoles.Any(r => r.Name == "SCP-035"))
                return CustomRT.Scp035;

            return (CustomRT)Enum.Parse(typeof(CustomRT), player.Role.Type.ToString());
        }

        /// <summary>
        /// Returns a <see cref="PercentInt"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="total">The total.</param>
        /// <param name="updater">The updater method.</param>
        /// <returns>The <see cref="PercentInt"/>.</returns>
        public static PercentInt GetPI(int value, int total, Func<int> updater) => PercentIntPool.Pool.Get(value, total, updater);

        /// <summary>
        /// Returns <see cref="CustomTeam"/> of team. Will return SH or UIU for respective teams.
        /// </summary>
        /// <param name="player">Player.</param>
        /// <returns>Player's team.</returns>
        public static CustomTeam GetTeam(Player player)
        {
            if (player is null)
                return CustomTeam.Dead;
            if (player.SessionVariables.ContainsKey("IsSH"))
                return CustomTeam.SH;
            else if (player.SessionVariables.ContainsKey("IsUIU"))
                return CustomTeam.UIU;

            ReadOnlyCollection<CustomRole> customRoles = player.GetCustomRoles();
            if (customRoles.Any(r => r.Name == "SCP-008"))
                return CustomTeam.SCPs;
            else if (customRoles.Any(r => r.Name == "SCP-035"))
                return CustomTeam.SCPs;

            return (CustomTeam)Enum.Parse(typeof(CustomTeam), player.Role.Team.ToString());
        }

        /// <summary>
        /// Returns if stats related to a player should be recorded.
        /// </summary>
        /// <param name="ply">The player to check.</param>
        /// <returns>Boolean.</returns>
        public static bool ECheck(Player ply)
        {
            if (MainPlugin.Reporter is null)
                return false;

            if (ply is null)
                return false;

            bool flag = true;
            if (ply is not null)
            {
                if (GetRole(ply) is CustomRT.Tutorial && MainPlugin.Configs.ExcludeTutorials)
                    return false; // Exit func early (we don't want to show hidden message for tutorial exclusion)
            }

            if (ply.DoNotTrack && MainPlugin.Configs.ExcludeDNTUsers)
                flag = false;

            if (MainPlugin.Configs.IgnoredUsers.Contains(ply.UserId))
                flag = false;

            if (!flag && MainPlugin.Reporter is not null)
                MainPlugin.Reporter.AtLeastOneHidden = true;
            return flag;
        }

        /// <summary>
        /// Holds a stat, preserving its data until the end of the round. Wipes any previously stored data.
        /// </summary>
        /// <typeparam name="T">Stat to hold.</typeparam>
        /// <param name="stat">The data of the stat.</param>
        public void Hold<T>(T stat)
            where T : class, IReportStat
            => MainPlugin.Reporter?.SetStat(stat);

        /// <summary>
        /// Obtains a stat, setting it up if it doesn't already exist.
        /// </summary>
        /// <typeparam name="T">Stat to obtain.</typeparam>
        /// <returns>Stat.</returns>
        public T GetStat<T>()
            where T : class, IReportStat, new()
            => MainPlugin.Reporter?.GetStat<T>() ?? null;

        /// <summary>
        /// Increments MVP points. Ignored if user: Is null, Is DNT, Is role Tutorial, IsDead, or is an ignored user.
        /// </summary>
        /// <param name="plr">Player.</param>
        /// <param name="amount">Amount of points.</param>
        /// <param name="reason">Reason for removing.</param>
        /// <param name="teamOverride">Override the <see cref="PointTeam"/> being used.</param>
        /// <param name="overrideRoleChecks">Override dead/tutorial checks when adding points.</param>
        public void IncrementPoints(Player plr, int amount, string reason = "Unknown", PointTeam? teamOverride = null, bool overrideRoleChecks = false)
        {
            if (!MvpSettings.MvpEnabled || plr is null || plr.DoNotTrack || (overrideRoleChecks == false && (MvpSettings.RoleBlacklist.Any(role => role == GetRole(plr)) || plr.IsDead)) || MainPlugin.Configs.IgnoredUsers.Contains(plr.UserId))
                return;

            if (amount == 0) return;

            Log.Debug($"Incrementing {amount} points to {plr.Nickname}. {reason}");

            // Sorry stylecop, you could not pay me $500 to name a variable "pT".
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            PointTeam PT = teamOverride ?? (plr.IsScp ? PointTeam.SCP : PointTeam.Human);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter

            if (Points[PT].ContainsKey(plr))
                Points[PT][plr] += amount;
            else
                Points[PT][plr] = amount;

            if (Points[PT][plr] < MvpSettings.MinPoints)
                Points[PT][plr] = MvpSettings.MinPoints;
            else if (Points[PT][plr] > MvpSettings.MaxPoints)
                Points[PT][plr] = MvpSettings.MaxPoints;

            MVPStats logs = GetStat<MVPStats>();
            string str = (amount > 0 ? MainPlugin.Translations.AddPointsLog : MainPlugin.Translations.RemovePointsLog)
                .Replace("{PLAYER}", plr.Nickname)
                .Replace("{ROLE}", GetRole(plr).ToString())
                .Replace("{AMOUNT}", amount.ToString())
                .Replace("{REASON}", reason);
            logs.PointLogs.Insert(0, $"[{Reporter.GetDisplay(Round.ElapsedTime)}] {str}");

            // Show hint
            Hint h = new();
            h.CopyProperties(amount > 0 ? MvpSettings.AddHint : MvpSettings.RemoveHint);
            h.Content = h.Content
                .Replace("{POINTS}", amount.ToString())
                .Replace("{REASON}", reason);
            plr.ShowHint(h);

            Hold(logs);
        }

        /// <summary>
        /// Called when the round is waiting for connections.
        /// </summary>
        public void OnWaitingForPlayers()
        {
            FirstEscape = false;
            FirstUpgrade = false;
            FirstKill = false;
            FirstDoor = false;
            Interactions = 0;
            UsedCameras.Clear();
            Talkers.Clear();
            IsSpeakingLock.Clear();
            FinalStatsFilledOut = false;
            Points.Clear();
            Points[PointTeam.SCP] = new();
            Points[PointTeam.Human] = new();
            MainPlugin.Reporter = new Reporter();
            MainPlugin.IsRestarting = false;

            // Map Generation Stats
            var stats = GetStat<MiscStats>();
            stats.TotalCameras = Camera.List.Count();
            stats.TotalDoors = Door.List.Count();
            stats.TotalRooms = Room.List.Count();
            stats.TotalTeslaGates = TeslaGate.List.Count();

            foreach (var room in Room.List)
            {
                if (!stats.RoomsByZone.ContainsKey(room.Zone))
                    stats.RoomsByZone[room.Zone] = GetPI(1, stats.TotalRooms, () => MainPlugin.Reporter.GetStat<MiscStats>().TotalRooms);
                else
                    stats.RoomsByZone[room.Zone].IncrementValue(1);
            }

            Hold(stats);
        }

        /// <summary>
        /// Begin send/upload process.
        /// </summary>
        public void SendData()
        {
            if (MainPlugin.Reporter is null)
                return;

            // Send
            if (!MainPlugin.Reporter.HasSent)
            {
                Log.Debug("Report upload request received, step: 1.");
                MainPlugin.Reporter.SendReport();
            }
        }

        /// <summary>
        /// Called when the round starts.
        /// </summary>
        public void OnRoundStarted()
        {
            if (MainPlugin.Reporter is null)
                return;

            Timing.CallDelayed(.5f, () =>
            {
                StartingStats stats = new()
                {
                    ClassDPersonnel = Player.Get(RoleTypeId.ClassD).Count(player => ECheck(player)),
                    SCPs = Player.Get(player => GetTeam(player) is CustomTeam.SCPs && ECheck(player)).Select((ply) => GetRole(ply).ToString()).ToList(),
                    FacilityGuards = Player.Get(RoleTypeId.FacilityGuard).Count(player => ECheck(player)),
                    Scientists = Player.Get(RoleTypeId.Scientist).Count(player => ECheck(player)),
                    StartTime = DateTime.Now,
                    PlayersAtStart = Player.List.Where(r => !r.IsDead).Count(player => ECheck(player)),
                    Players = ListPool<string>.Pool.Get(),
                };
                foreach (Player player in Player.List.Where(player => ECheck(player) && !player.IsDead))
                    stats.Players.Add($"{Reporter.GetDisplay(player, typeof(Player))} [{GetRole(player)}]");
                Hold(stats);
                Timing.RunCoroutine(RecordSCPsStats().CancelWith(Server.Host.GameObject));
            });
        }

        /// <summary>
        /// Called when the round restarts.
        /// </summary>
        public void OnRestarting()
        {
            MainPlugin.IsRestarting = true;
            if (MainPlugin.Reporter is not null && !MainPlugin.Reporter.HasSent && !FinalStatsFilledOut)
            {
                FinalStatsFilledOut = true;
                FillOutFinalStats();
                SendData();
            }
        }

        /// <summary>
        /// Called when the round ends.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            if (MainPlugin.Reporter is not null && !MainPlugin.Reporter.HasSent && !FinalStatsFilledOut)
            {
                FinalStatsFilledOut = true;
                FillOutFinalStats(ev.LeadingTeam);
                SendData();
            }
        }

        /// <summary>
        /// Called when a team respawns.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnRespawningTeam(RespawningTeamEventArgs ev)
        {
            if (!Round.InProgress || MainPlugin.IsRestarting || MainPlugin.Reporter is null) return;
            if (!ev.IsAllowed || ev.Players.Count < 1) return;
            MiscStats stats = GetStat<MiscStats>();

            if (ev.NextKnownTeam is Respawning.SpawnableTeamType.NineTailedFox)
            {
                if (IsUIUTeamSpawnable())
                    stats.SpawnWaves.Add("UIU");
                else if (ev.IsAllowed)
                    stats.SpawnWaves.Add("Nine Tailed Fox");
            }
            else if (ev.NextKnownTeam is Respawning.SpawnableTeamType.ChaosInsurgency)
            {
                if (IsSerpentsHandTeamSpawnable())
                    stats.SpawnWaves.Add("Serpent's Hand");
                else if (ev.IsAllowed)
                    stats.SpawnWaves.Add("Chaos Insurgency");
            }

            Hold(stats);
        }

        /// <summary>
        /// Called when a player leaves.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnLeft(LeftEventArgs ev)
        {
            if (!Reporter.NameStore.ContainsKey(ev.Player))
            {
                if (!ev.Player.DoNotTrack && !MainPlugin.Configs.IgnoredUsers.Contains(ev.Player.UserId))
                    Reporter.NameStore.Add(ev.Player, ev.Player.Nickname);
                else
                    Reporter.NameStore.Add(ev.Player, Reporter.DoNotTrackText);
            }
        }

        /// <summary>
        /// Called when a player spawns.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnSpawned(SpawnedEventArgs ev)
        {
            if (!Round.InProgress || MainPlugin.IsRestarting || Round.ElapsedTime.TotalSeconds <= 30 || !ECheck(ev.Player)) return;
            if (GetTeam(ev.Player) is CustomTeam.FoundationForces or CustomTeam.ChaosInsurgency or CustomTeam.SH or CustomTeam.UIU)
            {
                MiscStats stats = GetStat<MiscStats>();
                stats.TotalRespawned++;
                stats.Respawns.Insert(0, $"[{Reporter.GetDisplay(Round.ElapsedTime)}] " + MainPlugin.Translations.RespawnLog.Replace("{PLAYER}", Reporter.GetDisplay(ev.Player, typeof(Player))).Replace("{ROLE}", GetRole(ev.Player).ToString()));
                Hold(stats);
            }
        }

        /// <summary>
        /// Called when a player takes damage.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnHurting(HurtingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || MainPlugin.IsRestarting || MainPlugin.Reporter is null) return;
            int amount = (int)Math.Round(ev.Amount);
            if (ev.Amount == -1 || ev.Amount > 150) amount = 150;

            OrganizedDamageStats stats = GetStat<OrganizedDamageStats>();

            if (ECheck(ev.Player))
            {
                // Stats
                stats.TotalDamage += amount;

                // Check damage type
                if (!stats.DamageByType.ContainsKey(ev.DamageHandler.Type))
                    stats.DamageByType.Add(ev.DamageHandler.Type, GetPI(amount, stats.TotalDamage, () => MainPlugin.Reporter.GetStat<OrganizedDamageStats>().TotalDamage));
                else
                    stats.DamageByType[ev.DamageHandler.Type].IncrementValue(amount);
            }

            // Check Attacker
            if (ECheck(ev.Attacker))
            {
                stats.PlayerDamage += amount;
                if (!stats.DamageByPlayer.ContainsKey(ev.Attacker))
                    stats.DamageByPlayer.Add(ev.Attacker, GetPI(amount, stats.TotalDamage, () => MainPlugin.Reporter.GetStat<OrganizedDamageStats>().TotalDamage));
                else
                    stats.DamageByPlayer[ev.Attacker].IncrementValue(amount);
            }

            Hold(stats);
        }

        /// <summary>
        /// Called when a player dies (rip).
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnDying(DyingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || MainPlugin.IsRestarting || MainPlugin.Reporter is null) return;
            FinalStats stats = GetStat<FinalStats>();
            OrganizedKillsStats killStats = GetStat<OrganizedKillsStats>();
            stats.TotalDeaths++;
            if (ev.Attacker is not null)
            {
                // Kill logs
                string killerRole = GetRole(ev.Attacker).ToString();
                string dyingRole = GetRole(ev.Player).ToString();
                killStats.PlayerKills.Insert(0, $"[{Reporter.GetDisplay(Round.ElapsedTime)}] {Reporter.GetDisplay(ev.Attacker, typeof(Player))} [{killerRole}] killed {Reporter.GetDisplay(ev.Player, typeof(Player))} [{dyingRole}]");

                // Kill by player
                if (!killStats.KillsByPlayer.ContainsKey(ev.Attacker))
                    killStats.KillsByPlayer.Add(ev.Attacker, GetPI(1, stats.TotalKills, () => MainPlugin.Reporter.GetStat<FinalStats>().TotalKills));
                else
                    killStats.KillsByPlayer[ev.Attacker].IncrementValue(1);

                // Kill by zone
                if (killStats.KillsByZone.ContainsKey(ev.Player.Zone))
                    killStats.KillsByZone[ev.Player.Zone].IncrementValue(1);
                else
                    killStats.KillsByZone.Add(ev.Player.Zone, GetPI(1, stats.TotalKills, () => MainPlugin.Reporter.GetStat<FinalStats>().TotalKills));

                // Role kills
                stats.TotalKills++;
                if (GetTeam(ev.Attacker) is CustomTeam.UIU)
                {
                    stats.UIUKills++;
                }
                else if (GetTeam(ev.Attacker) is CustomTeam.SH)
                {
                    stats.SerpentsHandKills++;
                }
                else
                {
                    switch (ev.Attacker.Role.Team)
                    {
                        case Team.SCPs:
                            stats.SCPKills++;
                            break;
                        case Team.ClassD:
                            stats.DClassKills++;
                            break;
                        case Team.Scientists:
                            stats.ScientistKills++;
                            break;
                        case Team.FoundationForces:
                            stats.MTFKills++;
                            break;
                        case Team.ChaosInsurgency:
                            stats.ChaosKills++;
                            break;
                        case Team.OtherAlive:
                            stats.TutorialKills++;
                            break;
                    }
                }

                // First kill check
                if (!FirstKill && MainPlugin.Reporter is not null && ECheck(ev.Attacker))
                {
                    string killText = MainPlugin.Translations.KillRemark
                        .Replace("{PLAYER}", Reporter.GetDisplay(ev.Attacker, typeof(Player)))
                        .Replace("{ROLE}", GetRole(ev.Attacker).ToString())
                        .Replace("{TARGET}", Reporter.GetDisplay(ev.Player, typeof(Player)))
                        .Replace("{TARGETROLE}", dyingRole);
                    MainPlugin.Reporter.AddRemark(killText);
                    FirstKill = true;
                }

                // Killer points
                if (ev.Player.Role.Side == ev.Attacker.Role.Side && ev.DamageHandler.Type != DamageType.Scp018)
                    IncrementPoints(ev.Attacker, MvpSettings.Points.KillTeammate, MainPlugin.Translations.KilledTeammate); // Kill teammate
                else if (GetTeam(ev.Player) is CustomTeam.SCPs && GetRole(ev.Player) != CustomRT.Scp0492)
                    IncrementPoints(ev.Attacker, MvpSettings.Points.KillScp, MainPlugin.Translations.KilledSCP); // Kill SCP
                else if (GetTeam(ev.Player) is CustomTeam.Scientists)
                    IncrementPoints(ev.Attacker, MvpSettings.Points.KillScientist, MainPlugin.Translations.KilledScientist); // Kill scientist
                else
                    IncrementPoints(ev.Attacker, MvpSettings.Points.KillEnemy, MainPlugin.Translations.KilledEnemy); // Other kills

                // Grant points to SCP-079 if death in a locked down/blackout room
                if (GetTeam(ev.Attacker) is CustomTeam.SCPs || ev.DamageHandler.Type == DamageType.CardiacArrest)
                {
                    foreach (Player player in Player.Get(ECheck))
                    {
                        if (player.Role is Scp079Role role && role.SubroutineModule.TryGetSubroutine(out Scp079RewardManager manager) && manager._markedRooms.ContainsKey(ev.Player.CurrentRoom.Identifier))
                            IncrementPoints(player, MvpSettings.Points.Scp079AssistKill, MainPlugin.Translations.AssistKill);
                    }
                }
            }

            // Edge case for SCP-049 kills
            else
            {
                if (ev.DamageHandler.Type == DamageType.CardiacArrest)
                {
                    IEnumerable<Player> doctors = Player.Get(ply => GetRole(ply) is CustomRT.Scp049).Where(bird => Vector3.Distance(bird.Position, ev.Player.Position) < 12);
                    foreach (Player birds in doctors)
                    {
                        IncrementPoints(birds, MvpSettings.Points.KillEnemy, MainPlugin.Translations.KilledEnemy);
                    }
                }
            }

            // Kill by type
            if (!killStats.KillsByType.ContainsKey(ev.DamageHandler.Type))
                killStats.KillsByType.Add(ev.DamageHandler.Type, 1);
            else
                killStats.KillsByType[ev.DamageHandler.Type]++;
            Hold(killStats);
            Hold(stats);

            // Target Points
            if (ev.DamageHandler.Type is DamageType.Warhead or DamageType.Decontamination or DamageType.Tesla or DamageType.Crushed or DamageType.Falldown)
                IncrementPoints(ev.Player, ev.Player.IsScp && ev.Player.Role != RoleTypeId.Scp0492 ? MvpSettings.Points.ScpDiedDumb : MvpSettings.Points.DiedDumb, MainPlugin.Translations.Death); // Dumb causes
            else
                IncrementPoints(ev.Player, ev.Player.IsScp && ev.Player.Role != RoleTypeId.Scp0492 ? MvpSettings.Points.ScpDied : MvpSettings.Points.Died, MainPlugin.Translations.Death); // Other causes
        }

        /// <summary>
        /// Called when an item is dropped.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player))
                return;
            ItemStats stats = GetStat<ItemStats>();
            stats.TotalDrops++;

            if (stats.Drops.ContainsKey(ev.Item.Type))
                stats.Drops[ev.Item.Type].IncrementValue(1);
            else
                stats.Drops[ev.Item.Type] = GetPI(1, stats.TotalDrops, () => MainPlugin.Reporter.GetStat<ItemStats>().TotalDrops);

            if (stats.PlayerDrops.ContainsKey(ev.Player))
                stats.PlayerDrops[ev.Player].IncrementValue(1);
            else
                stats.PlayerDrops[ev.Player] = GetPI(1, stats.TotalDrops, () => MainPlugin.Reporter.GetStat<ItemStats>().TotalDrops);
            Hold(stats);
        }

        /// <summary>
        /// Called when a player enters the pocket dimension.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnEnteringPocketDimension(EnteringPocketDimensionEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || ev.Player is null || ev.Scp106 is null || !ECheck(ev.Scp106)) return;
            IncrementPoints(ev.Scp106, MvpSettings.Points.Scp106GrabPlayer, MainPlugin.Translations.GrabbedPlayer);

            foreach (Player player in Player.Get(ECheck))
            {
                if (player.Role is Scp079Role role && role.SubroutineModule.TryGetSubroutine(out Scp079RewardManager manager) && manager._markedRooms.ContainsKey(ev.Player.CurrentRoom.Identifier))
                    IncrementPoints(player, MvpSettings.Points.Scp079AssistKill, MainPlugin.Translations.AssistKill);
            }
        }

        /// <summary>
        /// Called when a tesla gate is triggered.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnTriggeringTesla(TriggeringTeslaEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            var stats = GetStat<MiscStats>();

            stats.TeslaShocks++;

            Hold(stats);
        }

        /*
         * The following three methods are pure SHIT.
         * This is the only way I can think of doing it. I am sorry for what you are about to read.
         * If you have a better way to do this, please let me know :)
         * VoiceChatting event: Called every frame a player is speaking.
         * I could technically add to the Talkers dictionary every frame,
         * although I'm not sure if that's the best option either.
        */

        /// <summary>
        /// Called when a player talks.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnVoiceChatting(VoiceChattingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player) || ev.Player.Role.Type is RoleTypeId.Overwatch) return;

            if (!Talkers.ContainsKey(ev.Player))
                Talkers.Add(ev.Player, Time.deltaTime);
            else
                Talkers[ev.Player] += Time.deltaTime;
        }

        /// <summary>
        /// Called when a shot is fired.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnShooting(ShootingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;

            ItemStats stats = GetStat<ItemStats>();
            stats.TotalShotsFired++;

            Firearm firearm = (Firearm)ev.Player.CurrentItem;
            if (firearm is not null)
            {
                if (!stats.ShotsByFirearm.ContainsKey(firearm.FirearmType))
                {
                    stats.ShotsByFirearm[firearm.FirearmType] = GetPI(1, stats.TotalShotsFired, () => MainPlugin.Reporter.GetStat<ItemStats>().TotalShotsFired);
                }
                else
                {
                    stats.ShotsByFirearm[firearm.FirearmType].IncrementValue(1);
                }
            }

            Hold(stats);
        }

        /// <summary>
        /// Called when a weapon is reloaded.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnReloadingWeapon(ReloadingWeaponEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            ItemStats stats = GetStat<ItemStats>();
            stats.TotalReloads++;
            Hold(stats);
        }

        /// <summary>
        /// Called when SCP-079 gains a level.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnScp079GainingLevel(GainingLevelEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            IncrementPoints(ev.Player, MvpSettings.Points.Scp079LeveledUp, MainPlugin.Translations.LeveledUp);
        }

        /// <summary>
        /// Called when SCP-079 interacts with a tesla.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnScp079InteractTesla(InteractingTeslaEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            foreach (Player player in Player.List)
            {
                if (ev.Tesla.IsPlayerInHurtRange(player))
                {
                    IncrementPoints(ev.Player, MvpSettings.Points.Scp079TeslaKill, MainPlugin.Translations.TeslaKill);
                    break;
                }
            }

            Interactions++;
        }

        /// <summary>
        /// Called when SCP-079 changes cameras.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnScp079ChangingCamera(ChangingCameraEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            if (UsedCameras.ContainsKey(ev.Camera))
            {
                UsedCameras[ev.Camera]++;
                return;
            }

            var stats = GetStat<SCPStats>();
            if (stats.Scp079CamerasUsed is null)
                stats.Scp079CamerasUsed = GetPI(1, Camera.List.Count(), () => Camera.List.Count());
            else
                stats.Scp079CamerasUsed.IncrementValue(1);

            UsedCameras.Add(ev.Camera, 1);

            Hold(stats);
        }

        /// <summary>
        /// Called when a player damages a window.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnDamagingWindow(DamagingWindowEventArgs ev)
        {
            if (!Round.InProgress || !ECheck(ev.Player)) return;
            if (ev.Window.Type is GlassType.Scp079Trigger && Player.Get(RoleTypeId.Scp079).Count() > 0)
            {
                IncrementPoints(ev.Player, MvpSettings.Points.RecontainScp079, MainPlugin.Translations.RecontainScp079);
            }
        }

        /// <summary>
        /// Called when SCP-049 recalls.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnScp049Recalling(FinishingRecallEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;

            SCPStats stat = GetStat<SCPStats>();
            stat.Scp049Revives++;
            Hold(stat);
        }

        /// <summary>
        /// Called when SCP-079 interacts with a door.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnScp079TriggeringDoor(TriggeringDoorEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || ev.Door.IsOpen || !ECheck(ev.Player)) return;
            if (Player.List.Count(plr => plr != ev.Player && GetTeam(plr) is CustomTeam.SCPs && Vector3.Distance(ev.Door.Transform.position, plr.GameObject.transform.position) <= 20) == 0)
                return;

            int pts = 0;
            if (ev.Door.IsKeycardDoor)
            {
                if (ev.Door.IsGate) pts = MvpSettings.Points.Scp079OpenGate;
                else pts = MvpSettings.Points.Scp079OpenKeycardDoor;
            }

            IncrementPoints(ev.Player, pts, MainPlugin.Translations.OpenedDoor);
            Interactions++;
        }

        /// <summary>
        /// Called when SCP-096 charges.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnScp096Charge(ChargingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            stats.Scp096Charges++;
            Hold(stats);
        }

        /// <summary>
        /// Called when SCP-096 enrages.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnScp096Enrage(EnragingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            stats.Scp096Enrages++;
            Hold(stats);
        }

        /// <summary>
        /// Called when SCP-106 teleports.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnScp106Teleport(TeleportingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            stats.Scp106Teleports++;
            Hold(stats);
        }

        /// <summary>
        /// Called when SCP-173 blinks.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnScp173Blink(BlinkingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            stats.Scp173Blinks++;
            Hold(stats);
        }

        /// <summary>
        /// Called when SCP-173 places tantrum.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnScp173Tantrum(PlacingTantrumEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            stats.Scp173Tantrums++;
            Hold(stats);
        }

        /// <summary>
        /// Called when SCP-939 lunges.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnScp939Lunge(LungingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            stats.Scp939Lunges++;
            Hold(stats);
        }

        /// <summary>
        /// Called when SCP-939 places an amnestic cloud.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnScp939Cloud(PlacingAmnesticCloudEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            stats.Scp939Clouds++;
            Hold(stats);
        }

        /// <summary>
        /// Called when an item is used.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnUsedItem(UsedItemEventArgs ev)
        {
            if (!Round.InProgress || !ECheck(ev.Player)) return;
            if (ev.Item.Type.IsScp())
            {
                SCPStats stats = GetStat<SCPStats>();
                switch (ev.Item.Type)
                {
                    case ItemType.SCP018:
                        stats.Scp018Thrown++;
                        break;
                    case ItemType.SCP207:
                        stats.Scp207Drank++;
                        break;
                    case ItemType.SCP268:
                        stats.Scp268Uses++;
                        break;
                    case ItemType.SCP1853:
                        stats.Scp1853Uses++;
                        break;
                }

                Hold(stats);
            }
            else
            {
                ItemStats stats = GetStat<ItemStats>();
                switch (ev.Item.Type)
                {
                    case ItemType.Painkillers:
                        stats.PainkillersConsumed++;
                        break;
                    case ItemType.Medkit:
                        stats.MedkitsConsumed++;
                        break;
                    case ItemType.Adrenaline:
                        stats.AdrenalinesConsumed++;
                        break;
                    case ItemType.SCP500:
                        stats.SCP500sConsumed++;
                        break;
                }

                Hold(stats);
            }
        }

        /// <summary>
        /// Called when a player interacts with a door.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            if (ev.Player is null) return;

            FinalStats stats = GetStat<FinalStats>();
            if (ev.Door.IsOpen)
            {
                stats.DoorsClosed++;
                if (stats.PlayerDoorsClosed.ContainsKey(ev.Player))
                    stats.PlayerDoorsClosed[ev.Player].IncrementValue(1);
                else
                    stats.PlayerDoorsClosed.Add(ev.Player, GetPI(1, stats.DoorsClosed, () => MainPlugin.Reporter.GetStat<FinalStats>().DoorsClosed));
            }
            else
            {
                stats.DoorsOpened++;
                if (stats.PlayerDoorsOpened.ContainsKey(ev.Player))
                    stats.PlayerDoorsOpened[ev.Player].IncrementValue(1);
                else
                    stats.PlayerDoorsOpened.Add(ev.Player, GetPI(1, stats.DoorsOpened, () => MainPlugin.Reporter.GetStat<FinalStats>().DoorsOpened));

                if (!FirstDoor && ev.Player is not null && MainPlugin.Reporter is not null)
                {
                    FirstDoor = true;
                    string remarkText = MainPlugin.Translations.DoorRemark
                        .Replace("{PLAYER}", Reporter.GetDisplay(ev.Player, typeof(Player)))
                        .Replace("{ROLE}", GetRole(ev.Player).ToString())
                        .Replace("{MILLISECOND}", Round.ElapsedTime.Milliseconds.ToString());
                    MainPlugin.Reporter.AddRemark(remarkText);
                }
            }

            if (ev.Door.IsKeycardDoor)
            {
                ItemStats itemStats = GetStat<ItemStats>();
                itemStats.KeycardScans++;
                Hold(itemStats);
            }

            Hold(stats);
            Interactions++;
        }

        /// <summary>
        /// Called when a player interacts with an elevator.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            Interactions++;
        }

        /// <summary>
        /// Called when a player interacts with a locker.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnInteractingLocker(InteractingLockerEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            Interactions++;
        }

        /// <summary>
        /// Called when a player interacts with SCP-330.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnInteractingScp330(InteractingScp330EventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            if (stats.FirstUse == DateTime.MinValue)
            {
                stats.FirstUse = DateTime.Now;
                stats.FirstUser = ev.Player;
            }

            stats.TotalCandiesTaken++;
            if (!stats.CandiesByPlayer.ContainsKey(ev.Player))
                stats.CandiesByPlayer.Add(ev.Player, 1);
            else
                stats.CandiesByPlayer[ev.Player]++;

            // Hands
            if (ev.ShouldSever)
            {
                stats.SeveredHands++;
                IncrementPoints(ev.Player, MvpSettings.Points.Took3Candies, MainPlugin.Translations.Took3Candies);
            }

            // Candies Taken
            if (!stats.CandiesTaken.ContainsKey(ev.Candy))
                stats.CandiesTaken.Add(ev.Candy, 1);
            else
                stats.CandiesTaken[ev.Candy]++;
            Hold(stats);
            Interactions++;
        }

        /// <summary>
        /// Called when a player escapes.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnEscaping(EscapingEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress || !ECheck(ev.Player)) return;
            if (!FirstEscape && MainPlugin.Reporter is not null)
            {
                FirstEscape = true;
                string escapeText = MainPlugin.Translations.EscapeRemark
                    .Replace("{PLAYER}", Reporter.GetDisplay(ev.Player, typeof(Player)))
                    .Replace("{ROLE}", GetRole(ev.Player).ToString())
                    .Replace("{MINUTE}", Round.ElapsedTime.Minutes.ToString())
                    .Replace("{SECOND}", Round.ElapsedTime.Seconds.ToString());
                MainPlugin.Reporter.AddRemark(escapeText);
            }

            IncrementPoints(ev.Player, MvpSettings.Points.Escaped, MainPlugin.Translations.Escaped);
        }

        /// <summary>
        /// Called when a player activates SCP-914.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnActivatingScp914(ActivatingEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            if (stats.FirstActivation == DateTime.MinValue && ev.Player is not null)
            {
                stats.FirstActivation = DateTime.Now;
                stats.FirstActivator = ev.Player;
            }

            stats.TotalActivations++;
            if (!stats.Activations.ContainsKey(Scp914Object.KnobStatus))
                stats.Activations.Add(Scp914Object.KnobStatus, GetPI(1, stats.TotalActivations, () => MainPlugin.Reporter.GetStat<SCPStats>().TotalActivations));
            else
                stats.Activations[Scp914Object.KnobStatus].IncrementValue(1);

            Hold(stats);
            Interactions++;
        }

        /// <summary>
        /// Called when a player modified SCP-914's knob setting.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnChangingKnobSetting(ChangingKnobSettingEventArgs ev)
        {
            Interactions++;
        }

        /// <summary>
        /// Called when a <see cref="Exiled.API.Features.Pickups.Pickup"/> is upgraded in SCP-914.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void On914UpgradingPickup(UpgradingPickupEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress || MainPlugin.Reporter is null) return;
            UpgradeItemLog(ev.Pickup.Type, ev.KnobSetting);
        }

        /// <summary>
        /// Called when an inventory item is upgraded in SCP-914.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void On914UpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress || !ECheck(ev.Player)) return;
            UpgradeItemLog(ev.Item.Type, ev.KnobSetting);
        }

        /// <summary>
        /// Called when a generator is unlocked.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnUnlockingGenerator(UnlockingGeneratorEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress || !ECheck(ev.Player)) return;
            IncrementPoints(ev.Player, MvpSettings.Points.UnlockGenerator, MainPlugin.Translations.UnlockedGenerator);
        }

        /// <summary>
        /// Called when the warhead panel button is unlocked.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress || MainPlugin.Reporter is null) return;
            FinalStats stats = GetStat<FinalStats>();
            stats.ButtonUnlocked = true;
            if (stats.ButtonUnlocker == null)
            {
                stats.ButtonUnlocker = ev.Player;
                IncrementPoints(ev.Player, MvpSettings.Points.OpenWarheadPanel, MainPlugin.Translations.OpenedWarheadPanel);
            }

            Hold(stats);
            Interactions++;
        }

        /// <summary>
        /// Called when the warhead starts.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnWarheadStarting(StartingEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress || MainPlugin.Reporter is null) return;
            FinalStats stats = GetStat<FinalStats>();
            stats.FirstActivator ??= ev.Player;
            Hold(stats);
            Interactions++;
        }

        /// <summary>
        /// Called when the warhead is detonated.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnWarheadDetonated()
        {
            if (!Round.InProgress || MainPlugin.Reporter is null) return;
            FinalStats stats = GetStat<FinalStats>();
            if (!stats.Detonated)
            {
                stats.Detonated = true;
                stats.DetonationTime = DateTime.Now;
            }

            Hold(stats);
        }

        /// <summary>
        /// This is to account for plugins such as SCPSwap, as well as SCP deaths early on, etc.
        /// </summary>
        /// <returns>Coroutine.</returns>
        private IEnumerator<float> RecordSCPsStats()
        {
            yield return Timing.WaitForSeconds(60f);
            StartingStats stats = GetStat<StartingStats>();
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            List<string> SCPs = Player.Get(player => GetTeam(player) is CustomTeam.SCPs && ECheck(player)).Select(ply => GetRole(ply).ToString()).ToList();
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            SCPs.Sort();
            stats.SCPs = SCPs;
            Hold(stats);
        }

        /// <summary>
        /// Fills out <see cref="FinalStats"/>.
        /// </summary>
        /// <param name="leadingTeam">The winning team of the round.</param>
        private void FillOutFinalStats(LeadingTeam leadingTeam = LeadingTeam.Draw)
        {
            if (MainPlugin.Reporter is null)
                return;

            MainPlugin.Reporter.AddMissingStats();

            foreach (IReportStat stat in MainPlugin.Reporter.Stats)
            {
                if (stat is null) continue;

                try
                {
                    stat.FillOutFinal();
                }
                catch (Exception ex)
                {
                    Log.Error($"Error when filling out {stat.Title} stats: {ex}");
                }
            }

            var stats = GetStat<FinalStats>();

            // Fill out final stats
            stats.WinningTeam = leadingTeam switch
            {
                LeadingTeam.Anomalies => MainPlugin.Translations.ScpTeam,
                LeadingTeam.ChaosInsurgency => MainPlugin.Translations.InsurgencyTeam,
                LeadingTeam.FacilityForces => MainPlugin.Translations.MtfTeam,
                LeadingTeam.Draw => MainPlugin.Translations.Stalemate,
                _ => MainPlugin.Translations.Unknown
            };

            // Set Leading Team
            MainPlugin.Reporter.WinTeam = leadingTeam;

            Hold(stats);

            // Compile MVP info
            if (MvpSettings.MvpEnabled)
            {
                var sortedHumanData = Points[PointTeam.Human].OrderByDescending(data => data.Value);
                var sortedSCPData = Points[PointTeam.SCP].OrderByDescending(data => data.Value);
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                MVPStats MVPInfo = GetStat<MVPStats>();
#pragma warning restore SA1312 // Variable names should begin with lower-case letter

                if (sortedHumanData.Count(data => data.Value > 0) >= 1)
                {
                    KeyValuePair<Player, int>? mvp = sortedHumanData.FirstOrDefault(data => data.Value > 0);
                    if (mvp.HasValue)
                        MVPInfo.HumanMVP = mvp.Value.Key.Nickname + $" ({mvp.Value.Value} points)";
                }

                if (sortedSCPData.Count(data => data.Value > 0) >= 1)
                {
                    KeyValuePair<Player, int>? mvp = sortedSCPData.FirstOrDefault(data => data.Value > 0);
                    if (mvp.HasValue)
                        MVPInfo.SCPMVP = mvp.Value.Key.Nickname + $" ({mvp.Value.Value} points)";
                }

                MVPInfo.HumanPoints = sortedHumanData.ToDictionary(kp => kp.Key, kp2 => kp2.Value);
                MVPInfo.SCPPoints = sortedSCPData.ToDictionary(kp => kp.Key, kp2 => kp2.Value);
                Hold(MVPInfo);
            }
        }

        private void UpgradeItemLog(ItemType type, Scp914KnobSetting mode)
        {
            SCPStats stats = GetStat<SCPStats>();
            stats.TotalItemUpgrades++;
            if (!FirstUpgrade && MainPlugin.Reporter is not null)
            {
                FirstUpgrade = true;
                string upgradeText = MainPlugin.Translations.UpgradeRemark
                    .Replace("{ITEM}", type.ToString())
                    .Replace("{MODE}", mode.ToString());
                MainPlugin.Reporter.AddRemark(upgradeText);
            }

            if (type.IsKeycard())
                stats.KeycardUpgrades++;
            else if (type.IsWeapon(false))
                stats.FirearmUpgrades++;

            if (!stats.Upgrades.ContainsKey(type))
                stats.Upgrades.Add(type, GetPI(1, stats.TotalItemUpgrades, () => MainPlugin.Reporter.GetStat<SCPStats>().TotalItemUpgrades));
            else
                stats.Upgrades[type].IncrementValue(1);

            Hold(stats);
        }

        /// <summary>
        /// Returns if SerpentsHand is ready to spawn.
        /// </summary>
        /// <returns>SH spawnable.</returns>
        // Credit to RespawnTimer for this method
        private bool IsSerpentsHandTeamSpawnable()
        {
            if (MainPlugin.SerpentsHandAssembly == null)
                return false;

            Type type = MainPlugin.SerpentsHandAssembly.GetType("SerpentsHand");
            object singleton = type.GetField("Singleton").GetValue(null);
            return (bool)type.GetField("IsSpawnable").GetValue(singleton);
        }

        /// <summary>
        /// Returns if UIU is ready to spawn.
        /// </summary>
        /// <returns>UIU spawnable.</returns>
        // and this method
        private bool IsUIUTeamSpawnable()
        {
            if (MainPlugin.UIURescueSquadAssembly == null)
                return false;

            return (bool)MainPlugin.UIURescueSquadAssembly.GetType("UIURescueSquad.EventHandlers")?.GetField("IsSpawnable").GetValue(null);
        }
    }
}
