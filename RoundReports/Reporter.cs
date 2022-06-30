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
using System.Collections;
using Exiled.API.Enums;

namespace RoundReports
{
    public class Reporter
    {
        private List<IReportStat> Stats { get; set; }
        public static Dictionary<Player, string> NameStore { get; set; }
        private string _webhook;
        public bool HasSent { get; private set; } = false;
        public LeadingTeam LeadingTeam { get; set; }
        public const string DoNotTrackText = "[DO NOT TRACK USER]";

        public Reporter(string webhookUrl)
        {
            _webhook = webhookUrl;
            Stats = ListPool<IReportStat>.Shared.Rent();
            NameStore = new(0);
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

        public void ReturnLists()
        {
            ListPool<IReportStat>.Shared.Return(Stats);
            NameStore.Clear();
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
                            bldr.AppendLine($"{SplitString(pinfo.Name)}: {GetDisplay(pinfo.GetValue(stat))}");
                        }
                        else
                        {
                            object val = pinfo.GetValue(stat);
                            bldr.AppendLine($"{attr.Description}: {GetDisplay(val)}");
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

            // Conclude
            ReturnLists();
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
            var pasteWWW = UnityWebRequest.Put("https://api.paste.ee/v1/pastes", JsonConvert.SerializeObject(data));
            pasteWWW.method = "POST";
            pasteWWW.SetRequestHeader("Content-Type", "application/json");
            pasteWWW.SetRequestHeader("X-Auth-Token", key);
            yield return Timing.WaitUntilDone(pasteWWW.SendWebRequest());
            if (!pasteWWW.isHttpError && !pasteWWW.isNetworkError)
            {
                PasteResponse response;
                try
                {
                    response = JsonConvert.DeserializeObject<PasteResponse>(pasteWWW.downloadHandler.text);
                }
                catch (Exception e)
                {
                    Log.Warn($"Report failed to post! {e}");
                    yield break;
                }
                if (!response.success)
                {
                    Log.Warn("Unknown error when uploading the report.");
                }
                else
                {
                    Log.Info($"Report uploaded successfully! Access it here: {response.link}");
                    if (!string.IsNullOrEmpty(MainPlugin.Singleton.Config.DiscordWebhook))
                    {
                        DiscordHook hookData = new()
                        {
                            Username = "Round Report",
                            Embeds = new()
                            {
                                new()
                                {
                                    Title = "Round Report",
                                    TimeStamp = DateTime.Now,
                                    Color = LeadingTeam switch
                                    {
                                        LeadingTeam.Anomalies => 16711680,
                                        LeadingTeam.FacilityForces => 38143,
                                        LeadingTeam.ChaosInsurgency => 26916,
                                        LeadingTeam.Draw => 10197915,
                                        _ => 10197915,
                                    },
                                    Description = response.link,
                                    Fields = new()
                                    {
                                        new()
                                        {
                                            Name = "Post Date",
                                            Value = $"<t:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:D>",
                                            Inline = true
                                        },
                                        new()
                                        {
                                            Name = "Expire Date",
                                            Value = $"<t:{DateTimeOffset.UtcNow.AddDays(28).ToUnixTimeSeconds()}:D>",
                                            Inline = true,
                                        }
                                    },
                                }
                            },
                        };

                        var discordWWW = UnityWebRequest.Put(MainPlugin.Singleton.Config.DiscordWebhook, JsonConvert.SerializeObject(hookData));
                        discordWWW.method = "POST";
                        discordWWW.SetRequestHeader("Content-Type", "application/json");
                        yield return Timing.WaitUntilDone(discordWWW.SendWebRequest());
                        if (discordWWW.isHttpError || discordWWW.isNetworkError)
                        {
                            Log.Warn($"Error when attempting to send report to discord log: {discordWWW.error}");
                        }
                    }
                }
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

        private static string GetDisplay(object val)
        {
            if (val is null)
                return "No Data";
            if (val is Player plr)
            {
                if (!plr.IsConnected)
                {
                    if (NameStore.ContainsKey(plr))
                    {
                        return $"[DC] {NameStore[plr]}";
                    }
                    else
                    {
                        return "[DC] Unknown";
                    }
                }
                if (plr.DoNotTrack)
                {
                    return DoNotTrackText;
                }
                else
                {
                    return plr.Nickname;
                }
            }
            else if (val is DateTime dt)
            {
                if (dt == DateTime.MinValue)
                    return "No Data";
                return dt.ToString("MMMM dd, yyyy hh:mm:ss tt");
            }
            else if (val is TimeSpan ts)
            {
                if (ts == TimeSpan.Zero)
                    return "No Data";
                return $"{ts.Minutes}m{ts.Seconds}s";
            }
            else if (val is IDictionary dict)
            {
                if (dict.Count == 0)
                {
                    return "[None]";
                }
                StringBuilder bldr2 = StringBuilderPool.Shared.Rent();
                bldr2.AppendLine();
                foreach (DictionaryEntry item in dict)
                {
                    if (item.Key is null || item.Value is null) continue;
                    bldr2.AppendLine("- " + GetDisplay(item.Key) + ": " + GetDisplay(item.Value));
                }
                var display = bldr2.ToString().TrimEnd(' ', '\r', '\n');
                StringBuilderPool.Shared.Return(bldr2);
                return display;
            }
            else if (val is IEnumerable list && val.GetType().IsGenericType)
            {
                // Check for zero results
                int i = 0;
                foreach (var item in list) i++;
                if (i == 0)
                {
                    return "[None]";
                }

                StringBuilder bldr2 = StringBuilderPool.Shared.Rent();
                bldr2.AppendLine();
                foreach (var item in list)
                {
                    if (item is null) continue;
                    bldr2.AppendLine("- " + GetDisplay(item));
                }
                var display = bldr2.ToString().TrimEnd(' ', '\r', '\n');
                StringBuilderPool.Shared.Return(bldr2);
                return display;
            }
            else
            {
                return val.ToString();
            }
        }
    }
}
