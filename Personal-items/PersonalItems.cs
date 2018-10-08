using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Smod2;
using Smod2.Attributes;
using Smod2.Events;

namespace PersonalItems
{
    [PluginDetails(
        author = "Karl Essinger",
        name = "Personal-items",
        description = "Gives specific players items on spawn.",
        id = "karlofduty.personal-items",
        version = "1.0.0",
        SmodMajor = 3,
        SmodMinor = 1,
        SmodRevision = 18
    )]
    public class PersonalItems : Plugin
    {
        JArray jsonObject;
        public override void OnDisable()
        {

        }

        public override void Register()
        {
            
        }

        public override void OnEnable()
        {
            if (!File.Exists(FileManager.AppFolder + "personal-items.json"))
            {
                File.WriteAllText(FileManager.AppFolder + "personal-items.json", "[]");
            }
            jsonObject = JArray.Parse(File.ReadAllText(FileManager.AppFolder + "personal-items.json"));
        }

        public static bool IsPossibleSteamID(string steamID)
        {
            return (steamID.Length == 17 && long.TryParse(steamID, out long n));
        }

        public Smod2.API.Player GetPlayer(string steamID)
        {
            foreach (Smod2.API.Player player in this.pluginManager.Server.GetPlayers())
            {
                if (player.SteamId == steamID)
                {
                    return player;
                }
            }
            return null;
        }
    }
}