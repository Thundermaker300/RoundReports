namespace RoundReports
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.API.Features.Pools;
    using Exiled.API.Features.Roles;
    using Exiled.Events.EventArgs.Player;
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
    using PlayerRoles.PlayableScps.Scp079.Rewards;
    using Scp914;
    using UnityEngine;
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
        /// Gets or sets stats currently being held.
        /// </summary>
        public List<IReportStat> Holding { get; set; }

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
        /// Returns string of role. Will return SerpentsHand or UIU for respective roles.
        /// </summary>
        /// <param name="player">Player.</param>
        /// <returns>Player's role.</returns>
        public static string GetRole(Player player)
        {
            if (player is null)
                return RoleTypeId.None.ToString();
            if (player.SessionVariables.ContainsKey("IsSH"))
                return "SerpentsHand";
            else if (player.SessionVariables.ContainsKey("IsUIU"))
                return "UIU";

            return player.Role.Type.ToString();
        }

        /// <summary>
        /// Returns string of team. Will return SH or UIU for respective teams.
        /// </summary>
        /// <param name="player">Player.</param>
        /// <returns>Player's team.</returns>
        public static string GetTeam(Player player)
        {
            if (player is null)
                return Team.Dead.ToString();
            if (player.SessionVariables.ContainsKey("IsSH"))
                return "SH";
            else if (player.SessionVariables.ContainsKey("IsUIU"))
                return "UIU";

            return player.Role.Team.ToString();
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
                if (GetRole(ply) == "Tutorial" && MainPlugin.Configs.ExcludeTutorials)
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
        {
            Log.Debug($"Updating stat: {typeof(T).Name}");
            Holding.RemoveAll(r => r.GetType() == typeof(T));
            Holding.Add(stat);
        }

        /// <summary>
        /// Obtains a stat, setting it up if it doesn't already exist.
        /// </summary>
        /// <typeparam name="T">Stat to obtain.</typeparam>
        /// <returns>Stat.</returns>
        public T GetStat<T>()
            where T : class, IReportStat, new()
        {
            if (Holding.FirstOrDefault(r => r.GetType() == typeof(T)) is not T value)
            {
                value = new T();
                value.Setup();
            }

            return value;
        }

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
                Points[PT][plr] = 0 + amount;

            if (Points[PT][plr] < MvpSettings.MinPoints)
                Points[PT][plr] = MvpSettings.MinPoints;
            else if (Points[PT][plr] > MvpSettings.MaxPoints)
                Points[PT][plr] = MvpSettings.MaxPoints;

            MVPStats logs = GetStat<MVPStats>();
            string str = (amount > 0 ? MainPlugin.Translations.AddPointsLog : MainPlugin.Translations.RemovePointsLog)
                .Replace("{PLAYER}", plr.Nickname)
                .Replace("{ROLE}", GetRole(plr))
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
            Holding = ListPool<IReportStat>.Pool.Get();
            Points.Clear();
            Points[PointTeam.SCP] = new();
            Points[PointTeam.Human] = new();
            MainPlugin.Reporter = new Reporter();
            MainPlugin.IsRestarting = false;
        }

        /// <summary>
        /// Begin send/upload process.
        /// </summary>
        public void SendData()
        {
            if (MainPlugin.Reporter is null)
                return;

            // Set Stats
            foreach (IReportStat stat in Holding)
                MainPlugin.Reporter.SetStat(stat);

            // Send
            if (!MainPlugin.Reporter.HasSent)
            {
                Log.Debug("Report upload request received, step: 1.");
                MainPlugin.Reporter.SendReport();
            }

            ListPool<IReportStat>.Pool.Return(Holding);
        }

        /// <summary>
        /// Called when the round starts.
        /// </summary>
        public void OnRoundStarted()
        {
            Timing.CallDelayed(.5f, () =>
            {
                StartingStats stats = new()
                {
                    ClassDPersonnel = Player.Get(RoleTypeId.ClassD).Count(player => ECheck(player)),
                    SCPs = Player.Get(Team.SCPs).Where(player => ECheck(player)).Select(player => player.Role.Type).ToList(),
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
            if (MainPlugin.Reporter is not null && !MainPlugin.Reporter.HasSent)
            {
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
            if (MainPlugin.Reporter is not null && !MainPlugin.Reporter.HasSent)
            {
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
            if (!Round.InProgress || MainPlugin.IsRestarting) return;
            if (!ev.IsAllowed || ev.Players.Count < 1) return;
            RespawnStats stats = GetStat<RespawnStats>();

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
            if (!Round.InProgress || MainPlugin.IsRestarting || Round.ElapsedTime.TotalMinutes <= 0.5 || !ECheck(ev.Player) || GetTeam(ev.Player) is not("SH" or "UIU" or "FoundationForces" or "ChaosInsurgency")) return;
            RespawnStats stats = GetStat<RespawnStats>();
            stats.TotalRespawned++;
            stats.Respawns.Insert(0, $"[{Reporter.GetDisplay(Round.ElapsedTime)}] " + MainPlugin.Translations.RespawnLog.Replace("{PLAYER}", Reporter.GetDisplay(ev.Player, typeof(Player))).Replace("{ROLE}", GetRole(ev.Player)));
            Hold(stats);
        }

        /// <summary>
        /// Called when a player takes damage.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnHurting(HurtingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || MainPlugin.IsRestarting) return;
            int amount = (int)Math.Round(ev.Amount);
            if (ev.Amount == -1 || ev.Amount > 150) amount = 150;

            OrganizedDamageStats stats = GetStat<OrganizedDamageStats>();

            if (ECheck(ev.Player))
            {
                // Stats
                stats.TotalDamage += amount;

                // Check damage type
                if (!stats.DamageByType.ContainsKey(ev.DamageHandler.Type))
                    stats.DamageByType.Add(ev.DamageHandler.Type, amount);
                else
                    stats.DamageByType[ev.DamageHandler.Type] += amount;
            }

            // Check Attacker
            if (ECheck(ev.Attacker))
            {
                stats.PlayerDamage += amount;
                if (!stats.DamageByPlayer.ContainsKey(ev.Attacker))
                    stats.DamageByPlayer.Add(ev.Attacker, amount);
                else
                    stats.DamageByPlayer[ev.Attacker] += amount;
            }

            Hold(stats);
        }

        /// <summary>
        /// Called when a player dies (rip).
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnDying(DyingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || MainPlugin.IsRestarting) return;
            FinalStats stats = GetStat<FinalStats>();
            OrganizedKillsStats killStats = GetStat<OrganizedKillsStats>();
            stats.TotalDeaths++;
            if (ev.Attacker is not null)
            {
                // Kill logs
                string killerRole = GetRole(ev.Attacker);
                string dyingRole = GetRole(ev.Player);
                killStats.PlayerKills.Insert(0, $"[{Reporter.GetDisplay(Round.ElapsedTime)}] {Reporter.GetDisplay(ev.Attacker, typeof(Player))} [{killerRole}] killed {Reporter.GetDisplay(ev.Player, typeof(Player))} [{dyingRole}]");

                // Kill by player
                if (!killStats.KillsByPlayer.ContainsKey(ev.Attacker))
                    killStats.KillsByPlayer.Add(ev.Attacker, 1);
                else
                    killStats.KillsByPlayer[ev.Attacker]++;

                // Kill by zone
                if (killStats.KillsByZone.ContainsKey(ev.Player.Zone))
                    killStats.KillsByZone[ev.Player.Zone]++;
                else
                    killStats.KillsByZone.Add(ev.Player.Zone, 1);

                // Role kills
                stats.TotalKills++;
                if (GetRole(ev.Attacker) == "UIU")
                {
                    stats.UIUKills++;
                }
                else if (GetRole(ev.Attacker) == "SerpentsHand")
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
                        .Replace("{ROLE}", GetRole(ev.Attacker))
                        .Replace("{TARGET}", Reporter.GetDisplay(ev.Player, typeof(Player)))
                        .Replace("{TARGETROLE}", dyingRole);
                    MainPlugin.Reporter.AddRemark(killText);
                    FirstKill = true;
                }

                // Killer points
                if (ev.Player.Role.Side == ev.Attacker.Role.Side && ev.DamageHandler.Type != DamageType.Scp018)
                    IncrementPoints(ev.Attacker, MvpSettings.Points.KillTeammate, MainPlugin.Translations.KilledTeammate); // Kill teammate
                else if (GetTeam(ev.Player) == "SCPs" && GetRole(ev.Player) != "Scp0492")
                    IncrementPoints(ev.Attacker, MvpSettings.Points.KillScp, MainPlugin.Translations.KilledSCP); // Kill SCP
                else if (GetTeam(ev.Player) == "Scientists")
                    IncrementPoints(ev.Attacker, MvpSettings.Points.KillScientist, MainPlugin.Translations.KilledScientist); // Kill scientist
                else
                    IncrementPoints(ev.Attacker, MvpSettings.Points.KillEnemy, MainPlugin.Translations.KilledEnemy); // Other kills

                // Grant points to SCP-079 if death in a locked down/blackout room
                if (GetTeam(ev.Attacker) == "SCPs")
                {
                    foreach (Player player in Player.List)
                    {
                        if (player.Role is Scp079Role role && role.SubroutineModule.TryGetSubroutine(out Scp079RewardManager manager) && manager._markedRooms.ContainsKey(ev.Player.CurrentRoom.Identifier))
                            IncrementPoints(player, MvpSettings.Points.Scp079AssistKill, MainPlugin.Translations.AssistKill);
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
                stats.Drops[ev.Item.Type]++;
            else
                stats.Drops[ev.Item.Type] = 1;

            if (stats.PlayerDrops.ContainsKey(ev.Player))
                stats.PlayerDrops[ev.Player]++;
            else
                stats.PlayerDrops[ev.Player] = 1;
            Hold(stats);
        }

        /// <summary>
        /// Called when a player enters the pocket dimension.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnEnteringPocketDimension(EnteringPocketDimensionEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Scp106)) return;
            IncrementPoints(ev.Scp106, MvpSettings.Points.Scp106GrabPlayer, MainPlugin.Translations.GrabbedPlayer);
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

            var firearm = (Firearm)ev.Player.CurrentItem;
            if (firearm is not null)
            {
                if (!stats.ShotsByFirearm.ContainsKey(firearm.FirearmType))
                {
                    stats.ShotsByFirearm[firearm.FirearmType] = new(1, stats.TotalShotsFired, () => MainPlugin.Reporter.GetStat<ItemStats>().TotalShotsFired);
                }
                else
                {
                    stats.ShotsByFirearm[firearm.FirearmType].IncrementValue(1, stats.TotalShotsFired);
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
        /// Called when SCP-079 interacts with a door.
        /// </summary>
        /// <param name="ev">Event arguments.</param>
        public void OnScp079TriggeringDoor(TriggeringDoorEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || ev.Door.IsOpen || !ECheck(ev.Player)) return;
            if (Player.List.Count(plr => plr != ev.Player && GetTeam(plr) is "SCPs" && Vector3.Distance(ev.Door.Transform.position, plr.GameObject.transform.position) <= 20) == 0)
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
                    stats.PlayerDoorsClosed[ev.Player]++;
                else
                    stats.PlayerDoorsClosed.Add(ev.Player, 1);
            }
            else
            {
                stats.DoorsOpened++;
                if (stats.PlayerDoorsOpened.ContainsKey(ev.Player))
                    stats.PlayerDoorsOpened[ev.Player]++;
                else
                    stats.PlayerDoorsOpened.Add(ev.Player, 1);

                if (!FirstDoor && ev.Player is not null && MainPlugin.Reporter is not null)
                {
                    FirstDoor = true;
                    string remarkText = MainPlugin.Translations.DoorRemark
                        .Replace("{PLAYER}", Reporter.GetDisplay(ev.Player, typeof(Player)))
                        .Replace("{ROLE}", GetRole(ev.Player))
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
                    .Replace("{ROLE}", GetRole(ev.Player))
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
                stats.Activations.Add(Scp914Object.KnobStatus, 1);
            else
                stats.Activations[Scp914Object.KnobStatus]++;

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
            if (!ev.IsAllowed || !Round.InProgress) return;
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
            if (!ev.IsAllowed || !Round.InProgress) return;
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
            if (!ev.IsAllowed || !Round.InProgress) return;
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
            if (!Round.InProgress) return;
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
            List<RoleTypeId> SCPs = Player.Get(Team.SCPs).Where(ECheck).Select(player => player.Role.Type).ToList();
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
            // Fill out door destroyed stat
            FinalStats stats = GetStat<FinalStats>();
            stats.DoorsDestroyed = Door.List.Count(d => d.IsBroken);

            // Fill out final stats
            stats.EndTime = DateTime.Now;
            stats.WinningTeam = leadingTeam switch
            {
                LeadingTeam.Anomalies => MainPlugin.Translations.ScpTeam,
                LeadingTeam.ChaosInsurgency => MainPlugin.Translations.InsurgencyTeam,
                LeadingTeam.FacilityForces => MainPlugin.Translations.MtfTeam,
                LeadingTeam.Draw => MainPlugin.Translations.Stalemate,
                _ => MainPlugin.Translations.Unknown
            };
            stats.TotalInteractions = Interactions;

            // Set Leading Team
            if (MainPlugin.Reporter is not null)
                MainPlugin.Reporter.WinTeam = leadingTeam;

            // Finish with final stats
            stats.RoundTime = Round.ElapsedTime;
            foreach (Player player in Player.Get(plr => plr.IsAlive && ECheck(plr)))
                stats.SurvivingPlayers.Add($"{Reporter.GetDisplay(player, typeof(Player))} ({GetRole(player)})");

            Hold(stats);

            // Compile MVP info
            if (MvpSettings.MvpEnabled)
            {
                var sortedHumanData = Points[PointTeam.Human].Where(data => data.Value > 0).OrderByDescending(data => data.Value);
                var sortedSCPData = Points[PointTeam.SCP].Where(data => data.Value > 0).OrderByDescending(data => data.Value);
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                MVPStats MVPInfo = GetStat<MVPStats>();
#pragma warning restore SA1312 // Variable names should begin with lower-case letter

                if (sortedHumanData.Count() >= 1)
                {
                    KeyValuePair<Player, int> mvp = sortedHumanData.First();
                    MVPInfo.HumanMVP = mvp.Key.Nickname + $" ({mvp.Value} points)";
                }

                if (sortedSCPData.Count() >= 1)
                {
                    KeyValuePair<Player, int> mvp = sortedSCPData.First();
                    MVPInfo.SCPMVP = mvp.Key.Nickname + $" ({mvp.Value} points)";
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
                stats.Upgrades.Add(type, 1);
            else
                stats.Upgrades[type]++;

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
