using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using NorthwoodLib.Pools;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Scp914;

using Scp914Object = Exiled.API.Features.Scp914;
using UnityEngine;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using Exiled.Events.EventArgs.Scp079;
using Exiled.Events.EventArgs.Scp096;
using Exiled.Events.EventArgs.Scp106;
using Exiled.Events.EventArgs.Scp173;
using Exiled.Events.EventArgs.Scp330;
using Exiled.Events.EventArgs.Scp914;
using Exiled.Events.EventArgs.Warhead;
using Exiled.Events.EventArgs.Scp939;

namespace RoundReports
{
    public class EventHandlers
    {
        public List<IReportStat> Holding { get; set; }
        public Dictionary<PointTeam, Dictionary<Player, int>> Points { get; set; } = new();
        public bool FirstEscape { get; set; } = false;
        public bool FirstUpgrade { get; set; } = false;
        public bool FirstKill { get; set; } = false;
        public bool FirstDoor { get; set; } = false;
        public int Interactions { get; set; } = 0;

        /// <summary>
        /// Holds a stat, preserving its data until the end of the round. Wipes any previously stored data.
        /// </summary>
        /// <typeparam name="T">Stat to hold.</typeparam>
        /// <param name="stat">The data of the stat.</param>
        public void Hold<T>(T stat)
            where T: class, IReportStat
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
            where T: class, IReportStat, new()
        {
            if (Holding.FirstOrDefault(r => r.GetType() == typeof(T)) is not T value)
            {
                value = new T();
                value.Setup();
            }
            return value;
        }

        /// <summary>
        /// Returns string of role. Will return SerpentsHand or UIU for respective roles.
        /// </summary>
        /// <param name="player">Player</param>
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
        public string GetTeam(Player player)
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
        /// Adds MVP points. Ignored if user: Is null, Is DNT, Is role Tutorial, IsDead, or is an ignored user.
        /// </summary>
        /// <param name="plr">Player.</param>
        /// <param name="amount">Amount of points.</param>
        /// <param name="reason">Reason for adding.</param>
        public void AddPoints(Player plr, int amount, string reason = "Unknown")
        {
            if (plr is null || plr.DoNotTrack || GetRole(plr) == "Tutorial" || plr.IsDead || MainPlugin.Configs.IgnoredUsers.Contains(plr.UserId))
                return;

            Log.Debug($"Adding {amount} points to {plr.Nickname}. {reason}");

            PointTeam PT = plr.IsScp ? PointTeam.SCP : PointTeam.Human;
            if (Points[PT].ContainsKey(plr))
                Points[PT][plr] += amount;
            else
                Points[PT][plr] = amount;

            MVPStats logs = GetStat<MVPStats>();
            string str = MainPlugin.Translations.AddPointsLog
                .Replace("{PLAYER}", plr.Nickname)
                .Replace("{ROLE}", GetRole(plr))
                .Replace("{AMOUNT}", amount.ToString())
                .Replace("{REASON}", reason);
            logs.PointLogs.Insert(0, $"[{Reporter.GetDisplay(Round.ElapsedTime)}] {str}");
            Hold(logs);
        }

        /// <summary>
        /// Removes MVP points. Ignored if user: Is null, Is DNT, Is role Tutorial, IsDead, or is an ignored user.
        /// </summary>
        /// <param name="plr">Player.</param>
        /// <param name="amount">Amount of points.</param>
        /// <param name="reason">Reason for removing.</param>
        public void RemovePoints(Player plr, int amount, string reason = "Unknown")
        {
            if (plr is null || plr.DoNotTrack || GetRole(plr) == "Tutorial" || plr.IsDead || MainPlugin.Configs.IgnoredUsers.Contains(plr.UserId))
                return;

            Log.Debug($"Removing {amount} points from {plr.Nickname}. {reason}");

            PointTeam PT = plr.IsScp ? PointTeam.SCP : PointTeam.Human;
            if (Points[PT].ContainsKey(plr))
                Points[PT][plr] -= amount;
            else
                Points[PT][plr] = 0 - amount;

            MVPStats logs = GetStat<MVPStats>();
            string str = MainPlugin.Translations.RemovePointsLog
                .Replace("{PLAYER}", plr.Nickname)
                .Replace("{ROLE}", GetRole(plr))
                .Replace("{AMOUNT}", amount.ToString())
                .Replace("{REASON}", reason);
            logs.PointLogs.Insert(0, $"[{Reporter.GetDisplay(Round.ElapsedTime)}] {str}");
            Hold(logs);
        }

        public void OnWaitingForPlayers()
        {
            FirstEscape = false;
            FirstUpgrade = false;
            FirstKill = false;
            FirstDoor = false;
            Interactions = 0;
            Holding = ListPool<IReportStat>.Shared.Rent();
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

            ListPool<IReportStat>.Shared.Return(Holding);
        }

