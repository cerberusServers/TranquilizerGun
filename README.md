<p align="center">
<!--- # Issue counter -->
    <a href="https://github.com/cerberusServers/TranquilizerGun/issues"><img src="https://img.shields.io/github/issues/cerberusServers/TranquilizerGun?color=red&style=for-the-badge"></a>
<!--- # Download counter -->
    <a href="https://github.com/cerberusServers/TranquilizerGun/releases"><img alt="GitHub all releases" src="https://img.shields.io/github/downloads/cerberusServers/TranquilizerGun/total?style=for-the-badge"></a>
<!--- # Repository license -->
   <a href="https://github.com/cerberusServers/TranquilizerGun/blob/master/LICENSE"><img alt="GitHub license" 
src="https://img.shields.io/github/license/cerberusServers/TranquilizerGun?style=for-the-badge"></a>
<!--- # Forks project number -->
   <a href="https://github.com/cerberusServers/TranquilizerGun/network"><img alt="GitHub forks" src="https://img.shields.io/github/forks/cerberusServers/TranquilizerGun?style=for-the-badge"></a>
<!--- # Exiled Discord Server -->
   <a href="https://discord.gg/PyUkWTg"><img alt="Discord" src="https://img.shields.io/discord/656673194693885975?color=critical&label=EXILED%20Discord&style=for-the-badge">
<!--- # Latest release, includes pre-release -->
   <a href="https://github.com/cerberusServers/TranquilizerGun/releases"><img alt="GitHub release (latest by date)" src="https://img.shields.io/github/v/release/cerberusServers/TranquilizerGun?label=Last%20release&style=for-the-badge"></a>
</p>

# 💤 Tranquilizer Gun 2.0 💤
Remade from the ground, here it's the long awaited Tranquilizer Gun plugin! Allowing you to sleep both players and SCPs (Highly configurable).

## What does it do ❓
This plugin lets you put people to sleep using a pistol randomly found in LCZ & HCZ, one shot uses all your ammo and you will need to shoot SCPs twice to put them to sleep (Everything here is configurable, even the weapon!).

# 💾｜Installation
#### First of all, you must have [EXILED](https://github.com/galaxy119/EXILED "EXILED") installed.
Like all Exiled plugins, put the ``.dll`` in the Plugins folder.

Linux:
``.config/EXILED/Plugins``

Windows:
``\AppData\Roaming\EXILED\Plugins``

And you will have to restart the server.


# 📝｜Commands
Arguments inside &lt;&gt; are required. [] means it's optional.
| Command | Description | Arguments |
| ------------- | ------------------------------ | -------------------- |
| `tg`   | Plugin's main command, sends info. | **protect/toggle/replaceguns/etc**|
- Toggle: Toggles all of the plugin's functions besides it's commands.
- ReplaceGuns: Replaces all the COM15s on the map with a Tranquilizer.
- Protect: Protection against "T-Guns" and Sleep command. (Good for administrators!)
- Sleep: Force Sleep on someone.
- AddGun: Gives you a Tranquilizer Gun.
- Version: Shows you what version of this plugin you're using.

