using Exiled.API.Features;
using Newtonsoft.Json;
using QuickChart;
using System.Linq;

namespace RoundReports
{
    public class ChartGen
    {
        public Reporter Reporter { get; }
        public Chart Chart { get; }
        public GraphSettings Settings { get; }

        public ChartGen(Reporter reporter)
        {
            Chart = new();
            Settings = new() { Data = new() { DataSets = new()  } };
            Reporter = reporter;
        }

        public string GetUrl()
        {
            Chart.Config = JsonConvert.SerializeObject(Settings);
            return Chart.GetUrl();
        }

        public void Fill(string type)
        {
            if (type is "kills")
            {
                OrganizedKillsStats stats = Reporter.GetStat<OrganizedKillsStats>();
                Settings.Type = "bar";
                Settings.Data.Labels = stats.KillsByPlayer.Keys.Select(player => Reporter.GetDisplay(player)).ToArray();
                foreach (var data in stats.KillsByPlayer)
                {
                    Settings.Data.DataSets.Add(new() { Label = Reporter.GetDisplay(data.Key), Data = new[] { (float)data.Value } });
                }
            }
        }


    }
}