        /// <summary>
        /// This is to account for plugins such as SCPSwap, as well as SCP deaths early on, etc.
        /// </summary>
        /// <returns>Coroutine.</returns>
        private IEnumerator<float> RecordSCPsStats()
        {
            yield return Timing.WaitForSeconds(60f);
            StartingStats stats = GetStat<StartingStats>();
            List<RoleTypeId> SCPs = Player.Get(Team.SCPs).Where(plr => ECheck(plr)).Select(player => player.Role.Type).ToList();
            SCPs.Sort();
            stats.SCPs = SCPs;
            Hold(stats);
        }

        /// <summary>
        /// Returns if stats related to a player should be recorded.
        /// </summary>
        /// <param name="ply">The player to check.</param>
        /// <returns>Boolean.</returns>
        public static bool ECheck(Player ply)
        {
            if (ply is null)
                return false;

            bool flag = true;
            if (ply is not null)
            if (GetRole(ply) == "Tutorial" && MainPlugin.Configs.ExcludeTutorials)
                return false; // Exit func early (we don't want to show hidden message for tutorial exclusion)

            if (ply.DoNotTrack && MainPlugin.Configs.ExcludeDNTUsers)
                flag = false;

            if (MainPlugin.Configs.IgnoredUsers.Contains(ply.UserId))
                flag = false;

            if (!flag && MainPlugin.Reporter is not null)
                MainPlugin.Reporter.AtLeastOneHidden = true;
            return flag;
        }

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
                    Players = ListPool<string>.Shared.Rent()
                };
                foreach (Player player in Player.List.Where(player => ECheck(player) && !player.IsDead))
                    stats.Players.Add($"{Reporter.GetDisplay(player, typeof(Player))} [{GetRole(player)}]");
                Hold(stats);
                Timing.RunCoroutine(RecordSCPsStats().CancelWith(Server.Host.GameObject));
            });
        }

        /// <summary>
        /// Fills out <see cref="FinalStats"/>.
        /// </summary>
        /// <param name="leadingTeam"></param>
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
            var sortedHumanData = Points[PointTeam.Human].OrderByDescending(data => data.Value);
            var sortedSCPData = Points[PointTeam.SCP].OrderByDescending(data => data.Value);
            MVPStats MVPInfo = GetStat<MVPStats>();

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

        public void OnRestarting()
        {
            MainPlugin.IsRestarting = true;
            if (MainPlugin.Reporter is not null && !MainPlugin.Reporter.HasSent)
            {
                FillOutFinalStats();
                SendData();
            }
        }

        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            if (MainPlugin.Reporter is not null && !MainPlugin.Reporter.HasSent)
            {
                FillOutFinalStats(ev.LeadingTeam);
                SendData();
            }
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

        public void OnSpawned(SpawnedEventArgs ev)
        {
            if (!Round.InProgress || MainPlugin.IsRestarting || Round.ElapsedTime.TotalMinutes <= 0.5 || !ECheck(ev.Player) || GetTeam(ev.Player) is not ("SH" or "UIU" or "FacilityForces" or "ChaosInsurgency")) return;
            RespawnStats stats = GetStat<RespawnStats>();
            stats.TotalRespawned++;
            stats.Respawns.Insert(0, $"[{Reporter.GetDisplay(Round.ElapsedTime)}] " + MainPlugin.Translations.RespawnLog.Replace("{PLAYER}", Reporter.GetDisplay(ev.Player, typeof(Player))).Replace("{ROLE}", GetRole(ev.Player)));
            Hold(stats);
        }

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
                    stats.UIUKills++;
                else if (GetRole(ev.Attacker) == "SerpentsHand")
                    stats.SerpentsHandKills++;
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
                if (ev.Player.Role.Side == ev.Attacker.Role.Side)
                    RemovePoints(ev.Attacker, 10, MainPlugin.Translations.KilledTeammate); // Kill teammate
                else if (GetTeam(ev.Player) == "SCPs")
                    AddPoints(ev.Attacker, 10, MainPlugin.Translations.KilledSCP); // Kill SCP
                else if (GetTeam(ev.Player) == "Scientists")
                    AddPoints(ev.Attacker, 3, MainPlugin.Translations.KilledScientist); // Kill scientist
                else
                    AddPoints(ev.Attacker, 2, MainPlugin.Translations.KilledEnemy); // Other kills

                // Grant points to SCP-079 if death in a locked down/blackout room
                if (GetTeam(ev.Attacker) == "SCPs" && (ev.Player.CurrentRoom.AreLightsOff || ev.Player.CurrentRoom.Doors.All(door => door.IsLocked)))
                {
                    foreach (Player player in Player.List)
                    {
                        if (player.Role == RoleTypeId.Scp079)
                            AddPoints(player, 1, MainPlugin.Translations.AssistKill);
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
            if (ev.DamageHandler.Type is DamageType.Warhead or DamageType.Decontamination or DamageType.Tesla)
                RemovePoints(ev.Player, ev.Player.IsScp ? 10 : 2, MainPlugin.Translations.Death); // Dumb causes
            else
                RemovePoints(ev.Player, ev.Player.IsScp ? 5 : 1, MainPlugin.Translations.Death); // Other causes
        }

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

        public void OnEnteringPocketDimension(EnteringPocketDimensionEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Scp106)) return;
            AddPoints(ev.Scp106, 1, MainPlugin.Translations.GrabbedPlayer);
        }

        public void OnShooting(ShootingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            ItemStats stats = GetStat<ItemStats>();
            stats.TotalShotsFired++;
            Hold(stats);
        }

        public void OnReloadingWeapon(ReloadingWeaponEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            ItemStats stats = GetStat<ItemStats>();
            stats.TotalReloads++;
            Hold(stats);
        }

        public void OnScp079GainingLevel(GainingLevelEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            AddPoints(ev.Player, 5, MainPlugin.Translations.LeveledUp);
        }

        public void OnScp079InteractTesla(InteractingTeslaEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            foreach (Player player in Player.List)
            {
                if (ev.Tesla.IsPlayerInHurtRange(player))
                {
                    AddPoints(ev.Player, 5, MainPlugin.Translations.TeslaKill);
                    break;
                }
            }
            Interactions++;
        }

        public void OnScp079TriggeringDoor(TriggeringDoorEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || ev.Door.IsOpen || !ECheck(ev.Player)) return;
            if (Player.List.Count(plr => plr != ev.Player && GetTeam(plr) is "SCPs" && Vector3.Distance(ev.Door.Transform.position, plr.GameObject.transform.position) <= 20) == 0)
                return;
            
            int pts = 0;
            if (ev.Door.IsKeycardDoor)
            {
                if (ev.Door.IsGate) pts++;
                pts++;
            };
            AddPoints(ev.Player, pts, MainPlugin.Translations.OpenedDoor);
            Interactions++;
        }

        public void OnScp096Charge(ChargingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            stats.Scp096Charges++;
            Hold(stats);
        }

        public void OnScp096Enrage(EnragingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            stats.Scp096Enrages++;
            Hold(stats);
        }

        public void OnScp106Teleport(TeleportingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            stats.Scp106Teleports++;
            Hold(stats);
        }

        public void OnScp173Blink(BlinkingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            stats.Scp173Blinks++;
            Hold(stats);
        }

        public void OnScp173Tantrum(PlacingTantrumEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            stats.Scp173Tantrums++;
            Hold(stats);
        }

        public void OnScp939Lunge(LungingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            stats.Scp939Lunges++;
            Hold(stats);
        }

        public void OnScp939Cloud(PlacingAmnesticCloudEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            SCPStats stats = GetStat<SCPStats>();
            stats.Scp939Clouds++;
            Hold(stats);
        }

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

        public void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            Interactions++;
        }

        public void OnInteractingLocker(InteractingLockerEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            Interactions++;
        }

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
                RemovePoints(ev.Player, 10, MainPlugin.Translations.Took3Candies);
            }

            // Candies Taken
            if (!stats.CandiesTaken.ContainsKey(ev.Candy))
                stats.CandiesTaken.Add(ev.Candy, 1);
            else
                stats.CandiesTaken[ev.Candy]++;
            Hold(stats);
            Interactions++;
        }

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
            AddPoints(ev.Player, 5, MainPlugin.Translations.Escaped);
        }

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

        public void OnChangingKnobSetting(ChangingKnobSettingEventArgs ev)
        {
            Interactions++;
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

        public void On914UpgradingPickup(UpgradingPickupEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress) return;
            UpgradeItemLog(ev.Pickup.Type, ev.KnobSetting);
        }

        public void On914UpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress || !ECheck(ev.Player)) return;
            UpgradeItemLog(ev.Item.Type, ev.KnobSetting);
        }

        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress) return;
            FinalStats stats = GetStat<FinalStats>();
            stats.ButtonUnlocked = true;
            if (stats.ButtonUnlocker == null)
            {
                stats.ButtonUnlocker = ev.Player;
                AddPoints(ev.Player, 2, MainPlugin.Translations.OpenedWarheadPanel);
            }
            Hold(stats);
            Interactions++;
        }

        public void OnWarheadStarting(StartingEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress) return;
            FinalStats stats = GetStat<FinalStats>();
            stats.FirstActivator ??= ev.Player;
            Hold(stats);
            Interactions++;
        }

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
    }
}
