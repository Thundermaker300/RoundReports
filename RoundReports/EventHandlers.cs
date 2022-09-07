using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NorthwoodLib.Pools;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Scp914;

using Scp914Object = Exiled.API.Features.Scp914;

namespace RoundReports
{
    public class EventHandlers
    {
        public List<IReportStat> Holding { get; set; }
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
            var value = Holding.FirstOrDefault(r => r.GetType() == typeof(T)) as T;
            if (value is null)
            {
                value = new T();
            }
            return value;
        }

        public string GetRole(Player player)
        {
            if (player.SessionVariables.ContainsKey("IsSH"))
            {
                return "SerpentsHand";
            }
            else if (player.SessionVariables.ContainsKey("IsUIU"))
            {
                return "UIU";
            }
            return player.Role.Type.ToString();
        }

        public void OnWaitingForPlayers()
        {
            FirstEscape = false;
            FirstUpgrade = false;
            FirstKill = false;
            FirstDoor = false;
            Holding = ListPool<IReportStat>.Shared.Rent();
            MainPlugin.Reporter = new Reporter();
        }

        public void SendData()
        {
            if (MainPlugin.Reporter is null)
                return;

            foreach (var stat in Holding)
            {
                MainPlugin.Reporter.SetStat(stat);
            }
            if (!MainPlugin.Reporter.HasSent)
            {
                MainPlugin.Reporter.SendReport();
            }
            ListPool<IReportStat>.Shared.Return(Holding);
        }

        public void OnRoundStarted()
        {
            Timing.CallDelayed(.5f, () =>
            {
                StartingStats stats = new()
                {
                    ClassDPersonnel = Player.Get(RoleType.ClassD).Count(),
                    SCPs = Player.Get(Team.SCP).Count(),
                    FacilityGuards = Player.Get(RoleType.FacilityGuard).Count(),
                    Scientists = Player.Get(RoleType.Scientist).Count(),
                    StartTime = DateTime.Now,
                    PlayersAtStart = Player.List.Where(r => !r.IsDead).Count(),
                    Players = new()
                };
                foreach (var player in Player.List)
                {
                    stats.Players.Add($"{Reporter.GetDisplay(player)} [{GetRole(player)}]");
                }
                Hold(stats);
            });
        }

        public void OnRestarting()
        {
            if (MainPlugin.Reporter is not null && !MainPlugin.Reporter.HasSent)
            {
                SendData();
            }
        }

        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            // Fill out door destroyed stat
            var stats = GetStat<FinalStats>();
            stats.DoorsDestroyed = Door.List.Count(d => d.IsBroken);
            // Fill out final stats
            stats.EndTime = DateTime.Now;
            stats.WinningTeam = ev.LeadingTeam switch
            {
                LeadingTeam.Anomalies => MainPlugin.Translations.ScpTeam,
                LeadingTeam.ChaosInsurgency => MainPlugin.Translations.InsurgencyTeam,
                LeadingTeam.FacilityForces => MainPlugin.Translations.MtfTeam,
                LeadingTeam.Draw => MainPlugin.Translations.Stalemate,
                _ => MainPlugin.Translations.Unknown
            };
            if (MainPlugin.Reporter is not null)
            {
                MainPlugin.Reporter.WinTeam = ev.LeadingTeam;
            }

            stats.RoundTime = Round.ElapsedTime;
            foreach (var player in Player.Get(plr => plr.IsAlive))
            {
                string role = GetRole(player);
                stats.SurvivingPlayers.Add($"{Reporter.GetDisplay(player)} ({role})");
            }
            Hold(stats);

            // Send stats
            SendData();

