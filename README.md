![VERSION](https://img.shields.io/github/v/release/Thundermaker300/RoundReports?include_prereleases&style=for-the-badge)
![DOWNLOADS](https://img.shields.io/github/downloads/Thundermaker300/RoundReports/total?style=for-the-badge)

# Round Reports
 An SCP:SL Exiled plugin which generates reports at the end of each round, and shares them in your Discord server. Also includes end-of-round broadcasts, with support for over 20 different round statistics displayed in these broadcasts.  This plugin supports Serpent's Hand and UIU Rescue Squad, and will display stats accordingly.
  
This plugin uses [paste.ee](https://paste.ee/) for hosting reports. This service is not affiliated with RoundReports, and they have their own Terms of Service you must abide by when using this plugin. By default, reports automatically expire after 30 days; however, this can be configured, and reports can be saved with a simple tool like the [Wayback Machine](https://web.archive.org/).  
  
Note: Statistics from users with Do Not Track (DNT) enabled are treated differently, please see "Note on DNT Players" section below.

 ## Commands
 ### pausereport
 Permission: rr.pause  
 Disables reporting for the duration of the round. Automatically unpauses at the start of the next round.  

 ### addremark / remark
 Permission: rr.remark  
 Adds a remark to the report. Can be used to specify event rounds & etc.  
  
## Note on DNT Players
Players with Do Not Track enabled will never have their username present on reports. How their stats are handled is based on the `ExcludeDNTUsers` config. If this config is set to `true`, no stats at all will be recorded except for kill logs (in which case their name will be removed). If this config is set to `false`, their stats will still be included but their name will be removed. Do Not Track players will never be included in the MVP statistics, regardless of this setting's value.

## MVP Statistic Info
MVP is calculated by granting players points for specific actions. These points can also be taken away by other actions. There are two separate point systems, and as such two separate MVPs: One for SCPs, and one for humans. Full list of actions and the points they grant or remove below. (Configurable: Coming soon)

### Human Points

| **Action**                            | **Point Change** |
|---------------------------------------|------------------|
| Kill SCP                              | +10              |
| Escape                                | +5               |
| Kill Scientist                        | +3               |
| Kill Player (Not Scientist)           | +2               |
| Open Warhead Panel (Surface)          | +2               |
| Die (Most Causes)                     | -1               |
| Die (Warhead, Decontamination, Tesla) | -2               |
| Take 3 Candies                        | -10              |
| Kill Teammate                         | -10              |


### SCP Points

| **Action**                            | **Point Change** |
|---------------------------------------|------------------|
| Level Up (SCP-079)                    | +5               |
| Tesla Gate Kill (SCP-079)             | +5               |
| Kill Scientist                        | +3               |
| Kill Player (Not Scientist)           | +2               |
| Open Door (SCP-079)                   | +0-2\*\*         |
| Kill Assist (SCP-079)                 | +1\*             |
| Capture Player (SCP-106)              | +1               |
| Die (Most Causes)                     | -5               |
| Die (Warhead, Decontamination, Tesla) | -10              |

\*+1 for each player in a room that SCP-079 has blacked out/locked down. Does not include SCPs, Tutorials, and Serpent's Hand (if the plugin is installed).  
\*\*+1 if the door is a keycard door and there is an SCP nearby. +2 for keycarded gate with SCP nearby.

 ## Stats
 Every stat below can be disabled in the config by adding its respective `key` to the `IgnoredStats` config. Some of the following stats can be used in end-of-round broadcasts, and the Discord embed footer config. Note that arguments must be surrounded by curly braces and capitalized (eg. `{HUMANMVP}`).
| **Key**               | **Available in Broadcasts** | **Description**                                                                         |
|-----------------------|-----------------------------|-----------------------------------------------------------------------------------------|
| AdrenalinesConsumed   |                             | The total amount of adrenalines consumed.                                               |
| AllActivations        |                             | Each SCP-914 mode and the total amount of activations.                                  |
| AllUpgrades           |                             | Each item type and the amount of times it's been upgraded in SCP-914.                   |
| ButtonUnlocked        |                             | Yes/No if the warhead button was opened.                                                |
| ButtonUnlocker        |                             | The name of the player who unlocked the warhead.                                        |
| CandiesByPlayer       |                             | Each player and the amount of candies they've taken from SCP-330.                       |
| CandiesTaken          | Yes                         | The total amount of candies taken from SCP-330.                                         |
| ChaosKills            | Yes                         | The total amount of Chaos Insurgency kills in the round.                                |
| DamageByPlayer        |                             | Each player and the amount of damage they've dealt.                                     |
| DamageByType          |                             | Each damage type and the amount of damage dealt.                                        |
| DClassKills           | Yes                         | The total amount of Class-D kills in the round.                                         |
| Detonated             |                             | Yes/No if the warhead was detonated.                                                    |
| DetonationTime        |                             | The time the warhead was detonated.                                                     |
| DoorsClosed           | Yes                         | The total amount of closed door interactions.                                           |
| DoorsDestroyed        | Yes                         | The total amount of destroyed doors.                                                    |
| DoorsOpened           | Yes                         | The total amount of open door interactions.                                             |
| Drops                 |                             | Each item type and the amount of times it's been dropped.                               |
| EndTime               |                             | The end time of the round.                                                              |
| FirearmUpgrades       |                             | The total amount of times a firearm was upgraded.                                       |
| First330Use           |                             | The time in the round the first SCP-330 candy was obtained.                             |
| First330User          |                             | The name of the player who used SCP-330 first.                                          |
| First914Activation    |                             | The time in the round the first SCP-914 cycle was activated.                            |
| First914Activator     |                             | The name of the player who activated SCP-914 first.                                     |
| FirstWarheadActivator |                             | The name of the player who activated the warhead first.                                 |
| HumanMVP              | Yes                         | The human MVP of the round.                                                             |
| HumanKills            | Yes                         | The total amount of human kills in the round. Broadcast argument only.                  |
| HumanPoints           |                             | Each player who has played a human role and the amount of points they've received.      |
| KeycardScans          | Yes                         | The total amount of keycard scans.                                                      |
| KeycardUpgrades       |                             | The total amount of times a keycard was upgraded.                                       |
| KillsByPlayer         |                             | Each player and their amount of kills.                                                  |
| KillsByType           |                             | Each death type and the total amount of kills of that type.                             |
| MedkitsConsumed       |                             | The total amount of medkits consumed.                                                   |
| MTFKills              | Yes                         | The total amount of MTF kills in the round.                                             |
| PainkillersConsumed   |                             | The total amount of painkillers consumed.                                               |
| PlayerCount           | Yes                         | The total amount of players currently connected to the server. Broadcast argument only. |
| PlayerDamage          |                             | The total amount of damage dealt by players.                                            |
| PlayerDoorsClosed     |                             | Each player and the amount of doors they've closed.                                     |
| PlayerDoorsOpened     |                             | Each player and the amount of doors they've opened.                                     |
| PlayerDrops           |                             | Each player and the amount of items they've dropped.                                    |
| PlayerKills           |                             | Each player and the amount of players they've killed.                                   |
| PointLogs             |                             | A log for each time points are added or removed.                                        |
| Respawns              |                             | Each respawn that occurs.                                                               |
| RoundTime             | Yes                         | The total time of the round.                                                            |
| ScientistKills        | Yes                         | The total amount of scientist kills in the round.                                       |
| Scp018sThrown         |                             | The total amount of SCP-018s thrown.                                                    |
| Scp096Charges         |                             | The total amount of SCP-096 charges. Only shown if value is greater than 0.             |
| Scp096Enrages         |                             | The total amount of SCP-096 enrages. Only shown if value is greater than 0.             |
| Scp106Teleports       |                             | The total amount of SCP-106 teleports. Only shown if value is greater than 0.           |
| Scp173Blinks          |                             | The total amount of SCP-173 blinks. Only shown if value is greater than 0.              |
| Scp1853Uses           |                             | The total amount of SCP-1853 uses.                                                      |
| Scp207sDrank          |                             | The total amount of SCP-207s drank.                                                     |
| Scp268Uses            |                             | The total amount of SCP-268 uses.                                                       |
| SCP500sConsumed       |                             | The total amount of SCP-500s consumed.                                                  |
| SCPKills              | Yes                         | The total amount of SCP kills in the round.                                             |
| SCPMVP                | Yes                         | The SCP MVP of the round.                                                               |
| SCPPoints             |                             | Each player who has played an SCP and the amount of points they received.               |
| SerpentsHandKills     |                             | The total amount of Serpent's Hand kills (only if the plugin is installed).             |
| SeveredHands          |                             | The total amount of hands severed from taking too many SCP-330 candies.                 |
| SpawnWaves            | Yes                         | List of each spawn wave. Converts to the total amount of respawn waves in broadcasts.   |
| StartClassD           |                             | The amount of Class-D personnel at the start of the round.                              |
| StartFacilityGuard    |                             | The amount of Facility Guards at the start of the round.                                |
| StartPlayers          |                             | The total amount of players at the start of the round.                                  |
| StartScientist        |                             | The amount of Scientists at the start of the round.                                     |
| StartSCP              |                             | A list of every SCP at the start of the round.*                                         |
| StartTime             | Yes                         | The time that the round started.                                                        |
| SurvivingPlayers      |                             | Each player that is alive at the end of the round.                                      |
| Total914Activations   | Yes                         | The total amount of times SCP-914 was activated.                                        |
| TotalCandiesTaken     |                             | The total amount of candies taken.                                                      |
| TotalDamage           | Yes                         | The total amount of damage dealt.                                                       |
| TotalDeaths           | Yes                         | The total amount of deaths.                                                             |
| TotalDrops            | Yes                         | The total amount of items dropped.                                                      |
| TotalItemUpgrades     |                             | The total amount of item upgrades.                                                      |
| TotalKills            | Yes                         | The total amount of kills in the round.                                                 |
| TotalReloads          | Yes                         | The total amount of firearm reloads.                                                    |
| TotalRespawned        | Yes                         | The total amount of respawned players.                                                  |
| TotalShotsFired       | Yes                         | The total amount of shots fired.                                                        |
| TutorialKills         |                             | The total amount of tutorial kills. Only shown if value is greater than 0.              |
| UIUKills              |                             | The total amount of UIU kills (only if the plugin is installed).                        |
| WinningTeam           | Yes                         | The winning team of the round.                                                          |                                                   

\* This stat is updated 60 seconds into the round to account for early SCP deaths, SCPSwap, etc.

Note: `HumanKills` and `PlayerCount` are only available as broadcast/embed arguments. These two stats cannot be used in IgnoredStats and will not show up on the round report.
