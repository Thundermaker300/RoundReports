using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NorthwoodLib.Pools;

namespace RoundReports
{
    public class EventHandlers
    {
        public List<IReportStat> Holding { get; set; }

        public T TryGetStat<T>()
            where T: class, IReportStat
        {
            return Holding.FirstOrDefault(r => r.GetType() == typeof(T)) as T;
        }

        public void OnWaitingForPlayers()
        {
            Holding = ListPool<IReportStat>.Shared.Rent();
            MainPlugin.Reporter = new Reporter(MainPlugin.Singleton.Config.DiscordWebhook);
        }

        public void OnRestarting()
        {
            foreach (var stat in Holding)
            {
                MainPlugin.Reporter.SetStat(stat);
            }
            if (MainPlugin.Reporter is not null && !MainPlugin.Reporter.HasSent)
            {
                MainPlugin.Reporter.SendReport();
            }
            ListPool<IReportStat>.Shared.Return(Holding);
        }

        public void OnRoundStarted()
        {
            Timing.CallDelayed(.5f, () =>
            {
                StartingStats stat = new()
                {
                    ClassDPersonnel = Player.Get(RoleType.ClassD).Count(),
                    SCPs = Player.Get(Team.SCP).Count(),
                    FacilityGuards = Player.Get(RoleType.FacilityGuard).Count(),
                    Scientists = Player.Get(RoleType.Scientist).Count(),
                    StartTime = DateTime.Now.ToString("MMMM dd, yyyy hh:mm:ss tt"),
                    PlayersAtStart = Player.List.Where(r => !r.IsDead).Count(),
                };
                MainPlugin.Reporter.SetStat(stat);
            });
        }
    }
}