            // Broadcast
            if (MainPlugin.Singleton.Config.EndingBroadcast?.Show == true)
            {
                Map.Broadcast(MainPlugin.Singleton.Config.EndingBroadcast, true);
            }
        }

        public void OnLeft(LeftEventArgs ev)
        {
            if (!Reporter.NameStore.ContainsKey(ev.Player))
            {
                if (!ev.Player.DoNotTrack)
                    Reporter.NameStore.Add(ev.Player, ev.Player.Nickname);
                else
                    Reporter.NameStore.Add(ev.Player, Reporter.DoNotTrackText);
            }
        }

        public void OnHurting(HurtingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed) return;
            int amount = (int)Math.Round(ev.Amount);
            if (!ev.IsAllowed) return;
            if (ev.Amount == -1 || ev.Amount > 150) amount = 150;

            // Stats
            var stats = GetStat<OrganizedDamageStats>();
            stats.TotalDamage += amount;

            // Check damage type
            if (!stats.DamageByType.ContainsKey(ev.Handler.Type))
            {
                stats.DamageByType.Add(ev.Handler.Type, amount);
            }
            else
            {
                stats.DamageByType[ev.Handler.Type] += amount;
            }

            // Check Attacker
            if (ev.Attacker is not null)
            {
                stats.PlayerDamage += amount;
                if (!stats.DamageByPlayer.ContainsKey(ev.Attacker))
                {
                    stats.DamageByPlayer.Add(ev.Attacker, amount);
                }
                else
                {
                    stats.DamageByPlayer[ev.Attacker] += amount;
                }
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
                killStats.PlayerKills.Insert(0, $"[{DateTime.Now.ToString("hh:mm:ss tt")}] {Reporter.GetDisplay(ev.Killer)} [{killerRole}] killed {Reporter.GetDisplay(ev.Target)} [{dyingRole}]");
                // Kill by player
                if (!killStats.KillsByPlayer.ContainsKey(ev.Killer))
                {
                    killStats.KillsByPlayer.Add(ev.Killer, 1);
                }
                else
                {
                    killStats.KillsByPlayer[ev.Killer]++;
                }
                stats.TotalKills++;
                if (GetRole(ev.Killer) == "UIU")
                {
                    stats.UIUKills++;
                }
                else if (GetRole(ev.Killer) == "SerpentsHand")
                {
                    stats.SerpentsHandKills++;
                }
                else
                {
                    switch (ev.Killer.Role.Team)
                    {
                        case Team.SCP:
                            stats.MTFKills++;
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
                if (!FirstKill && MainPlugin.Reporter is not null)
                {
                    string killText = MainPlugin.Translations.KillRemark
                        .Replace("{PLAYER}", Reporter.GetDisplay(ev.Killer))
                        .Replace("{ROLE}", GetRole(ev.Killer))
                        .Replace("{TARGET}", Reporter.GetDisplay(ev.Target))
                        .Replace("{TARGETROLE}", dyingRole);
                    MainPlugin.Reporter.AddRemark(killText);
                    FirstKill = true;
                }
            }
            // Kill by type
            if (!killStats.KillsByType.ContainsKey(ev.Handler.Type))
            {
                killStats.KillsByType.Add(ev.Handler.Type, 1);
            }
            else
            {
                killStats.KillsByType[ev.Handler.Type]++;
            }
            Hold(killStats);
            Hold(stats);
        }

        public void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed)
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

        public void OnScp096Charge(ChargingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed) return;
            var stats = GetStat<SCPStats>();
            stats.Scp096Charges++;
            Hold(stats);
        }

        public void OnScp096Enrage(EnragingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed) return;
            var stats = GetStat<SCPStats>();
            stats.Scp096Enrages++;
            Hold(stats);
        }

        public void OnContaining106(ContainingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed) return;
            var stats = GetStat<SCPStats>();
            stats.FemurBreakerActivated = true;
            Hold(stats);
        }

        public void OnScp106Teleport(TeleportingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed) return;
            var stats = GetStat<SCPStats>();
            stats.Scp106Teleports++;
            Hold(stats);
        }

        public void OnScp173Blink(BlinkingEventArgs ev)
        {
            if (!Round.InProgress || !ev.IsAllowed) return;
            var stats = GetStat<SCPStats>();
            stats.Scp173Blinks++;
            Hold(stats);
        }

        public void OnUsedItem(UsedItemEventArgs ev)
        {
            if (!Round.InProgress) return;
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
            if (!Round.InProgress || !ev.IsAllowed) return;
            if (ev.Player is null) return;
            var stats = GetStat<FinalStats>();
            if (ev.Door.IsOpen)
            {
                stats.DoorsClosed++;
                if (stats.PlayerDoorsClosed.ContainsKey(ev.Player))
                {
                    stats.PlayerDoorsClosed[ev.Player]++;
                }
                else
                {
                    stats.PlayerDoorsClosed.Add(ev.Player, 1);
                }
            }
            else
            {
                stats.DoorsOpened++;
                if (stats.PlayerDoorsOpened.ContainsKey(ev.Player))
                {
                    stats.PlayerDoorsOpened[ev.Player]++;
                }
                else
                {
                    stats.PlayerDoorsOpened.Add(ev.Player, 1);
                }

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
            if (!ev.IsAllowed || !Round.InProgress) return;
            var stats = GetStat<SCPStats>();
            if (stats.FirstUse == DateTime.MinValue)
            {
                stats.FirstUse = DateTime.Now;
                stats.FirstUser = ev.Player;
            }

            stats.TotalCandiesTaken++;
            if (!stats.CandiesByPlayer.ContainsKey(ev.Player))
            {
                stats.CandiesByPlayer.Add(ev.Player, 1);
            }
            else
            {
                stats.CandiesByPlayer[ev.Player]++;
            }

            // Hands
            if (ev.ShouldSever)
            {
                stats.SeveredHands++;
            }

            // Candies Taken
            if (!stats.CandiesTaken.ContainsKey(ev.Candy))
            {
                stats.CandiesTaken.Add(ev.Candy, 1);
            }
            else
            {
                stats.CandiesTaken[ev.Candy]++;
            }
            Hold(stats);
        }

        public void OnEscaping(EscapingEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress) return;
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
        }

        public void OnActivatingScp914(ActivatingEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress) return;
            var stats = GetStat<SCPStats>();
            if (stats.FirstActivation == DateTime.MinValue && ev.Player is not null)
            {
                stats.FirstActivation = DateTime.Now;
                stats.FirstActivator = ev.Player;
            }
            stats.TotalActivations++;
            if (!stats.Activations.ContainsKey(Scp914Object.KnobStatus))
            {
                stats.Activations.Add(Scp914Object.KnobStatus, 1);
            }
            else
            {
                stats.Activations[Scp914Object.KnobStatus]++;
            }
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
            {
                stats.Upgrades.Add(type, 1);
            }
            else
            {
                stats.Upgrades[type]++;
            }
            Hold(stats);
        }

        public void On914UpgradingItem(UpgradingItemEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress) return;
            UpgradeItemLog(ev.Item.Type, ev.KnobSetting);
        }

        public void On914UpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress) return;
            UpgradeItemLog(ev.Item.Type, ev.KnobSetting);
        }

        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress) return;
            var stats = GetStat<FinalStats>();
            stats.ButtonUnlocked = true;
            if (stats.ButtonUnlocker == null)
                stats.ButtonUnlocker = ev.Player;
            Hold(stats);
        }

        public void OnWarheadStarting(StartingEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.InProgress) return;
            var stats = GetStat<FinalStats>();
            if (stats.FirstActivator == null)
                stats.FirstActivator = ev.Player;
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
