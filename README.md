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
| **Key**               | **Available as Argument**   | **Description**                                                                         |
|-----------------------|-----------------------------|-----------------------------------------------------------------------------------------|
| AdrenalinesConsumed   |                             | TBA                                                                                     |
| AllActivations        |                             | TBA                                                                                     |
| AllUpgrades           |                             | TBA                                                                                     |
| ButtonUnlocked        |                             | TBA                                                                                     |
| ButtonUnlocker        |                             | TBA                                                                                     |
| CandiesByPlayer       |                             | TBA                                                                                     |
| CandiesTaken          | Yes                         | The total amount of candies taken from SCP-330.                                         |
| ChaosKills            | Yes                         | The total amount of Chaos Insurgency kills in the round.                                |
| DamageByPlayer        |                             | TBA                                                                                     |
| DamageByType          |                             | TBA                                                                                     |
| DClassKills           | Yes                         | The total amount of Class-D kills in the round.                                         |
| Detonated             |                             | TBA                                                                                     |
| DetonationTime        |                             | TBA                                                                                     |
| DoorsClosed           | Yes                         | The total amount of closed door interactions.                                           |
| DoorsDestroyed        | Yes                         | The total amount of destroyed doors.                                                    |
| DoorsOpened           | Yes                         | The total amount of open door interactions.                                             |
| Drops                 |                             | TBA                                                                                     |
| EndTime               |                             | TBA                                                                                     |
| FirearmUpgrades       |                             | TBA                                                                                     |
| First330Use           |                             | TBA                                                                                     |
| First330User          |                             | TBA                                                                                     |
| First914Activation    |                             | TBA                                                                                     |
| First914Activator     |                             | TBA                                                                                     |
| FirstWarheadActivator |                             | TBA                                                                                     |
| HumanMVP              | Yes                         | The human MVP of the round.                                                             |
| HumanKills            | Yes                         | The total amount of human kills in the round. Broadcast argument only.                  |
| HumanPoints           |                             | TBA                                                                                     |
| KeycardScans          | Yes                         | The total amount of keycard scans.                                                      |
| KeycardUpgrades       |                             | TBA                                                                                     |
| KillsByPlayer         |                             | TBA                                                                                     |
| KillsByType           |                             | TBA                                                                                     |
| MedkitsConsumed       |                             | TBA                                                                                     |
| MTFKills              | Yes                         | The total amount of MTF kills in the round.                                             |
| PainkillersConsumed   |                             | TBA                                                                                     |
| PlayerCount           | Yes                         | The total amount of players currently connected to the server. Broadcast argument only. |
| PlayerDamage          |                             | TBA                                                                                     |
| PlayerDoorsClosed     |                             | TBA                                                                                     |
| PlayerDoorsOpened     |                             | TBA                                                                                     |
| PlayerDrops           |                             | TBA                                                                                     |
| PlayerKills           |                             | TBA                                                                                     |
| PointLogs             |                             | TBA                                                                                     |
| Respawns              |                             | TBA                                                                                     |
| RoundTime             | Yes                         | The total time of the round.                                                            |
| ScientistKills        | Yes                         | The total amount of scientist kills in the round.                                       |
| Scp018sThrown         |                             | TBA                                                                                     |
| Scp096Charges         |                             | TBA                                                                                     |
| Scp096Enrages         |                             | TBA                                                                                     |
| Scp106Teleports       |                             | TBA                                                                                     |
| Scp173Blinks          |                             | TBA                                                                                     |
| Scp1853Uses           |                             | TBA                                                                                     |
| Scp207sDrank          |                             | TBA                                                                                     |
| Scp268Uses            |                             | TBA                                                                                     |
| SCP500sConsumed       |                             | TBA                                                                                     |
| SCPKills              | Yes                         | The total amount of SCP kills in the round.                                             |
| SCPMVP                | Yes                         | The SCP MVP of the round.                                                               |
| SCPPoints             |                             | TBA                                                                                     |
| SerpentsHandKills     |                             | TBA                                                                                     |
| SeveredHands          |                             | TBA                                                                                     |
| SpawnWaves            | Yes                         | The total amount of respawn waves.                                                      |
| StartClassD           |                             | TBA                                                                                     |
| StartFacilityGuard    |                             | TBA                                                                                     |
| StartPlayers          |                             | TBA                                                                                     |
| StartScientist        |                             | TBA                                                                                     |
| StartSCP              |                             | TBA                                                                                     |
| StartTime             | Yes                         | The time that the round started.                                                        |
| SurvivingPlayers      |                             | TBA                                                                                     |
| Total914Activations   | Yes                         | The total amount of times SCP-914 was activated.                                        |
| TotalCandiesTaken     |                             | TBA                                                                                     |
| TotalDamage           | Yes                         | The total amount of damage dealt.                                                       |
| TotalDeaths           | Yes                         | The total amount of deaths.                                                             |
| TotalDrops            | Yes                         | The total amount of items dropped.                                                      |
| TotalItemUpgrades     |                             | TBA                                                                                     |
| TotalKills            | Yes                         | The total amount of kills in the round.                                                 |
| TotalReloads          | Yes                         | The total amount of firearm reloads.                                                    |
| TotalRespawned        | Yes                         | The total amount of respawned players.                                                  |
| TotalShotsFired       | Yes                         | The total amount of shots fired.                                                        |
| TutorialKills         |                             | TBA                                                                                     |
| UIUKills              |                             | TBA                                                                                     |
| WinningTeam           | Yes                         | The winning team of the round.                                                          |

Note: `HumanKills` and `PlayerCount` are only available as broadcast/embed arguments. These two stats cannot be used in IgnoredStats and will not show up on the round report.
