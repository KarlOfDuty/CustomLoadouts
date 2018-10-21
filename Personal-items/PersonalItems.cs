using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Commands;
using Smod2.EventHandlers;
using Smod2.Events;

namespace PersonalItems
{
    [PluginDetails(
        author = "Karl Essinger",
        name = "Personal-items",
        description = "Gives specific players items on spawn.",
        id = "karlofduty.personal-items",
        version = "1.2.0",
        SmodMajor = 3,
        SmodMinor = 1,
        SmodRevision = 19
    )]
    public class PersonalItems : Plugin
    {
        public JArray jsonObject;
        public HashSet<string> spawning = new HashSet<string>();
        readonly string defaultConfig = 
        "[\n"                                           +
	    "    {\n"                                       +
        "        \"role\": \"all\",\n"                  +
        "        \"steamid\": \"76561198022373616\",\n" +
		"        \"class\": \"all\",\n"                 +
		"        \"item\": \"COIN\",\n"                 +
        "        \"chance\": \"50\"\n"                  +
        "    },\n"                                      +
        "    {\n"                                       +
        "        \"role\": \"donator\",\n"              +
        "        \"steamid\": \"all\",\n"               +
        "        \"class\": \"CLASSD\",\n"              +
        "        \"item\": \"CUP\",\n"                  +
        "        \"chance\": \"50\"\n"                  +
        "    },\n"                                      +
        "]";

        public override void OnDisable()
        {

        }

        public override void Register()
        {
            this.AddEventHandlers(new ItemGivingHandler(this), Priority.High);
            this.AddCommand("pi_reload", new ReloadCommand(this));
        }

        public override void OnEnable()
        {
            if (!Directory.Exists(FileManager.AppFolder + "Personal-items"))
            {
                Directory.CreateDirectory(FileManager.AppFolder + "Personal-items");
            }

            if (!File.Exists(FileManager.AppFolder + "Personal-items/config.json"))
            {
                File.WriteAllText(FileManager.AppFolder + "Personal-items/config.json", defaultConfig);
            }
            jsonObject = JArray.Parse(File.ReadAllText(FileManager.AppFolder + "Personal-items/config.json"));
            this.Info("Personal-Items enabled.");
        }
    }
    class ReloadCommand : ICommandHandler
    {
        private PersonalItems plugin;
        public ReloadCommand(PersonalItems plugin)
        {
            this.plugin = plugin;
        }

        public string GetCommandDescription()
        {
            return "Reloads the JSON config of Personal-Items";
        }

        public string GetUsage()
        {
            return "pi_reload";
        }

        public string[] OnCall(ICommandSender sender, string[] args)
        {
            plugin.jsonObject = JArray.Parse(File.ReadAllText(FileManager.AppFolder + "Personal-items/config.json "));
            return new string[] { "Personal-Items has been reloaded." };
        }
    }

    class ItemGivingHandler : IEventHandlerSpawn
    {
        private PersonalItems plugin;
        public ItemGivingHandler(PersonalItems plugin)
        {
            this.plugin = plugin;
        }

        public void OnSpawn(PlayerSpawnEvent ev)
        {
            if(!plugin.spawning.Contains(ev.Player.SteamId))
            {
                plugin.spawning.Add(ev.Player.SteamId);
                Thread messageThread = new Thread(new ThreadStart(() => new DelayedItemGiver(plugin, ev.Player)));
                messageThread.Start();
            }
        }
    }

    class DelayedItemGiver
    {
        public DelayedItemGiver(PersonalItems plugin, Player player)
        {
            Thread.Sleep(500);
            Random rnd = new Random();
            for (int i = 0; i < plugin.jsonObject.Count; i++)
            {
                if (string.Equals(plugin.jsonObject[i].SelectToken("role").Value<string>(), player.TeamRole.Role.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(plugin.jsonObject[i].SelectToken("role").Value<string>(), "ALL", StringComparison.OrdinalIgnoreCase))
                {
                    if (plugin.jsonObject[i].SelectToken("steamid").Value<string>() == player.SteamId 
                    || string.Equals(plugin.jsonObject[i].SelectToken("steamid").Value<string>(), "ALL", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(plugin.jsonObject[i].SelectToken("class").Value<string>(), "ALL", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(plugin.jsonObject[i].SelectToken("class").Value<string>(), player.TeamRole.Role.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            if (rnd.Next(1, 100) <= plugin.jsonObject[i].SelectToken("chance").Value<int>())
                            {
                                player.GiveItem((ItemType)Enum.Parse(typeof(ItemType), plugin.jsonObject[i].SelectToken("item").Value<string>()));
                            }
                        }
                    }
                }
            }
            plugin.spawning.Remove(player.SteamId);
        }
    }
}