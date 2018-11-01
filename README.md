# PersonalItems
An smod plugin similar to default-item but for specific players instead of classes.

# Installation

Extract the release zip and place the contents in sm_plugins

# Config

This plugin has it's own config which is placed in your global config folder when the plugin is run for the first time.

Default config:
```json
[
    {
        "rank": "all",
        "steamid": "76561198022373616",
        "class": "all",
        "item": "COIN",
        "chance": "50"
    },
    {
        "rank": "donator",
        "steamid": "all",
        "class": "CLASSD",
        "item": "CUP",
        "chance": "50"
    },
]
```

You can add more entries by following the example above. I believe each config option is pretty self explanatory especially with the example above. The role, steamid and class can all be replaced with ALL. If you have any questions, you can ask them in my discord server at the link above.

# Command

`pi_reload` - Reloads the pi config.
