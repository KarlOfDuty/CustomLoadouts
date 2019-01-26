# CustomLoadouts
An smod plugin similar to default-item but much more customisable to specific players. Can give items based on steamid, class, server rank and random chance or any combination of these.

Can also be used to set up special subclasses with customized loadouts with a customisable chance to get the loadout on spawn.

IMPORTANT: Turn off smart class picker in smod or wrong items may be given to players as it changes their class on spawn.

## Installation

Extract the release zip and place the contents in sm_plugins.

## Config

This plugin has it's own config which is placed in your global config folder when the plugin is run for the first time.

The plugin only has one option in the server config, `cl_config`, which can be used to change which plugin config to use, default value is `config.yml`.

Default config:
```yaml
debug: false
verbose: true

# Filters are processed in the order they are written except anything marked as "all" is always processed before all other entries
items:
    # SteamID
    all:
        # Rank, the name of the server rank defined in config_remoteadmin.txt. NOT THE BADGE, THE VARIABLE NAME
        donator:
            # Class, check #resources in the SMOD discord server for class names
            all:
                # Percentage chance for item group to spawn
                - 50:
                    # Items to spawn, must be all caps, check #resources in the SMOD discord server for item names. You can start with REMOVEITEMS and REMOVEAMMO to delete the existing items/ammo.
                    # All weapons spawn with one mag/clip loaded which cannot be removed, giving ammo adds the ammo directly to the player instead of spawning it as an item in their inventory.
                    - COIN
                - 10:
                    - MEDKIT
            SCIENTIST:
                - 10:
                    - REMOVEITEMS
                    - MAJOR_SCIENTIST_KEYCARD
                    - MEDKIT
                - 5:
                    - REMOVEITEMS
                    - REMOVEAMMO
                    - USP
                    - DROPPED_9
                    - DROPPED_9
                    - MAJOR_SCIENTIST_KEYCARD
                    - MEDKIT
        moderator:
            all:
                - 30:
                    - FLASHLIGHT
    # Quotes are important on SteamIDs
    "76561198022373616":
        all:
            all:
                - 100:
                    - MICROHID
                    - MICROHID
                    - MICROHID
                    - MICROHID
                    - MICROHID
                    - MICROHID
                    - MICROHID
                    - MICROHID
```

You can add more entries by following the example above. I believe each config option is pretty self explanatory with the example above. The role, steamid and class can all be replaced with `all`. If you have any questions, you can ask them in my discord server at the link above.

## Command

`cl_reload` - Reloads the cl config.
