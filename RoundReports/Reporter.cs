using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Exiled.API.Features;
using NorthwoodLib.Pools;
using System.ComponentModel;
using MEC;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace RoundReports
{
    public class Reporter
    {
        private List<IReportStat> Stats { get; set; }
        private string _webhook;
        public bool HasSent { get; private set; } = false;

        public Reporter(string webhookUrl)
        {
            _webhook = webhookUrl;
            Stats = ListPool<IReportStat>.Shared.Rent();
        }

        public T GetStat<T>()
            where T: class, IReportStat
        {
            return Stats.FirstOrDefault(r => r is T) as T;
        }

        public void SetStat(IReportStat stat)
        {
            Stats.RemoveAll(r => r.GetType() == stat.GetType());
            Stats.Add(stat);
        }

        public PasteEntry BuildReport()
        {
            var entry = new PasteEntry() { description = $"{MainPlugin.Singleton.Config.ServerName} | {DateTime.Now.ToString("MMMM dd, yyyy hh:mm:ss tt")}", sections = new(1) };
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IReportStat))))
            {
                if (!Stats.Any(r => r.GetType() == type))
                {
                    try
                    {
                        Stats.Add((IReportStat)Activator.CreateInstance(type));
                    }
                    catch (Exception ex)
                    {
                        Log.Warn(ex);
                    }
                }
            }
            // Stats
            var stats = Stats.OrderBy(stat => stat.Order);
            foreach (var stat in stats)
            {
                try
                {
                    var section = new PasteSection()
                    {
                        name = stat.Title,
                        syntax = "text",
                        contents = "No Data"
                    };
                    Type type = stat.GetType();
                    StringBuilder bldr = StringBuilderPool.Shared.Rent();
                    foreach (PropertyInfo pinfo in type.GetProperties())
                    {
                        if (pinfo.Name is "Title" or "Order") continue;
                        var attr = pinfo.GetCustomAttribute<DescriptionAttribute>();
                        if (attr is null)
                        {
                            bldr.AppendLine($"{SplitString(pinfo.Name)}: {pinfo.GetValue(stat)}");
                        }
                        else
                        {
                            bldr.AppendLine($"{attr.Description}: {pinfo.GetValue(stat)}");
                        }
                    }
                    section.contents = bldr.ToString();
                    if (string.IsNullOrEmpty(section.contents)) section.contents = "No Data";
                    StringBuilderPool.Shared.Return(bldr);
                    entry.sections.Add(section);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                }
            }

            ListPool<IReportStat>.Shared.Return(Stats);

            // Conclude
            return entry;
        }

        public void SendReport()
        {
            HasSent = true;
            Timing.RunCoroutine(_SendReport());
        }

        private IEnumerator<float> _SendReport()
        {
            var key = MainPlugin.Singleton.Config.PasteKey;
            PasteEntry data = BuildReport();
            Log.Info(JsonConvert.SerializeObject(data));
            var pasteWWW = UnityWebRequest.Put("https://api.paste.ee/v1/pastes", JsonConvert.SerializeObject(data));
            pasteWWW.method = "POST";
            pasteWWW.SetRequestHeader("Content-Type", "application/json");
            pasteWWW.SetRequestHeader("X-Auth-Token", key);
            yield return Timing.WaitUntilDone(pasteWWW.SendWebRequest());
            if (!pasteWWW.isHttpError && !pasteWWW.isNetworkError)
            {
                Log.Info("Report posted! " + pasteWWW.downloadHandler.text);
            }
            else
            {
                Log.Warn($"Report failed to post! {pasteWWW.error}");
            }
        }

        private static string SplitString(string s)
        {
            var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
            return r.Replace(s, " ");
        }
    }
}