# ⚙️｜Configuration
| Config | Default value | Description |
| ----------- | ------------------- | --------------- |
| `IsEnabled` |  true | Is the plugin enabled ? |
| `IsEnabledCustom` | true | If set to false, only the commands will be enabled. |
| `comIsTranquilizer` | false | Is the COM-15 treated as a Tranquilizer ? |
| `uspIsTranquilizer` | true | Is the USP treated as a Tranquilizer ? |
| `silencerRequired` | true | Any of the pistols need a silencer on them to be a Tranquilizer ? |
| `ammoUsedPerShot` | 9 | How much ammo is used per shot |
| `tranquilizerDamage` | 1 | How much damage does each shot of the tranquilizer do ? |
| `sleepDurationMax ` | 5 | Maximum time a player can be asleep. |
| `sleepDurationMin` | 1 | Minimum time a player can be asleep. |
| `clearBroadcasts` | true | Whether broadcasts are cleared before doing one. |
| `UseHintsSystem` | false | Whether to use the new SCP:SL Hints System over broadcasts |
| `tranquilizedBroadcastDuration` | 3 | Broadcast shown when the player is shot with a Tranquilizer. (Using %seconds in the text will be replaced by how many seconds the player will sleep for.
| `tranquilizedBroadcast` | \<color=red>You've been shot with a Tranquilizer... \</color> | When a player falls asleep, the following message will appear. |
| `pickedUpBroadcastDuration` | 5 | Duration of broadcast/Hint
| `pickedUpBroadcast` | \<color=green>\<b>You picked up a tranquilizer gun!\</b>\</color> \nEvery shot uses %ammo ammo, so count your bullets! | When a player picks up a tranquilizer weapon the following message will appear |
| `notEnoughAmmoBroadcastDuration` | 3 | Duration of broadcast/Hint. |
| `notEnoughAmmoBroadcast` | \<color=red>You need at least \%ammo ammo for the bullet to fire! \</color> | Broadcast shown when a player is trying to shoot but has no ammo |
| `FriendlyFire` | true | Can you use the Tranquilizer effects on allies |
|  | Where players are teleported when they are put to sleep. (usingEffects must be disabled) |  |
| `newPos_x` | 2 | |
| `newPos_y` | -2 | |
| `newPos_z` | 3 | |
| `teleportAway` | false | Whether the player will be teleported away. (This + the effect below will give the effect that the old Tranquilizer had). (This will also apply Amnesia + Invisibility effects for the duration of the sleep effect)
| `SummonRagdoll` | false | Whether a Ragdoll is summoned when you're tranquilized. |
| `dropItems` | false | Should the player's inventory be dropped when shot. |
| `areTutorialSerpentsHand` | false | If Serpents Hand is enabled and you don't want friendly fire enabled, set this to true. |
| `doBlacklist` | true | Is the blacklist enabled? |
| `blacklist` | Scp173, Scp106 | List of roles which will be ignored by the Tranquilizer.
| `doSpecialRoles` | false | Enables the multi-shot list. By doing this the role you put in the list needs the specified amount to be slept.  |
| `specialRolesList` | Scp173:2, Scp106:5 | List of roles which will require multiple shots to be put to sleep. |

# 📖｜Permissions
The permissions file should be named ``permissions.yml`` and it is located in the Exiled Config folder.

Windows: ``AppData\Roaming\EXILED\Configs``

Linux: ``.config/EXILED/Plugins/Configs``

Here is an example of how to assign a permission to a role
```yml
owner:
    default: false
    inheritance: []
    permissions:
        - tgun.tg
        - tgun.toggle
        - tgun.givegun
```

| Permission  | This permission belongs to |
| ------------- | ------------- |
| tgun.tg | `tg` and it's arguments | 
| tgun.armor | `tg protect` | 
| tgun.toggle | `tg toggle` | 
| tgun.replaceguns | `tg replaceguns` |
| tgun.sleep | `tg sleep` |
| tgun.givegun | `tg addgun` |
| tgun.* | `All above` | 

# Planned Changes:
- Add more configurable options. (Open to suggestions)
- ~~Changing the "sleeping" system. (Mostly waiting for new SCP:SL patch to hit for effects like slowing someone, stunning them, blurry their vision and stuff like that!)~~
- ~~SCPs Blacklist. (Configurable too)~~

# ❓｜F.A.Q.:
- **Using older versions of EXILED make this plugin not work:**
*This will not be fixed, but I still get people asking why this plugin doesn't work when they don't have EXILED up to date, so if this plugin doesn't work, try updating to the recommended EXILED version stated in the downloads tab.*

- **The message above says I don't have any bullets but my gun seems like it's shooting, is this a bug?**
*Sadly, most (if not all) animations can't be disabled since they're client-side, everything you see made in EXILED is server-side only. I still haven't found a way to do it, but once I do, don't worry that I'll immediately upload a fix. For the meantime, don't worry, even if it looks like you're shooting, you're not, **IT DOES NOT MAKE YOU SHOOT MORE THAN YOU SHOULD!** (This can be kinda avoided by changing `ammo_used_per_shot` with a value like 9, using the USP)*

- **How do I download?**
*There's a **releases** tab at the side of this Github page, or... just press [here!](https://github.com/cerberusServers/TranquilizerGun/releases)*

### That'd be all
Thanks for passing by, have a nice day! :)
