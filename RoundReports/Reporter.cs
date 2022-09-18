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

using EBroadcast = Exiled.API.Features.Broadcast;

namespace RoundReports
{
    public class Reporter
    {
        private List<IReportStat> Stats { get; set; }
        public static Dictionary<Player, string> NameStore { get; set; }
        public bool HasSent { get; private set; } = false;
        public LeadingTeam WinTeam { get; set; } = LeadingTeam.Draw;
        private List<string> Remarks { get; set; }
        public static string DoNotTrackText => MainPlugin.Translations.DoNotTrack;

        public Reporter()
        {
            Stats = ListPool<IReportStat>.Shared.Rent();
            Remarks = ListPool<string>.Shared.Rent();
            NameStore = new(0);
        }

        public static object GetDefault(Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
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
            ListPool<string>.Shared.Return(Remarks);
            NameStore.Clear();
        }

        public void Kill()
        {
            ReturnLists();
            MainPlugin.Reporter = null;
        }

        public void AddRemark(string remark)
        {
            Remarks.Add($"[{DateTime.Now.ToString("HH:mm:ss")}] {remark}");
        }

        public static long StringLengthToLong(string length) => length.ToLower() switch
        {
            "1d" => 86400,
            "7d" => 604800,
            "14d" => 1211100,
            "1m" => 2631000,
            "3m" => 7890000,
            "6m" => 15770000,
            "never" => 0,
            _ => StringLengthToLong("1M"),
        };

        public PasteEntry BuildReport()
        {
            var entry = new PasteEntry() { description = $"{MainPlugin.Singleton.Config.ServerName} | {DateTime.Now.ToString("MMMM dd, yyyy hh:mm:ss tt")}", sections = new(1) };
            entry.expiration = StringLengthToLong(MainPlugin.Singleton.Config.ExpiryTime);
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
            // Remarks
            if (Remarks.Count > 0)
            {
                var section = new PasteSection()
                {
                    name = MainPlugin.Singleton.Translation.RoundRemarks,
                    syntax = "text",
                    contents = string.Empty,

                };
                StringBuilder bldr = StringBuilderPool.Shared.Rent();
                foreach (var remark in Remarks)
                    bldr.AppendLine(remark);
                section.contents = bldr.ToString();
                entry.sections.Add(section);
                StringBuilderPool.Shared.Return(bldr);
            }
            // Stats
            var stats = Stats.OrderBy(stat => stat.Order);
            foreach (var stat in stats)
            {
                try
                {
                    Type type = stat.GetType();
                    string sectionTitle = stat.Title;
                    var titleAttr = type.GetProperty("Title").GetCustomAttribute<TranslationAttribute>();
                    if (titleAttr is not null)
                    {
                        PropertyInfo propInfo = typeof(Translation).GetProperty(titleAttr.KeyName);
                        string val = propInfo.GetValue(MainPlugin.Translations).ToString();
                        sectionTitle = val;
                    }
                    var section = new PasteSection()
                    {
                        name = sectionTitle,
                        syntax = "text",
                        contents = MainPlugin.Singleton.Translation.NoData,
                    };
                    StringBuilder bldr = StringBuilderPool.Shared.Rent();
                    foreach (PropertyInfo pinfo in type.GetProperties())
                    {
                        if (pinfo.Name is "Title" or "Order") continue;
                        var hideAttr = pinfo.GetCustomAttribute<HideIfDefaultAttribute>();
                        var propertyValue = pinfo.GetValue(stat);
                        if (hideAttr is not null && object.Equals(propertyValue, GetDefault(pinfo.PropertyType)))
                        {
                            continue;
                        }
                        var headerAttribute = pinfo.GetCustomAttribute<HeaderAttribute>();
                        if (headerAttribute is not null)
                        {
                            bldr.AppendLine($"\n====== {headerAttribute.Header} ======");
                        }
                        var ruleAttr = pinfo.GetCustomAttribute<RuleAttribute>();
                        var attr = pinfo.GetCustomAttribute<TranslationAttribute>();
                        if (attr is null)
                        {
                            bldr.AppendLine($"{SplitString(pinfo.Name)}: {GetDisplay(pinfo.GetValue(stat), ruleAttr?.Rule ?? Rule.None)}");
                        }
                        else
                        {
                            object val = pinfo.GetValue(stat);
                            bldr.AppendLine($"{attr.Text}: {GetDisplay(val, ruleAttr?.Rule ?? Rule.None)}");
                        }
                    }
                    section.contents = StringBuilderPool.Shared.ToStringReturn(bldr).Trim();
                    if (string.IsNullOrEmpty(section.contents)) section.contents = MainPlugin.Singleton.Translation.NoData;
                    entry.sections.Add(section);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                }
            }

            // Conclude
            Kill();
            return entry;
        }

