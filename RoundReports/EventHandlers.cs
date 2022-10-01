using Exiled.API.Features;
using Exiled.Events.EventArgs;
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

namespace RoundReports
{
    public enum PointTeam
    {
        Human,
        SCP,
    }

    public class EventHandlers
    {
        public List<IReportStat> Holding { get; set; }
        public Dictionary<PointTeam, Dictionary<Player, int>> Points { get; set; } = new();
        public bool FirstEscape { get; set; } = false;
        public bool FirstUpgrade { get; set; } = false;
        public bool FirstKill { get; set; } = false;
        public bool FirstDoor { get; set; } = false;

        public void Hold<T>(T stat)
            where T: class, IReportStat
        {
            Holding.RemoveAll(r => r.GetType() == typeof(T));
            Holding.Add(stat);
        }

        public T GetStat<T>()
            where T: class, IReportStat, new()
        {
            if (Holding.FirstOrDefault(r => r.GetType() == typeof(T)) is not T value)
                value = new T();
            return value;
        }

        public static string GetRole(Player player)
        {
            if (player.SessionVariables.ContainsKey("IsSH"))
                return "SerpentsHand";
            else if (player.SessionVariables.ContainsKey("IsUIU"))
                return "UIU";
            return player.Role.Type.ToString();
        }

        public string GetTeam(Player player)
        {
            if (player.SessionVariables.ContainsKey("IsSH"))
                return "SH";
            else if (player.SessionVariables.ContainsKey("IsUIU"))
                return "UIU";
            return player.Role.Team.ToString();
        }

        public void AddPoints(Player plr, int amount, string reason = "Unknown")
        {
            if (plr.DoNotTrack || GetRole(plr) == "Tutorial" || plr.IsDead || MainPlugin.Configs.IgnoredUsers.Contains(plr.UserId))
                return;

            var PT = plr.IsScp ? PointTeam.SCP : PointTeam.Human;
            if (Points[PT].ContainsKey(plr))
                Points[PT][plr] += amount;
            else
                Points[PT][plr] = amount;

            var logs = GetStat<MVPStats>();
            string str = MainPlugin.Translations.AddPointsLog
                .Replace("{PLAYER}", plr.Nickname)
                .Replace("{ROLE}", GetRole(plr))
                .Replace("{AMOUNT}", amount.ToString())
                .Replace("{REASON}", reason);
            logs.PointLogs.Insert(0, $"[{Reporter.GetDisplay(Round.ElapsedTime)}] {str}");
            Hold(logs);
        }

        public void RemovePoints(Player plr, int amount, string reason = "Unknown")
        {
            if (plr.DoNotTrack || GetRole(plr) == "Tutorial" || plr.IsDead)
                return;

            var PT = plr.IsScp ? PointTeam.SCP : PointTeam.Human;
            if (Points[PT].ContainsKey(plr))
                Points[PT][plr] -= amount;
            else
                Points[PT][plr] = 0 - amount;

            var logs = GetStat<MVPStats>();
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
            Holding = ListPool<IReportStat>.Shared.Rent();
            Points.Clear();
            Points[PointTeam.SCP] = new();
            Points[PointTeam.Human] = new();
            MainPlugin.Reporter = new Reporter();
        }

        public void SendData()
        {
            if (MainPlugin.Reporter is null)
                return;

            // Set Stats
            foreach (var stat in Holding)
                MainPlugin.Reporter.SetStat(stat);

            // Send
            if (!MainPlugin.Reporter.HasSent)
                MainPlugin.Reporter.SendReport();

            ListPool<IReportStat>.Shared.Return(Holding);
        }

        // This is to account for plugins such as SCPSwap, as well as SCP deaths early on, etc.
        private IEnumerator<float> RecordSCPsStats()
        {
            yield return Timing.WaitForSeconds(60f);
            StartingStats stats = GetStat<StartingStats>();
            List<RoleType> SCPs = Player.Get(Team.SCP).Where(plr => ECheck(plr)).Select(player => player.Role.Type).ToList();
            SCPs.Sort();
            stats.SCPs = SCPs;
            Hold(stats);
        }

