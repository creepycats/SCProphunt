[![Github All Releases](https://img.shields.io/github/downloads/creepycats/SCProphunt/total.svg)](https://github.com/creepycats/SCProphunt/releases) [![Maintenance](https://img.shields.io/badge/Maintained%3F-yes-green.svg)](https://github.com/creepycats/SCProphunt/graphs/commit-activity) [![GitHub license](https://img.shields.io/github/license/Naereen/StrapDown.js.svg)](https://github.com/creepycats/SCProphunt/blob/main/LICENSE)
<a href="https://github.com/creepycats/SCProphunt/releases"><img src="https://img.shields.io/github/v/release/creepycats/SCProphunt?include_prereleases&label=Release" alt="Releases"></a>
<a href="https://discord.gg/PyUkWTg"><img src="https://img.shields.io/discord/656673194693885975?color=%23aa0000&label=EXILED" alt="Support"></a>

# SCProphunt
An SCP:SL Exiled Plugin, Porting the PropHunt Gamemode From Garry's Mod as an Auto-Events-style Gamemode!

Made for `v13.3.1` of SCP:SL and `v8.4.3` of Exiled and onward by creepycats.

## Showcase
[Click To Watch on Youtube](http://www.youtube.com/watch?v=1za77upqP_A)

[![Plugin Showcase](http://img.youtube.com/vi/1za77upqP_A/0.jpg)](http://www.youtube.com/watch?v=1za77upqP_A "I Brought PROPHUNT to SCP: SECRET LABORATORY | Multiplayer Highlights")

## Features
- Props
    - Players only show up as Items to each other
    - Switch your Prop Item by Trying to Pick Up another Item
    - Pull out the Flashbang (Press your Hotkey) to Taunt on Command
    - Props automatically Taunt every Minute
    - Owners can add Custom Taunts if they Choose
    - Health and Size change depending on the Prop possessed
    - Non-player Prop Amounts scale with the number of players (More fake props when more players online)
    - 
- Hunters
    - Start with a FSP, Com-18, Grenade, Armor, and Infinite Ammo
    - After 30 seconds, Hunters are Released
    - Incorrect Guesses Penalize Hunters' Health
    - Killing Props will Let Hunters Regain their HP
    - Shooting the Player Props will give Hunters a hitmarker
    - Their inventory is full with coins to prevent trying to pick up Props
- Voting System : Vote before Each Round to choose whether the game is played in Light Containment, Heavy Containment, or Entrance Zone
- Music and Sound Effects : Sells the Vibe of 2010s Prophunt
- Customizable Taunt List
- Broadcast Hud : Features the number of Props and Hunters online
- Custom Killfeed : Stylized Killfeed using Hints to simulate the GMod Killfeed and announce when Hunters or Props die
- Doors and Pre-existing Items are Managed by the Game
- Items that existed before the game can randomly be removed and replaced after the game, presenting new playing fields each round

## Installation
**This Plugin Requires SCPSLAudioApi, [you can download it here](https://github.com/CedModV2/SCPSLAudioApi/releases)**

After you've installed SCPSLAudioApi, go to [Releases](https://github.com/creepycats/SCProphunt/releases) and download the latest Zip file.

Extract the Zip, and Paste the `Config` and `Plugins` folders into your `EXILED` folder.

## Permissions
`SCProphunt.admin`

## Commands
**Make sure you Enable Round Lock before starting a round!**

REMOTE ADMIN:
- `propadmin start` : Starts a Prophunt Round. Lasts 8.5 minutes by Default
- `propadmin start <time>` : Starts a Prophunt Round with the Given Time Limit
- `propadmin end/stop` : Ends a Prophunt Round Early
- `propadmin team <player> prop/admin` : Force a Player to Join a Specific Team
