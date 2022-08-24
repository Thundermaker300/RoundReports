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
            Holding = ListPool<IReportStat>.Shared.Rent();
            MainPlugin.Reporter = new Reporter(MainPlugin.Singleton.Config.DiscordWebhook);
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
                    if (player.DoNotTrack)
                    {
                        stats.Players.Add($"{Reporter.DoNotTrackText} [{player.Role.Type}]");
                    }
                    else
                    {
                        stats.Players.Add($"{player.Nickname} [{player.Role.Type}]");
                    }
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
            var doorStats = GetStat<DoorStats>();
            doorStats.DoorsDestroyed = Door.List.Count(d => d.IsBroken);
            Hold(doorStats);
            // Fill out final stats
            var stats = GetStat<FinalStats>();
            stats.EndTime = DateTime.Now;
            stats.WinningTeam = ev.LeadingTeam switch
            {
                LeadingTeam.Anomalies => "SCPs",
                LeadingTeam.ChaosInsurgency => "Insurgency",
                LeadingTeam.FacilityForces => "Mobile Task Force",
                LeadingTeam.Draw => "Stalemate",
                _ => "Unknown"
            };
            stats.RoundTime = Round.ElapsedTime;
            foreach (var player in Player.Get(plr => plr.IsAlive))
            {
                string role = GetRole(player);
                stats.SurvivingPlayers.Add($"{(player.DoNotTrack ? Reporter.DoNotTrackText : player.Nickname)} ({role})");
            }
            Hold(stats);

            // Send stats
            if (MainPlugin.Reporter is not null)
            {
                MainPlugin.Reporter.LeadingTeam = ev.LeadingTeam;
            }
            SendData();
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
            if (!Round.IsStarted || !ev.IsAllowed) return;
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
            if (!Round.IsStarted || !ev.IsAllowed) return;
            var stats = GetStat<FinalStats>();
            var killStats = GetStat<OrganizedKillsStats>();
            stats.TotalDeaths++;
            if (ev.Killer is not null)
            {
                // Kill logs
                string killerRole = GetRole(ev.Killer);
                string dyingRole = GetRole(ev.Target);
                killStats.PlayerKills.Insert(0, $"[{DateTime.Now.ToString("hh:mm:ss tt")}] {(ev.Killer.DoNotTrack ? Reporter.DoNotTrackText : ev.Killer.Nickname)} [{killerRole}] killed {(ev.Target.DoNotTrack ? Reporter.DoNotTrackText : ev.Target.Nickname)} [{dyingRole}]");
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
                    case Team.TUT when GetRole(ev.Killer) == "SerpentsHand":
                        stats.SerpentsHandKills++;
                        break;
                    case Team.TUT:
                        stats.TutorialKills++;
                        break;
                }
                // First kill check
                if (!FirstKill)
                {
                    MainPlugin.Reporter.AddRemark($"{(ev.Killer.DoNotTrack ? Reporter.DoNotTrackText : ev.Killer.Nickname)} [{killerRole}] killed first! They killed {(ev.Target.DoNotTrack ? Reporter.DoNotTrackText : ev.Target.Nickname)} [{dyingRole}]");
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

        public void OnUsedItem(UsedItemEventArgs ev)
        {
            if (!Round.IsStarted) return;
            if (ev.Item.IsScp)
            {
                var stats = GetStat<SCPItemStats>();
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
                var stats = GetStat<MedicalStats>();
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
            if (!Round.IsStarted) return;
            if (ev.Player is null) return;
            var stats = GetStat<DoorStats>();
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
            }
            Hold(stats);
        }

        public void OnInteractingScp330(InteractingScp330EventArgs ev)
        {
            if (!ev.IsAllowed || !Round.IsStarted) return;
            var stats = GetStat<Scp330Stats>();
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
            if (!ev.IsAllowed || !Round.IsStarted) return;
            if (!FirstEscape && MainPlugin.Reporter is not null)
            {
                FirstEscape = true;
                MainPlugin.Reporter.AddRemark($"{Reporter.GetDisplay(ev.Player)} [{ev.Player.Role.Type}] was the first to escape ({Round.ElapsedTime.Minutes}m{Round.ElapsedTime.Seconds}s)!");
            }
        }

        public void OnActivatingScp914(ActivatingEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.IsStarted) return;
            var stats = GetStat<Scp914Stats>();
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
            var stats = GetStat<Scp914Stats>();
            stats.TotalItemUpgrades++;
            if (!FirstUpgrade)
            {
                FirstUpgrade = true;
                MainPlugin.Reporter.AddRemark($"The first item to be upgraded in SCP-914 was a {type} on {mode}.");
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
            if (!ev.IsAllowed || !Round.IsStarted) return;
            UpgradeItemLog(ev.Item.Type, ev.KnobSetting);
        }

        public void On914UpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.IsStarted) return;
            UpgradeItemLog(ev.Item.Type, ev.KnobSetting);
        }

        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.IsStarted) return;
            var stats = GetStat<WarheadStats>();
            stats.ButtonUnlocked = true;
            if (stats.ButtonUnlocker == null)
                stats.ButtonUnlocker = ev.Player;
            Hold(stats);
        }

        public void OnWarheadStarting(StartingEventArgs ev)
        {
            if (!ev.IsAllowed || !Round.IsStarted) return;
            var stats = GetStat<WarheadStats>();
            if (stats.FirstActivator == null)
                stats.FirstActivator = ev.Player;
            Hold(stats);
        }

        public void OnWarheadDetonated()
        {
            if (!Round.IsStarted) return;
            var stats = GetStat<WarheadStats>();
            if (!stats.Detonated)
            {
                stats.Detonated = true;
                stats.DetonationTime = DateTime.Now;
            }
            Hold(stats);
        }
    }
}
