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

namespace RoundReports
{
    public class EventHandlers
    {
        public List<IReportStat> Holding { get; set; }

        public void Hold<T>(T stat)
            where T: class, IReportStat
        {
            Holding.RemoveAll(r => r.GetType() == typeof(T));
            Holding.Add(stat);
        }

        public bool TryGetStat<T>(out T value)
            where T: class, IReportStat
        {
            value = Holding.FirstOrDefault(r => r.GetType() == typeof(T)) as T;
            return value != null;
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
                Hold(stat);
            });
        }

        public void OnUsedItem(UsedItemEventArgs ev)
        {
            if (!TryGetStat<MedicalStats>(out MedicalStats stats))
            {
                stats = new();
            }
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
}
