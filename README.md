# CustomLoadouts [![Build Status](https://jenkins.karlofduty.com/job/CI/job/CustomLoadouts/job/master/badge/icon)](https://jenkins.karlofduty.com/blue/organizations/jenkins/CI%2FCustomLoadouts/activity) [![Release](https://img.shields.io/github/release/KarlofDuty/CustomLoadouts.svg)](https://github.com/KarlOfDuty/CustomLoadouts/releases) [![Downloads](https://img.shields.io/github/downloads/KarlOfDuty/CustomLoadouts/total.svg)](https://github.com/KarlOfDuty/CustomLoadouts/releases) [![Discord Server](https://img.shields.io/discord/430468637183442945.svg?label=discord)](https://discord.gg/C5qMvkj)

An smod plugin similar to default-item but much more customisable to specific players. It can give items based on the player's class and a random chance where access to each loadout can be restricted to specific players or ranks using permissions.

Can also be used to set up special subclasses with customized loadouts with a customisable chance to get the loadout on spawn.

## Installation

Extract the release zip and place the contents in sm_plugins.

## Config

This plugin has it's own config which is placed in your global config folder when the plugin is run for the first time.

The plugin only has one option in the server config, `cl_global`, it decides whether to use the global config folder or not, defaults to true.

### Default config:
```yaml
# Prints a confirmation whenever a player is given a loadout.
verbose: true
# Prints debug messages.
debug: false
# Adds a ms delay if needed for plugin compatibility.
delay: 0

# This sets up the loadouts and the permission nodes required to get them.
# For instance "customloadouts.donatorloadouts" gives all loadouts under the donatorloadouts node below, as long as the class and chance checks are successful.
customloadouts:
    # Name of this permission node.
    donatorloadouts:
        # Class, check https://github.com/ServerMod/Smod2/blob/master/Smod2/API/TeamRole.cs for class names (called RoleType in the link).
        # This one is set to all to give to all classes.
        all:
            # Percentage chance for item group to spawn.
            - 50:
                # Items to spawn, check https://github.com/ServerMod/Smod2/blob/master/Smod2/API/Item.cs for item names.
                # You can start a loadout with REMOVEITEMS and REMOVEAMMO to delete the existing items/ammo.
                # All weapons spawn with one mag/clip loaded which cannot be removed, giving ammo adds the ammo directly to the player instead of spawning it as an item in their inventory.
                - COIN # This example has a 50% chance to spawn a coin to all players with the customloadouts.donatorloadouts permission node.
            - 10:
                - MEDKIT
        SCIENTIST:
            - 10:
                # Starts with REMOVEITEMS to clear the default inventory before the items are added.
                - REMOVEITEMS
                - KEYCARD_SCIENTIST_MAJOR
                - MEDKIT
            - 5:
                - REMOVEITEMS
                - REMOVEAMMO
                - GUN_USP
                - AMMO9MM
                - AMMO9MM
                - KEYCARD_SCIENTIST_MAJOR
                - MEDKIT
    moderatorloadouts:
        CLASSD:
            - 30:
                - FLASHLIGHT
                - AMMO9MM
                - AMMO762
                - AMMO556
    adminloadouts:
        all:
            - 100:
                - MICRO_HID
                - MICRO_HID
                - MICRO_HID
                - MICRO_HID
                - MICRO_HID
                - MICRO_HID
                - MICRO_HID
                - MICRO_HID
```

You can add more entries by following the example above. I believe each config option is pretty self explanatory config comments.  If you have any questions, you can ask them in my discord server at the link above.

## Command
| Command | Permission | Description |
|----     |----        |----         |
| `cl_reload` | `customloadouts.reload` | Reloads the cl config.