        public void SendReport()
        {
            if (HasSent)
                return;

            HasSent = true;
            SendReportInternal();
        }

        private void SendReportInternal()
        {
            Timing.CallDelayed(0.1f, () =>
            {
                PasteEntry data = BuildReport();
                Timing.RunCoroutine(TryUpload(data, 0));
            });
        }

        private IEnumerator<float> TryUpload(PasteEntry data, int iter)
        {
            if (iter == 10)
            {
                Log.Warn("Failed to post round report to Pastee ten times. Request discarded.");
                yield break;
            }
            var pasteWWW = UnityWebRequest.Put("https://api.paste.ee/v1/pastes", JsonConvert.SerializeObject(data));
            pasteWWW.method = "POST";
            pasteWWW.SetRequestHeader("Content-Type", "application/json");
            pasteWWW.SetRequestHeader("X-Auth-Token", MainPlugin.Singleton.Config.PasteKey);
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
                    Log.Warn($"Report response could not be read. Retrying upload. Error: {e}");
                    Timing.RunCoroutine(TryUpload(data, iter + 1));
                    yield break;
                }
                if (response is null || !response.success)
                {
                    Log.Warn("Unknown error when uploading the report. Retrying upload.");
                    Timing.RunCoroutine(TryUpload(data, iter + 1));
                    yield break;
                }
                else
                {
                    if (MainPlugin.Singleton.Config.SendInConsole)
                        Log.Info($"Report uploaded successfully! Access it here: {response.link}");

                    if (!string.IsNullOrEmpty(MainPlugin.Singleton.Config.DiscordWebhook))
                    {
                        string winText = MainPlugin.Translations.WinText;
                        winText = WinTeam switch
                        {
                            LeadingTeam.Anomalies => winText.Replace("{TEAM}", MainPlugin.Translations.ScpTeam),
                            LeadingTeam.ChaosInsurgency => winText.Replace("{TEAM}", MainPlugin.Translations.InsurgencyTeam),
                            LeadingTeam.FacilityForces => winText.Replace("{TEAM}", MainPlugin.Translations.MtfTeam),
                            LeadingTeam.Draw => MainPlugin.Translations.Stalemate,
                            _ => winText.Replace("{TEAM}", MainPlugin.Translations.Unknown),
                        };
                        DiscordHook hookData = new()
                        {
                            Username = MainPlugin.Singleton.Translation.RoundReport,
                            Embeds = new()
                            {
                                new()
                                {
                                    Title = MainPlugin.Singleton.Translation.RoundReport,
                                    TimeStamp = DateTime.Now,
                                    Color = WinTeam switch
                                    {
                                        LeadingTeam.Anomalies => 16711680,
                                        LeadingTeam.FacilityForces => 38143,
                                        LeadingTeam.ChaosInsurgency => 26916,
                                        LeadingTeam.Draw => 10197915,
                                        _ => 10197915,
                                    },
                                    Description = winText + "\n" + response.link,
                                    Fields = new()
                                    {
                                        new()
                                        {
                                            Name = MainPlugin.Singleton.Translation.PostDate,
                                            Value = $"<t:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:D>",
                                            Inline = true
                                        },
                                        new()
                                        {
                                            Name = MainPlugin.Singleton.Translation.ExpireDate,
                                            Value = StringLengthToLong(MainPlugin.Singleton.Config.ExpiryTime) == 0 ? MainPlugin.Translations.Never : $"<t:{DateTimeOffset.UtcNow.AddSeconds(StringLengthToLong(MainPlugin.Singleton.Config.ExpiryTime)).ToUnixTimeSeconds()}:D>",
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
                            Log.Warn($"Error when attempting to send report to discord log: {discordWWW.error}");
                        else
                            if (MainPlugin.Singleton.Config.SendInConsole)
                                Log.Info("Report sent to Discord successfully.");
                    }

                    // Broadcast
                    EBroadcast br = MainPlugin.Singleton.Config.EndingBroadcast;
                    if (br is not null && br.Show == true && Server.Broadcast is not null)
                    {
                        Map.Broadcast(br, true);
                    }
                }
            }
            else
            {
                Log.Warn($"Report failed to post to pastee, retrying. Error: {pasteWWW.error}");
                Timing.RunCoroutine(TryUpload(data, iter + 1));
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

        public static string GetDisplay(object val, Rule rules = Rule.None)
        {
            if (val is null)
                return MainPlugin.Translations.NoData;
            if (val is bool b)
                return b ? MainPlugin.Translations.Yes : MainPlugin.Translations.No;
            if (val is Player plr)
            {
                if (!plr.IsConnected)
                    return NameStore.ContainsKey(plr) ? $"[DC] {NameStore[plr]}" : $"[DC] {MainPlugin.Translations.Unknown}";
                return plr.DoNotTrack ? DoNotTrackText : plr.Nickname;
            }
            else if (val is DateTime dt)
                return dt == DateTime.MinValue ? MainPlugin.Translations.NoData : dt.ToString(MainPlugin.Singleton.Config.TimeFormat);
            else if (val is TimeSpan ts)
                return ts == TimeSpan.Zero ? MainPlugin.Translations.NoData : $"{ts.Minutes}m{ts.Seconds}s";
            else if (val is IDictionary dict)
            {
                if (dict.Count == 0)
                    return MainPlugin.Translations.NoData;

                List<DictionaryEntry> internalList = new();

                StringBuilder bldr2 = StringBuilderPool.Shared.Rent();
                bldr2.AppendLine();

                foreach (DictionaryEntry item in dict)
                {
                    internalList.Add(item);
                }

                // Alphabetical rule
                if (rules.HasFlag(Rule.Alphabetical))
                    internalList = internalList.OrderBy(p => p.Key).ToList();

                foreach (DictionaryEntry item in internalList)
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
                    return MainPlugin.Translations.NoData;

                // Hacky solution: Convert IEnumerable to a List<object> to sort it
                List<object> internalList = new();
                StringBuilder bldr2 = StringBuilderPool.Shared.Rent();

                if (!rules.HasFlag(Rule.CommaSeparatedList))
                    bldr2.AppendLine();

                foreach (var item in list)
                    internalList.Add(item);

                // Alphabetical rule
                if (rules.HasFlag(Rule.Alphabetical))
                    internalList.Sort();

                foreach (var item in internalList)
                {
                    if (item is null) continue;
                    if (rules.HasFlag(Rule.CommaSeparatedList))
                        bldr2.Append(GetDisplay(item) + ", ");
                    else
                        bldr2.AppendLine("- " + GetDisplay(item));
                }
                var display = bldr2.ToString().TrimEnd(' ', '\r', '\n', ',');
                StringBuilderPool.Shared.Return(bldr2);
                return display;
            }
            else
                return val.ToString();
        }
    }
}