        public static bool ECheck(Player ply)
        {
            bool flag = true;
            if (ply is not null)
            {

                if (GetRole(ply) == "Tutorial" && MainPlugin.Configs.ExcludeTutorials)
                    return false; // Exit func early (we don't want to show hidden message for tutorial exclusion)

                if (ply.DoNotTrack && MainPlugin.Configs.ExcludeDNTUsers)
                    flag = false;

                if (MainPlugin.Configs.IgnoredUsers.Contains(ply.UserId))
                    flag = false;
            }
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
                    ClassDPersonnel = Player.Get(RoleType.ClassD).Count(player => ECheck(player)),
                    SCPs = Player.Get(Team.SCP).Where(player => ECheck(player)).Select(player => player.Role.Type).ToList(),
                    FacilityGuards = Player.Get(RoleType.FacilityGuard).Count(player => ECheck(player)),
                    Scientists = Player.Get(RoleType.Scientist).Count(player => ECheck(player)),
                    StartTime = DateTime.Now,
                    PlayersAtStart = Player.List.Where(r => !r.IsDead).Count(player => ECheck(player)),
                    Players = new()
                };
                foreach (var player in Player.List.Where(player => ECheck(player) && !player.IsDead))
                    stats.Players.Add($"{Reporter.GetDisplay(player)} [{GetRole(player)}]");
                Hold(stats);
                Timing.RunCoroutine(RecordSCPsStats().CancelWith(Server.Host.GameObject));
            });
        }

        private void FillOutFinalStats(LeadingTeam leadingTeam = LeadingTeam.Draw)
        {
            // Fill out door destroyed stat
            var stats = GetStat<FinalStats>();
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

            // Set Leading Team
            if (MainPlugin.Reporter is not null)
                MainPlugin.Reporter.WinTeam = leadingTeam;

            // Finish with final stats
            stats.RoundTime = Round.ElapsedTime;
            foreach (var player in Player.Get(plr => plr.IsAlive && ECheck(plr)))
                stats.SurvivingPlayers.Add($"{Reporter.GetDisplay(player)} ({GetRole(player)})");

            Hold(stats);

            // Compile MVP info
            var sortedHumanData = Points[PointTeam.Human].OrderByDescending(data => data.Value);
            var sortedSCPData = Points[PointTeam.SCP].OrderByDescending(data => data.Value);
            MVPStats MVPInfo = GetStat<MVPStats>();

            if (sortedHumanData.Count() >= 1)
            {
                var mvp = sortedHumanData.First();
                MVPInfo.HumanMVP = mvp.Key.Nickname + $" ({mvp.Value} points)";
            }
            if (sortedSCPData.Count() >= 1)
            {
                var mvp = sortedSCPData.First();
                MVPInfo.SCPMVP = mvp.Key.Nickname + $" ({mvp.Value} points)";
            }
            MVPInfo.HumanPoints = sortedHumanData.ToDictionary(kp => kp.Key, kp2 => kp2.Value);
            MVPInfo.SCPPoints = sortedSCPData.ToDictionary(kp => kp.Key, kp2 => kp2.Value);
            Hold(MVPInfo);
        }

        public void OnRestarting()
        {
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

        // Credit to RespawnTimer for this method
        private bool IsSerpentsHandTeamSpawnable()
        {
            if (MainPlugin.SerpentsHandAssembly == null)
                return false;

            Type type = MainPlugin.SerpentsHandAssembly.GetType("SerpentsHand");
            var singleton = type.GetField("Singleton").GetValue(null);
            return (bool)type.GetField("IsSpawnable").GetValue(singleton);
        }

        // and this method
        private bool IsUIUTeamSpawnable()
        {
            if (MainPlugin.UIURescueSquadAssembly == null)
                return false;

            return (bool)MainPlugin.UIURescueSquadAssembly.GetType("UIURescueSquad.EventHandlers")?.GetField("IsSpawnable").GetValue(null);
        }

        public void OnRespawningTeam(RespawningTeamEventArgs ev)
        {
            if (!Round.InProgress) return;
            if (!ev.IsAllowed) return;
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
            if (!Round.InProgress || Round.ElapsedTime.TotalMinutes <= 0.5 || !ECheck(ev.Player) || GetTeam(ev.Player) is not ("SH" or "UIU" or "MTF" or "CHI")) return;
            RespawnStats stats = GetStat<RespawnStats>();
            stats.TotalRespawnedPlayers++;
            stats.Respawns.Insert(0, $"[{Reporter.GetDisplay(Round.ElapsedTime)}] " + MainPlugin.Translations.RespawnLog.Replace("{PLAYER}", Reporter.GetDisplay(ev.Player)).Replace("{ROLE}", GetRole(ev.Player)));
            Hold(stats);
        }

        public void OnHurting(HurtingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed) return;
            int amount = (int)Math.Round(ev.Amount);
            if (ev.Amount == -1 || ev.Amount > 150) amount = 150;

            var stats = GetStat<OrganizedDamageStats>();

            if (ECheck(ev.Target))
            {
                // Stats
                stats.TotalDamage += amount;

                // Check damage type
                if (!stats.DamageByType.ContainsKey(ev.Handler.Type))
                    stats.DamageByType.Add(ev.Handler.Type, amount);
                else
                    stats.DamageByType[ev.Handler.Type] += amount;
            }

            // Check Attacker
            if (ev.Attacker is not null && ECheck(ev.Attacker))
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
            if (!Round.InProgress || !ev.IsAllowed) return;
            var stats = GetStat<FinalStats>();
            var killStats = GetStat<OrganizedKillsStats>();
            stats.TotalDeaths++;
            if (ev.Killer is not null)
            {
                // Kill logs
                string killerRole = GetRole(ev.Killer);
                string dyingRole = GetRole(ev.Target);
                killStats.PlayerKills.Insert(0, $"[{Reporter.GetDisplay(Round.ElapsedTime)}] {Reporter.GetDisplay(ev.Killer)} [{killerRole}] killed {Reporter.GetDisplay(ev.Target)} [{dyingRole}]");
                // Kill by player
                if (!killStats.KillsByPlayer.ContainsKey(ev.Killer))
                    killStats.KillsByPlayer.Add(ev.Killer, 1);
                else
                    killStats.KillsByPlayer[ev.Killer]++;

                stats.TotalKills++;
                if (GetRole(ev.Killer) == "UIU")
                    stats.UIUKills++;
                else if (GetRole(ev.Killer) == "SerpentsHand")
                    stats.SerpentsHandKills++;
                else
                {
                    switch (ev.Killer.Role.Team)
                    {
                        case Team.SCP:
                            stats.SCPKills++;
                            break;
                        case Team.CDP:
                            stats.DClassKills++;
                            break;
                        case Team.RSC:
                            stats.ScientistKills++;
                            break;
                        case Team.MTF:
                            stats.MTFKills++;
                            break;
                        case Team.CHI:
                            stats.ChaosKills++;
                            break;
                        case Team.TUT:
                            stats.TutorialKills++;
                            break;
                    }
                }
                // First kill check
                if (!FirstKill && MainPlugin.Reporter is not null && ECheck(ev.Killer))
                {
                    string killText = MainPlugin.Translations.KillRemark
                        .Replace("{PLAYER}", Reporter.GetDisplay(ev.Killer))
                        .Replace("{ROLE}", GetRole(ev.Killer))
                        .Replace("{TARGET}", Reporter.GetDisplay(ev.Target))
                        .Replace("{TARGETROLE}", dyingRole);
                    MainPlugin.Reporter.AddRemark(killText);
                    FirstKill = true;
                }

                // Killer points
                if (ev.Target.Role.Side == ev.Killer.Role.Side)
                    RemovePoints(ev.Killer, 10, MainPlugin.Translations.KilledTeammate); // Kill teammate
                else if (GetTeam(ev.Target) == "SCP")
                    AddPoints(ev.Killer, 10, MainPlugin.Translations.KilledSCP); // Kill SCP
                else if (GetTeam(ev.Target) == "RSC")
                    AddPoints(ev.Killer, 3, MainPlugin.Translations.KilledScientist); // Kill scientist
                else
                    AddPoints(ev.Killer, 2, MainPlugin.Translations.KilledEnemy); // Other kills
            }

            // Kill by type
            if (!killStats.KillsByType.ContainsKey(ev.Handler.Type))
                killStats.KillsByType.Add(ev.Handler.Type, 1);
            else
                killStats.KillsByType[ev.Handler.Type]++;
            Hold(killStats);
            Hold(stats);

            // Target Points
            if (ev.Handler.Type is DamageType.FemurBreaker)
                AddPoints(ev.Target, 6, MainPlugin.Translations.FemurBreakerSacrifice); // Sacrifice
            else if (ev.Handler.Type is DamageType.Warhead or DamageType.Decontamination or DamageType.Tesla)
                RemovePoints(ev.Target, ev.Target.IsScp ? 10 : 2, MainPlugin.Translations.Death); // Dumb causes
            else
                RemovePoints(ev.Target, ev.Target.IsScp ? 5 : 1, MainPlugin.Translations.Death); // Other causes
        }

        public void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player))
                return;
            var stats = GetStat<ItemStats>();
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
            if (!Round.InProgress || !ev.IsAllowed || ev.Scp106 is null) return;
            AddPoints(ev.Scp106, 1, MainPlugin.Translations.GrabbedPlayer);
        }

        public void OnShooting(ShootingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed) return;
            var stats = GetStat<ItemStats>();
            stats.TotalShotsFired++;
            Hold(stats);
        }

        public void OnReloadingWeapon(ReloadingWeaponEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed) return;
            var stats = GetStat<ItemStats>();
            stats.TotalReloads++;
            Hold(stats);
        }

        public void OnScp079GainingLevel(GainingLevelEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed) return;
            AddPoints(ev.Player, 5, MainPlugin.Translations.LeveledUp);
        }

        public void OnScp079Lockdown(LockingDownEventArgs ev)
        {
            Timing.CallDelayed(1.15f, () =>
            {
                int count = Player.List.Count(plr => plr.CurrentRoom.RoomIdentifier == ev.RoomGameObject && GetTeam(plr) is not ("SCP" or "SH") && GetRole(plr) is not "Tutorial");
                AddPoints(ev.Player, count, MainPlugin.Translations.RoomLockdown);
            });
        }

        public void OnScp079TriggeringDoor(TriggeringDoorEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || ev.Door.IsOpen) return;
            if (Player.List.Count(plr => plr != ev.Player && GetTeam(plr) is "SCP" && Vector3.Distance(ev.Door.Transform.position, plr.GameObject.transform.position) <= 20) == 0)
                return;
            
            int pts = 1;
            if (ev.Door.IsKeycardDoor)
            {
                if (ev.Door.IsGate) pts++;
                pts++;
            };
            AddPoints(ev.Player, pts, MainPlugin.Translations.OpenedDoor);
        }

        public void OnScp096Charge(ChargingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            var stats = GetStat<SCPStats>();
            stats.Scp096Charges++;
            Hold(stats);
        }

        public void OnScp096Enrage(EnragingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            var stats = GetStat<SCPStats>();
            stats.Scp096Enrages++;
            Hold(stats);
        }

        public void OnContaining106(ContainingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed) return; // Todo: Add ECheck and femur breaker activator
            var stats = GetStat<SCPStats>();
            stats.FemurBreakerActivated = true;
            Hold(stats);
            AddPoints(ev.ButtonPresser, 4, MainPlugin.Translations.FemurBreakerActivator);
        }

        public void OnScp106Teleport(TeleportingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            var stats = GetStat<SCPStats>();
            stats.Scp106Teleports++;
            Hold(stats);
        }

        public void OnScp173Blink(BlinkingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed || !ECheck(ev.Player)) return;
            var stats = GetStat<SCPStats>();
            stats.Scp173Blinks++;
            Hold(stats);
        }

        public void OnUsedItem(UsedItemEventArgs ev)
        {
            if (!Round.InProgress || !ECheck(ev.Player)) return;
            if (ev.Item.IsScp)
            {
                var stats = GetStat<SCPStats>();
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
                var stats = GetStat<ItemStats>();
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
            var stats = GetStat<FinalStats>();
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
                        .Replace("{PLAYER}", Reporter.GetDisplay(ev.Player))
                        .Replace("{ROLE}", GetRole(ev.Player))
                        .Replace("{MILLISECOND}", Round.ElapsedTime.Milliseconds.ToString());
                    MainPlugin.Reporter.AddRemark(remarkText);
                }
            }
            if (ev.Door.IsKeycardDoor)
            {
                var itemStats = GetStat<ItemStats>();
                itemStats.KeycardScans++;
                Hold(itemStats);
            }
            Hold(stats);
        }

        public void OnInteractingScp330(InteractingScp330EventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress || !ECheck(ev.Player)) return;
            var stats = GetStat<SCPStats>();
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
        }

        public void OnEscaping(EscapingEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress || !ECheck(ev.Player)) return;
            if (!FirstEscape && MainPlugin.Reporter is not null)
            {
                FirstEscape = true;
                string escapeText = MainPlugin.Translations.EscapeRemark
                    .Replace("{PLAYER}", Reporter.GetDisplay(ev.Player))
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
            var stats = GetStat<SCPStats>();
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
        }

        private void UpgradeItemLog(ItemType type, Scp914KnobSetting mode)
        {
            var stats = GetStat<SCPStats>();
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

        public void On914UpgradingItem(UpgradingItemEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress) return;
            UpgradeItemLog(ev.Item.Type, ev.KnobSetting);
        }

        public void On914UpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress || !ECheck(ev.Player)) return;
            UpgradeItemLog(ev.Item.Type, ev.KnobSetting);
        }

        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress) return;
            var stats = GetStat<FinalStats>();
            stats.ButtonUnlocked = true;
            if (stats.ButtonUnlocker == null)
            {
                stats.ButtonUnlocker = ev.Player;
                AddPoints(ev.Player, 2, MainPlugin.Translations.OpenedWarheadPanel);
            }
            Hold(stats);
        }

        public void OnWarheadStarting(StartingEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress) return;
            var stats = GetStat<FinalStats>();
            stats.FirstActivator ??= ev.Player;
            Hold(stats);
        }

        public void OnWarheadDetonated()
        {
            if (!Round.InProgress) return;
            var stats = GetStat<FinalStats>();
            if (!stats.Detonated)
            {
                stats.Detonated = true;
                stats.DetonationTime = DateTime.Now;
            }
            Hold(stats);
        }
    }
}
