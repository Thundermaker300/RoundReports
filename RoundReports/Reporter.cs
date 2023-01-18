using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Exiled.API.Features;
using NorthwoodLib.Pools;
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
        public Guid UniqueId { get; }
        public int UptimeRound { get; }
        private List<IReportStat> Stats { get; set; }
        public static Dictionary<Player, string> NameStore { get; set; }
        public bool HasSent { get; private set; }
        public LeadingTeam WinTeam { get; set; } = LeadingTeam.Draw;
        private List<string> Remarks { get; set; }
        private PasteEntry ReportData;
        public static string DoNotTrackText => MainPlugin.Translations.DoNotTrack;
        public bool AtLeastOneHidden { get; set; } = false;

        public Reporter()
        {
            UniqueId = Guid.NewGuid();
            UptimeRound = Round.UptimeRounds;
            Stats = ListPool<IReportStat>.Shared.Rent();
            Remarks = ListPool<string>.Shared.Rent();
            NameStore = new(0);
            HasSent = false;
            Log.Debug("New reporter has been instantiated.");
        }

        /// <summary>
        /// Gets the default value of a type.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>Default value.</returns>
        public static object GetDefault(Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
        }

        /// <summary>
        /// Obtains a stat, setting it up if it doesn't already exist.
        /// </summary>
        /// <typeparam name="T">Stat to obtain.</typeparam>
        /// <returns>Stat.</returns>
        public T GetStat<T>()
            where T: class, IReportStat, new()
        {
            if (Stats.FirstOrDefault(r => r is T) is not T stat)
            {
                stat = new T();
                stat.Setup();
            }
            return stat;
        }

        /// <summary>
        /// Sets a stat to be shown in the report.
        /// </summary>
        /// <param name="stat">The stat to store.</param>
        public void SetStat(IReportStat stat)
        {
            Log.Debug($"Setting stat: {stat.GetType().Name}");
            Stats.RemoveAll(r => r.GetType() == stat.GetType());
            Stats.Add(stat);
        }

        /// <summary>
        /// Clears and returns all used lists to the NW List pool.
        /// </summary>
        public void ReturnLists()
        {
            Log.Debug("Returning lists to pool...");
            ListPool<IReportStat>.Shared.Return(Stats);
            ListPool<string>.Shared.Return(Remarks);
            NameStore.Clear();
        }

        /// <summary>
        /// Shuts down the reporter by cleaning up all of the round stats.
        /// </summary>
        public void Kill()
        {
            Log.Debug("Killing reporter...");

            foreach (IReportStat stat in Stats)
                stat.Cleanup();

            ReturnLists();
        }

        /// <summary>
        /// Adds a round remark to be shown in the "remarks" section of the report.
        /// </summary>
        /// <param name="remark">The remark to add.</param>
        public void AddRemark(string remark)
        {
            Log.Debug($"Adding remark: {remark}");
            Remarks.Insert(0, $"[{GetDisplay(Round.ElapsedTime)}] {remark}");
        }

        /// <summary>
        /// Converts a string length (eg. "1D") to its respective time in seconds.
        /// </summary>
        /// <param name="length">The string length of time.</param>
        /// <returns>The amount of seconds.</returns>
        public static long StringLengthToLong(string length) => length.ToLower() switch
        {
            "1h" => 3600,
            "12h" => StringLengthToLong("1H") * 12,
            "1d" => StringLengthToLong("12H") * 2,
            "3d" => StringLengthToLong("1D") * 3,
            "7d" => StringLengthToLong("1D") * 7,
            "14d" => StringLengthToLong("7D") * 2,
            "1m" => StringLengthToLong("1D") * 30,
            "2m" => StringLengthToLong("1M") * 2,
            "3m" => StringLengthToLong("1M") * 3,
            "6m" => StringLengthToLong("3M") * 2,
            "1y" => StringLengthToLong("6M") * 2,
            "2y" => StringLengthToLong("1Y") * 2,
            "never" => 0,
            _ => StringLengthToLong("1M"),
        };

        /// <summary>
        /// Creates the round report. Uses all statistics stored in <see cref="Stats"/>.
        /// </summary>
        /// <returns>The report to be sent, in the form of a <see cref="PasteEntry"/>.</returns>
        public PasteEntry BuildReport()
        {
            Log.Debug("Building report...");
            PasteEntry entry = new PasteEntry() { Description = $"{MainPlugin.Singleton.Config.ServerName} | {DateTime.Now.ToString("MMMM dd, yyyy hh:mm:ss tt")}", Sections = new(1) };
            entry.Expiration = StringLengthToLong(MainPlugin.Singleton.Config.ExpiryTime);
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IReportStat))))
            {
                if (!Stats.Any(r => r.GetType() == type))
                {
                    try
                    {
                        IReportStat newStat = (IReportStat)Activator.CreateInstance(type);
                        newStat.Setup();
                        Stats.Add(newStat);
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
                Log.Debug($"{Remarks.Count} remarks saved.");
                PasteSection section = new PasteSection()
                {
                    Name = MainPlugin.Singleton.Translation.RoundRemarks,
                    Syntax = "text",
                    Contents = string.Empty,

                };
                Log.Debug($"Adding new section: REMARKS");
                StringBuilder bldr = StringBuilderPool.Shared.Rent();
                if (AtLeastOneHidden)
                    bldr.AppendLine(MainPlugin.Translations.HiddenUsersNotice);

                foreach (string remark in Remarks)
                    bldr.AppendLine(remark);
                section.Contents = bldr.ToString();
                entry.Sections.Add(section);
                StringBuilderPool.Shared.Return(bldr);
            }
            // Stats
            IOrderedEnumerable<IReportStat> stats = Stats.OrderBy(stat => stat.Order);
            foreach (IReportStat stat in stats)
            {
                try
                {
                    Type type = stat.GetType();
                    string sectionTitle = stat.Title;
                    TranslationAttribute titleAttr = type.GetProperty("Title").GetCustomAttribute<TranslationAttribute>();
                    if (titleAttr is not null)
                    {
                        PropertyInfo propInfo = typeof(Translation).GetProperty(titleAttr.KeyName);
                        string val = propInfo.GetValue(MainPlugin.Translations).ToString();
                        sectionTitle = val;
                    }
                    Log.Debug($"Adding new section: {sectionTitle}");
                    PasteSection section = new PasteSection()
                    {
                        Name = sectionTitle,
                        Syntax = "text",
                        Contents = MainPlugin.Singleton.Translation.NoData,
                    };
                    StringBuilder bldr = StringBuilderPool.Shared.Rent();
                    foreach (PropertyInfo pinfo in type.GetProperties())
                    {
                        if (pinfo.Name is "Title" or "Order") continue;

                        // Show headers
                        HeaderAttribute headerAttribute = pinfo.GetCustomAttribute<HeaderAttribute>();
                        if (headerAttribute is not null)
                        {
                            Log.Debug($"Adding header: {headerAttribute.Header}");
                            bldr.AppendLine($"\n====== {headerAttribute.Header} ======");
                        }

                        // Hide if default value
                        HideIfDefaultAttribute hideAttr = pinfo.GetCustomAttribute<HideIfDefaultAttribute>();
                        object propertyValue = pinfo.GetValue(stat);
                        if (hideAttr is not null && object.Equals(propertyValue, GetDefault(pinfo.PropertyType)))
                        {
                            continue;
                        }

                        // Hide if server disabled
                        BindStatAttribute bindAttribute = pinfo.GetCustomAttribute<BindStatAttribute>();
                        if (bindAttribute is not null && !MainPlugin.Check(bindAttribute.Type))
                        {
                            continue;
                        }

                        // Rules & Translation
                        RuleAttribute ruleAttr = pinfo.GetCustomAttribute<RuleAttribute>();
                        TranslationAttribute attr = pinfo.GetCustomAttribute<TranslationAttribute>();
                        Log.Debug($"Adding stat {pinfo.Name} with value of {propertyValue}");
                        if (attr is null)
                        {
                            bldr.AppendLine($"{SplitString(pinfo.Name)}: {GetDisplay(propertyValue, pinfo.PropertyType, ruleAttr?.Rule ?? Rule.None)}");
                        }
                        else
                        {
                            bldr.AppendLine($"{attr.Text}: {GetDisplay(propertyValue, pinfo.PropertyType, ruleAttr?.Rule ?? Rule.None)}");
                        }
                    }
                    section.Contents = StringBuilderPool.Shared.ToStringReturn(bldr).Trim();
                    if (string.IsNullOrEmpty(section.Contents)) continue;
                    entry.Sections.Add(section);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                }
            }

            // Conclude
            Log.Debug("Report has been built.");
            return entry;
        }

        /// <summary>
        /// Begins the coroutine to build and send the report.
        /// </summary>
        public void SendReport()
        {
            if (HasSent)
                return;

            Log.Debug("Report upload request received, step: 2.");

            HasSent = true;
            Timing.RunCoroutine(SendReportInternal());
        }

        /// <summary>
        /// Builds the report, and then sends it.
        /// </summary>
        /// <returns>Coroutine.</returns>
        private IEnumerator<float> SendReportInternal()
        {
            Log.Debug("Report upload request received, step: 3.");
            yield return Timing.WaitForSeconds(0.5f);
            ReportData = BuildReport();

            Log.Debug("Report upload request received, step: 4.");
            Timing.RunCoroutine(TryUpload());
        }

        /// <summary>
        /// Huge method for handling the upload to Pastee and Discord, as well as the in-game broadcasts.
        /// </summary>
        /// <param name="iter">The current iteration of the method. Upload will be canceled if this reaches 10.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator<float> TryUpload(int iter = 0)
        {
            PasteEntry data = ReportData;
            Log.Debug("Beginning report upload process.");
            if (iter >= 10)
            {
                Log.Warn("Failed to post round report to Pastee ten times. Request discarded.");
                Timing.CallDelayed(.25f, Kill);
                yield break;
            }

            UnityWebRequest pasteWWW = UnityWebRequest.Put("https://api.paste.ee/v1/pastes", JsonConvert.SerializeObject(data));
            pasteWWW.method = "POST";
            pasteWWW.SetRequestHeader("Content-Type", "application/json");
            pasteWWW.SetRequestHeader("X-Auth-Token", MainPlugin.Singleton.Config.PasteKey);
            Log.Debug("Sending report to Pastee.");
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
                    Timing.RunCoroutine(TryUpload(iter + 1));
                    yield break;
                }
                if (response is null || !response.Success)
                {
                    Log.Warn("Unknown error when uploading the report. Retrying upload.");
                    Timing.RunCoroutine(TryUpload(iter + 1));
                    yield break;
                }
                else
                {
                    if (MainPlugin.Singleton.Config.SendInConsole)
                        Log.Info($"Report uploaded successfully! Access it here: {response.Link}");

                    if (!string.IsNullOrEmpty(MainPlugin.Singleton.Config.DiscordWebhook))
                    {
                        Log.Debug("Sending report to Discord.");
                        string winText = MainPlugin.Translations.WinText;
                        winText = WinTeam switch
                        {
                            LeadingTeam.Anomalies => winText.Replace("{TEAM}", MainPlugin.Translations.ScpTeam),
                            LeadingTeam.ChaosInsurgency => winText.Replace("{TEAM}", MainPlugin.Translations.InsurgencyTeam),
                            LeadingTeam.FacilityForces => winText.Replace("{TEAM}", MainPlugin.Translations.MtfTeam),
                            LeadingTeam.Draw => MainPlugin.Translations.Stalemate,
                            _ => winText.Replace("{TEAM}", MainPlugin.Translations.Unknown),
                        };
                        Log.Debug("Building webhook information.");
                        DiscordHook hookData = new()
                        {
                            Username = MainPlugin.Singleton.Translation.RoundReport,
                            Embeds = new()
                            {
                                new()
                                {
                                    Title = MainPlugin.Singleton.Translation.RoundReport,
                                    Url = "https://github.com/Thundermaker300/RoundReports",
                                    TimeStamp = DateTime.Now,
                                    Color = WinTeam switch
                                    {
                                        LeadingTeam.Anomalies => 16711680,
                                        LeadingTeam.FacilityForces => 38143,
                                        LeadingTeam.ChaosInsurgency => 26916,
                                        LeadingTeam.Draw => 10197915,
                                        _ => 10197915,
                                    },
                                    Description = winText + "\n" + $"[{MainPlugin.Translations.ViewReport}]({response.Link})",
                                    Fields = new()
                                    {
                                        new()
                                        {
                                            Name = MainPlugin.Translations.PostDate,
                                            Value = $"<t:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:D>",
                                            Inline = true
                                        },
                                        new()
                                        {
                                            Name = MainPlugin.Translations.ExpireDate,
                                            Value = StringLengthToLong(MainPlugin.Singleton.Config.ExpiryTime) == 0 ? MainPlugin.Translations.Never : $"<t:{DateTimeOffset.UtcNow.AddSeconds(StringLengthToLong(MainPlugin.Singleton.Config.ExpiryTime)).ToUnixTimeSeconds()}:D>",
                                            Inline = true,
                                        }
                                    },
                                    Footer = new()
                                    {
                                        Text = ProcessReportArgs(MainPlugin.Singleton.Config.FooterText),
                                    }
                                }
                            },
                        };

                        UnityWebRequest discordWWW = UnityWebRequest.Put(MainPlugin.Singleton.Config.DiscordWebhook, JsonConvert.SerializeObject(hookData));
                        discordWWW.method = "POST";
                        discordWWW.SetRequestHeader("Content-Type", "application/json");
                        yield return Timing.WaitUntilDone(discordWWW.SendWebRequest());

                        if (discordWWW.isHttpError || discordWWW.isNetworkError)
                            Log.Warn($"Error when attempting to send report to discord log: {discordWWW.error}");
                        else
                        {
                            if (MainPlugin.Singleton.Config.SendInConsole)
                                Log.Info("Report sent to Discord successfully.");
                        }
                    }

                    // Broadcast
                    Log.Debug("Sending broadcasts.");
                    try
                    {
                        List<EBroadcast> brList = MainPlugin.Singleton.Config.EndingBroadcasts;
                        if (brList is not null && brList.Count > 0 && !MainPlugin.IsRestarting)
                        {
                            if (brList.Any(br => br.Show))
                                Map.ClearBroadcasts();

                            foreach (EBroadcast br in brList)
                            {
                                br.Content = ProcessReportArgs(br.Content);
                                Log.Debug($"Queueing broadcast: {br.Content}");
                                Map.Broadcast(br);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Exception when showing round-end broadcasts: {e}");
                    }

                    Timing.CallDelayed(2f, Kill);
                }
            }
            else
            {
                Log.Warn($"Report failed to post to Pastee, retrying. Error: {pasteWWW.error}");
                Timing.RunCoroutine(TryUpload(iter + 1));
            }
        }

        private static string SplitString(string s)
        {
            Regex r = new(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
            return r.Replace(s, " ");
        }

        public static string GetDisplay(object val, Type expectedType = null, Rule rules = Rule.None)
        {
            if (val is null)
                if (expectedType is not null && expectedType.FullName is "Exiled.API.Features.Player")
                    return MainPlugin.Translations.Nobody;
                else
                    return MainPlugin.Translations.NoData;
            if (val is bool b)
                return b ? MainPlugin.Translations.Yes : MainPlugin.Translations.No;
            if (val is Player plr)
            {
                if (!plr.IsConnected)
                    return NameStore.ContainsKey(plr) ? $"[DC] {NameStore[plr]}" : $"[DC] {MainPlugin.Translations.Unknown}";
                return (plr.DoNotTrack || MainPlugin.Configs.IgnoredUsers.Contains(plr.UserId)) ? DoNotTrackText : plr.Nickname;
            }
            else if (val is DateTime dt)
                return dt == DateTime.MinValue ? MainPlugin.Translations.NoData : dt.ToString(MainPlugin.Singleton.Config.FullTimeFormat);
            else if (val is TimeSpan ts)
                return ts == TimeSpan.Zero ? MainPlugin.Translations.NoData : $"{ts.Minutes}m{ts.Seconds}s";
            else if (val is IDictionary dict)
            {
                if (dict.Count == 0)
                    return MainPlugin.Translations.NoData;

                List<DictionaryEntry> internalList = ListPool<DictionaryEntry>.Shared.Rent();

                StringBuilder bldr2 = StringBuilderPool.Shared.Rent();
                bldr2.AppendLine();

                foreach (DictionaryEntry item in dict)
                {
                    internalList.Add(item);
                }

                if (internalList.Count == 0)
                    return MainPlugin.Translations.NoData;

                // Alphabetical rule
                if (rules.HasFlag(Rule.Alphabetical))
                    internalList = internalList.OrderBy(p => p.Key).ToList();

                foreach (DictionaryEntry item in internalList)
                {
                    if (item.Key is null || item.Value is null) continue;
                    bldr2.AppendLine("- " + GetDisplay(item.Key) + ": " + GetDisplay(item.Value));
                }

                string display = StringBuilderPool.Shared.ToStringReturn(bldr2).TrimEnd(' ', '\r', '\n');
                ListPool<DictionaryEntry>.Shared.Return(internalList);

                return display;
            }
            else if (val is IEnumerable list && val.GetType().IsGenericType)
            {
                // Check for zero results
                int i = 0;
                foreach (object item in list) i++;

                if (i == 0)
                    return MainPlugin.Translations.NoData;

                // Hacky solution: Convert IEnumerable to a List<object> to sort it
                List<object> internalList = ListPool<object>.Shared.Rent();
                StringBuilder bldr2 = StringBuilderPool.Shared.Rent();

                if (!rules.HasFlag(Rule.CommaSeparatedList))
                    bldr2.AppendLine();

                foreach (object item in list)
                    internalList.Add(item);

                if (internalList.Count == 0)
                    return MainPlugin.Translations.NoData;

                // Alphabetical rule
                if (rules.HasFlag(Rule.Alphabetical))
                    internalList.Sort();

                foreach (object item in internalList)
                {
                    if (item is null) continue;
                    if (rules.HasFlag(Rule.CommaSeparatedList))
                        bldr2.Append(GetDisplay(item) + ", ");
                    else
                        bldr2.AppendLine("- " + GetDisplay(item));
                }

                string display = StringBuilderPool.Shared.ToStringReturn(bldr2).TrimEnd(' ', '\r', '\n', ',');
                ListPool<object>.Shared.Return(internalList);

                return display;
            }
            else
                return val.ToString();
        }

        // Todo: Slow AF
        private string ProcessReportArgs(string input)
        => input
            .Replace("{ID}", UniqueId.ToString())
            .Replace("{UPTIMEROUND}", UptimeRound.ToString())
            .Replace("{HUMANMVP}", GetStat<MVPStats>().HumanMVP)
            .Replace("{SCPMVP}", GetStat<MVPStats>().SCPMVP)
            .Replace("{TOTALKILLS}", GetStat<FinalStats>().TotalKills.ToString())
            .Replace("{SCPKILLS}", GetStat<FinalStats>().SCPKills.ToString())
            .Replace("{MTFKILLS}", GetStat<FinalStats>().MTFKills.ToString())
            .Replace("{DCLASSKILLS}", GetStat<FinalStats>().DClassKills.ToString())
            .Replace("{CHAOSKILLS}", GetStat<FinalStats>().ChaosKills.ToString())
            .Replace("{SCIENTISTKILLS}", GetStat<FinalStats>().TotalKills.ToString())
            .Replace("{HUMANKILLS}", (GetStat<FinalStats>().TotalKills - GetStat<FinalStats>().SCPKills).ToString())
            .Replace("{TOTALDEATHS}", GetStat<FinalStats>().TotalDeaths.ToString())
            .Replace("{TOTALDAMAGE}", GetStat<OrganizedDamageStats>().TotalDamage.ToString())
            .Replace("{WINNINGTEAM}", GetStat<FinalStats>().WinningTeam)
            .Replace("{ROUNDTIME}", GetDisplay(GetStat<FinalStats>().RoundTime))
            .Replace("{STARTTIME}", GetDisplay(GetStat<StartingStats>().StartTime))
            .Replace("{PLAYERCOUNT}", Player.List.Count().ToString())
            .Replace("{TOTALDROPS}", GetStat<ItemStats>().TotalDrops.ToString())
            .Replace("{KEYCARDSCANS}", GetStat<ItemStats>().KeycardScans.ToString())
            .Replace("{TOTALRESPAWNED}", GetStat<RespawnStats>().TotalRespawned.ToString())
            .Replace("{SPAWNWAVES}", GetStat<RespawnStats>().SpawnWaves.Count.ToString())
            .Replace("{TOTALSHOTSFIRED}", GetStat<ItemStats>().TotalShotsFired.ToString())
            .Replace("{TOTALRELOADS}", GetStat<ItemStats>().TotalReloads.ToString())
            .Replace("{DOORSOPENED}", GetStat<FinalStats>().DoorsOpened.ToString())
            .Replace("{DOORSCLOSED}", GetStat<FinalStats>().DoorsClosed.ToString())
            .Replace("{DOORSDESTROYED}", GetStat<FinalStats>().DoorsDestroyed.ToString())
            .Replace("{FIRST330USER}", GetDisplay(GetStat<SCPStats>().FirstUser, typeof(Player)))
            .Replace("{TOTALCANDIESTAKEN}", GetStat<SCPStats>().TotalCandiesTaken.ToString())
            .Replace("{BUTTONUNLOCKER}", GetDisplay(GetStat<FinalStats>().ButtonUnlocker, typeof(Player)))
            .Replace("{FIRSTWARHEADACTIVATOR}", GetDisplay(GetStat<FinalStats>().FirstActivator, typeof(Player)))
            .Replace("{FIRST914ACTIVATOR}", GetDisplay(GetStat<SCPStats>().FirstActivator, typeof(Player)))
            .Replace("{TOTAL914ACTIVATIONS}", GetStat<SCPStats>().TotalActivations.ToString())
            .Replace("{TOTALITEMUPGRADES}", GetStat<SCPStats>().TotalItemUpgrades.ToString())
            .Replace("{TOTALINTERACTIONS}", GetStat<FinalStats>().TotalInteractions.ToString());
    }
}
