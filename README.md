![VERSION](https://img.shields.io/github/v/release/Thundermaker300/RoundReports?include_prereleases&style=for-the-badge)
![DOWNLOADS](https://img.shields.io/github/downloads/Thundermaker300/RoundReports/total?style=for-the-badge)

# Round Reports
 An SCP:SL Exiled plugin which generates reports at the end of each round, and shares them in your Discord server.  This plugin supports Serpent's Hand and UIU Rescue Squad, and will display stats accordingly.
  
This plugin uses [paste.ee](https://paste.ee/) for hosting reports. This service is provided entirely for free. Reports automatically expire after 30 days; however, they can be saved with a simple tool like the [Wayback Machine](https://web.archive.org/).  
  
Note: Statistics from users with Do Not Track (DNT) enabled will still be shown, but all personal information relating to them (Username, SteamId, etc) will be hidden. Do Not Track users will not be present in the MVP stats.

 ## Commands
 ### pausereport
 Permission: rr.pause  
 Disables reporting for the duration of the round. Automatically unpauses at the start of the next round.

 ### addremark / remark
 Permission: rr.remark  
 Adds a remark to the report. Can be used to specify event rounds & etc.

## MVP Statistic Info
MVP is calculated by granting players points for specific actions. These points can also be taken away by other actions. There are two separate point systems, and as such two separate MVPs: One for SCPs, and one for humans. Full list of actions and the points they grant or remove below. (Configurable: Coming soon)

### Human Points

| **Action**                            | **Point Change** |
|---------------------------------------|------------------|
| Kill SCP                              | +10              |
| Sacrifice in Femur Breaker            | +6               |
| Escape                                | +5               |
| Press Femur Breaker Button            | +4               |
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
| Kill Scientist                        | +3               |
| Kill Player (Not Scientist)           | +2               |
| Open Door (SCP-079)                   | +1-3\*\*         |
| Lockdown Room (SCP-079)               | +1\*             |
| Die (Most Causes)                     | -5               |
| Die (Warhead, Decontamination, Tesla) | -10              |

\*+1 for each player in the room. Does not include SCPs, Tutorials, and Serpent's Hand (if the plugin is installed).  
\*\*+1 if there is an SCP nearby. +2 if the door is a keycard door and there is an SCP nearby. +3 for keycarded gate with SCP nearby.