using Newtonsoft.Json.Linq;
using Personal_items.Properties;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Commands;
using Smod2.EventHandlers;
using Smod2.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace PersonalItems
{
    [PluginDetails(
        author = "Karl Essinger",
        name = "Personal-items",
        description = "Gives specific players items on spawn.",
        id = "karlofduty.personal-items",
        version = "1.2.2",
        SmodMajor = 3,
        SmodMinor = 2,
        SmodRevision = 0
    )]
    public class PersonalItems : Plugin
    {
        public bool debug = false;
        public bool verbose = false;
        public JToken config = null;
        public HashSet<string> spawning = new HashSet<string>();
        public Random rnd = new Random();

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
            Reload();
            this.Info("Personal-Items enabled.");
        }

        public void Reload()
        {
            if (!Directory.Exists(FileManager.GetAppFolder() + "Personal-items"))
            {
                Directory.CreateDirectory(FileManager.GetAppFolder() + "Personal-items");
            }

            if (!File.Exists(FileManager.GetAppFolder() + "Personal-items/config.yml"))
            {
                File.WriteAllText(FileManager.GetAppFolder() + "Personal-items/config.yml", Encoding.UTF8.GetString(Resources.config));
            }

            // Reads file contents into FileStream
            FileStream stream = File.OpenRead(FileManager.GetAppFolder() + "Personal-items/config.yml");

            // Converts the FileStream into a YAML Dictionary object
            IDeserializer deserializer = new DeserializerBuilder().Build();
            object yamlObject = deserializer.Deserialize(new StreamReader(stream));

            // Converts the YAML Dictionary into JSON String
            ISerializer serializer = new SerializerBuilder().JsonCompatible().Build();
            string jsonString = serializer.Serialize(yamlObject);

            JObject json = JObject.Parse(jsonString);
            debug = json.SelectToken("debug").Value<bool>();
            verbose = json.SelectToken("verbose").Value<bool>();

            config = json.SelectToken("items");
        }

        public void TryGiveItems(JToken currentNode, IEnumerable<string> nodesToCheck, Player player)
        {
            if (debug)
            {
                this.Info(currentNode.Path);
            }

            if (nodesToCheck.Count() == 0)
            {
                // Checks all filters on the player
                foreach (JObject itemGroupNode in currentNode.Children())
                {
                    // Converts the JObject to key/value pair
                    JProperty itemGroup = itemGroupNode.Properties().First();

                    // Attempts to parse the percentage chance from the config
                    if (int.TryParse(itemGroup.Name, out int chance))
                    {
                        // Rolls a D100
                        int d100 = rnd.Next(1, 100);

                        // Success if dice roll is lower than the percentage chance
                        if (chance >= d100)
                        {
                            if (debug)
                            {
                                this.Info(currentNode.Path + ": Succeded random chance. " + chance + " >= " + d100);
                            }

                            // Gives all items in the item bundle to the player
                            foreach (string itemName in itemGroup.Value as JArray)
                            {
                                if (itemName == "REMOVEALL")
                                {
                                    // Deletes the existing items if set in the config
                                    try
                                    {
                                        foreach (Smod2.API.Item item in player.GetInventory())
                                        {
                                            item.Remove();
                                        }

                                        if (verbose)
                                        {
                                            this.Info("Cleared inventory of " + player.TeamRole.Role + " " + player.Name + "(" + player.SteamId + ").");
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        this.Error("Error occured while resetting inventory of " + player + ".");
                                        if (debug)
                                        {
                                            this.Error(e.ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    // Parses the string into the enumerable value
                                    try
                                    {
                                        player.GiveItem((ItemType)Enum.Parse(typeof(ItemType), itemName));
                                        if (verbose)
                                        {
                                            this.Info(player.TeamRole.Role + " " + player.Name + "(" + player.SteamId + ") was given item " + itemName + ".");
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        this.Error("Error occured while giving item \"" + itemName + "\" to " + player + ".");
                                        if (debug)
                                        {
                                            this.Error(e.ToString());
                                        }
                                    }
                                }
                            }
                        }
                        else if (debug)
                        {
                            this.Info(currentNode.Path + ": Failed random chance. " + chance + " < " + d100);
                        }
                    }
                    else
                    {
                        this.Error("Invalid chance: " + itemGroup.Name);
                    }
                }
                return;
            }

            // Lets all players into open fields from the config
            if (currentNode.SelectToken("all") != null)
            {
                TryGiveItems(currentNode["all"], nodesToCheck.Skip(1), player);
            }

            // Checks if the player fits the filter from the config
            if (currentNode.SelectToken(nodesToCheck.First()) != null)
            {
                TryGiveItems(currentNode[nodesToCheck.First()], nodesToCheck.Skip(1), player);
            }
        }
    }

    internal class ReloadCommand : ICommandHandler
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
            plugin.Reload();
            return new string[] { "Personal-Items has been reloaded." };
        }
    }

    internal class ItemGivingHandler : IEventHandlerSpawn
    {
        private PersonalItems plugin;

        public ItemGivingHandler(PersonalItems plugin)
        {
            this.plugin = plugin;
        }

        public void OnSpawn(PlayerSpawnEvent ev)
        {
            if (!plugin.spawning.Contains(ev.Player.SteamId))
            {
                plugin.spawning.Add(ev.Player.SteamId);
                new Task(async () =>
                {
                    await Task.Delay(1000);
                    plugin.TryGiveItems(plugin.config, new List<string> { ev.Player.SteamId, ev.Player.GetRankName(), ev.Player.TeamRole.Role.ToString() }, ev.Player);
                    plugin.spawning.Remove(ev.Player.SteamId);
                }).Start();
            }
        }
    }
}