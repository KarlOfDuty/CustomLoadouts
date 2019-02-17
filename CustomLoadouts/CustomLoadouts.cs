using CustomLoadouts.Properties;
using Newtonsoft.Json.Linq;
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

namespace CustomLoadouts
{
    [PluginDetails(
        author = "Karl Essinger",
        name = "CustomLoadouts",
        description = "Gives specific players items on spawn.",
        id = "karlofduty.CustomLoadouts",
        version = "2.1.0",
        SmodMajor = 3,
        SmodMinor = 3,
        SmodRevision = 0
    )]
    public class CustomLoadouts : Plugin
    {
        internal JToken config;
        private bool debug;
        internal Random rnd = new Random();
        internal HashSet<string> spawning = new HashSet<string>();
        private bool verbose;
        internal int delay = 1000;

        public override void OnDisable()
        {
            // It's ya boi, useless function
        }

        public override void OnEnable()
        {
            new Task(async () =>
            {
                await Task.Delay(4000);
                this.Info("Loading config " + GetConfigString("cl_config") + "...");
                Reload();
                this.Info("Config loaded.");
                this.Info("CustomLoadouts enabled.");
            }).Start();
        }

        public override void Register()
        {
            this.AddEventHandlers(new ItemGivingHandler(this), Priority.High);
            this.AddCommand("cl_reload", new ReloadCommand(this));
            this.AddConfig(new Smod2.Config.ConfigSetting("cl_config", "config.yml", Smod2.Config.SettingType.STRING, true, "Name of the config file to use, by default 'config.yml'"));
        }

        public void Reload()
        {
            if (!Directory.Exists(FileManager.GetAppFolder() + "CustomLoadouts"))
            {
                Directory.CreateDirectory(FileManager.GetAppFolder() + "CustomLoadouts");
            }

            if (!File.Exists(FileManager.GetAppFolder() + "CustomLoadouts/" + GetConfigString("cl_config")))
            {
                File.WriteAllText(FileManager.GetAppFolder() + "CustomLoadouts/" + GetConfigString("cl_config"), Encoding.UTF8.GetString(Resources.config));
            }

            // Reads file contents into FileStream
            FileStream stream = File.OpenRead(FileManager.GetAppFolder() + "CustomLoadouts/" + GetConfigString("cl_config"));

            // Converts the FileStream into a YAML Dictionary object
            IDeserializer deserializer = new DeserializerBuilder().Build();
            object yamlObject = deserializer.Deserialize(new StreamReader(stream));

            // Converts the YAML Dictionary into JSON String
            ISerializer serializer = new SerializerBuilder().JsonCompatible().Build();
            string jsonString = serializer.Serialize(yamlObject);

            JObject json = JObject.Parse(jsonString);

            // Sets config variables
            debug = json.SelectToken("debug").Value<bool>();
            verbose = json.SelectToken("verbose").Value<bool>();
            delay = json.SelectToken("delay").Value<int>();

            config = json.SelectToken("items");
        }

        public void TryGiveItems(JToken currentNode, IEnumerable<string> nodesToCheck, Player player)
        {
            if (debug)
            {
                this.Info(currentNode.Path);
            }

            try
            {
                if (nodesToCheck.Count() == 0)
                {
                    if (debug)
                    {
                        this.Info("Preparing to give items if random chance succeeds...");
                    }

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
                                    switch (itemName)
                                    {
                                        case "REMOVEAMMO":
                                            // Deletes the existing ammo if set in the config
                                            try
                                            {
                                                player.SetAmmo(AmmoType.DROPPED_5, 0);
                                                player.SetAmmo(AmmoType.DROPPED_7, 0);
                                                player.SetAmmo(AmmoType.DROPPED_9, 0);

                                                if (verbose)
                                                {
                                                    this.Info("Cleared ammo of " + player.TeamRole.Role + " " + player.Name + "(" + player.SteamId + ").");
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                this.Error("Error occured while resetting ammo of " + player + ".");
                                                if (debug)
                                                {
                                                    this.Error(e.ToString());
                                                }
                                            }
                                            break;

                                        case "REMOVEITEMS":
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
                                            break;

                                        case "DROPPED_5":
                                            // Gives a mag of 5.56mm ammo
                                            try
                                            {
                                                player.SetAmmo(AmmoType.DROPPED_5, player.GetAmmo(AmmoType.DROPPED_5) + 25);
                                                if (verbose)
                                                {
                                                    this.Info(player.TeamRole.Role + " " + player.Name + "(" + player.SteamId + ") was given a mag of 5.56mm ammo (25 shots).");
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                this.Error("Error occured while giving a mag of 5.56mm ammo to " + player + ".");
                                                if (debug)
                                                {
                                                    this.Error(e.ToString());
                                                }
                                            }
                                            break;

                                        case "DROPPED_7":
                                            // Gives a mag of 7.62mm ammo
                                            try
                                            {
                                                player.SetAmmo(AmmoType.DROPPED_7, player.GetAmmo(AmmoType.DROPPED_7) + 35);
                                                if (verbose)
                                                {
                                                    this.Info(player.TeamRole.Role + " " + player.Name + "(" + player.SteamId + ") was given a mag of 7.62mm ammo (35 shots).");
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                this.Error("Error occured while giving a mag of 7.62mm ammo to " + player + ".");
                                                if (debug)
                                                {
                                                    this.Error(e.ToString());
                                                }
                                            }
                                            break;

                                        case "DROPPED_9":
                                            // Gives a clip of 9mm ammo
                                            try
                                            {
                                                player.SetAmmo(AmmoType.DROPPED_9, player.GetAmmo(AmmoType.DROPPED_9) + 15);
                                                if (verbose)
                                                {
                                                    this.Info(player.TeamRole.Role + " " + player.Name + "(" + player.SteamId + ") was given a clip of 9mm ammo (15 shots).");
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                this.Error("Error occured while giving a clip of 9mm ammo to " + player + ".");
                                                if (debug)
                                                {
                                                    this.Error(e.ToString());
                                                }
                                            }
                                            break;

                                        default:
                                            // Parses the string to the enumerable itemtype
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
                                            break;
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
            }
            catch (Exception e)
            {
                this.Error("ERROR while giving player items: \n" + e.ToString());
            }

            try
            {
                // Lets all players into open fields from the config
                if (currentNode.SelectToken("all") != null)
                {
                    if (debug)
                    {
                        this.Info(currentNode.Path + ".all exists.");
                    }
                    TryGiveItems(currentNode["all"], nodesToCheck.Skip(1), player);
                }
                else if (debug)
                {
                    this.Info(currentNode.Path + ".all does not exist.");
                }
            }
            catch (Exception e)
            {
                this.Error("ERROR while checking if " + currentNode.Path + "." + nodesToCheck.First() + " node exists: \n" + e.ToString());
            }

            try
            {
                // Checks if the player fits the filter from the config
                if (currentNode.SelectToken(nodesToCheck.First()) != null)
                {
                    if (debug)
                    {
                        this.Info(currentNode.Path + "." + nodesToCheck.First() + " exists.");
                    }
                    TryGiveItems(currentNode[nodesToCheck.First()], nodesToCheck.Skip(1), player);
                }
                else if (debug)
                {
                    this.Info(currentNode.Path + "." + nodesToCheck.First() + " does not exist.");
                }
            }
            catch (Exception e)
            {
                this.Error("ERROR while checking if " + currentNode.Path + "." + nodesToCheck.First() + " node exists: \n" + e.ToString());
            }
        }
    }

    internal class ItemGivingHandler : IEventHandlerSpawn
    {
        private CustomLoadouts plugin;

        public ItemGivingHandler(CustomLoadouts plugin)
        {
            this.plugin = plugin;
        }

        public void OnSpawn(PlayerSpawnEvent ev)
        {
            // Only runs if not already running
            if (!plugin.spawning.Contains(ev.Player.SteamId))
            {
                plugin.spawning.Add(ev.Player.SteamId);
                new Task(async () =>
                {
                    // Delays execution until smod has created the object
                    await Task.Delay(500);
                    await Task.Delay(plugin.delay);

                    Player player = plugin.Server.GetPlayers(ev.Player.SteamId)[0];
                    if (player == null)
                    {
                        plugin.Warn("Could not find spawning player '" + ev.Player.Name + "', did they disconnect?");
                        plugin.spawning.Remove(ev.Player.SteamId);
                        return;
                    }
                    try
                    {
                        // The ternary statements just re-evaluate empty strings to "none"
                        plugin.TryGiveItems(plugin.config, new List<string> { player.SteamId == "" ? "none" : player.SteamId, player.GetRankName() == "" ? "none" : player.GetRankName(), player.TeamRole.Role.ToString() == "" ? "none" : player.TeamRole.Role.ToString() }, player);
                    }
                    catch (Exception e)
                    {
                        plugin.Error(e.ToString());
                    }
                    plugin.spawning.Remove(ev.Player.SteamId);
                }).Start();
            }
        }
    }

    internal class ReloadCommand : ICommandHandler
    {
        private CustomLoadouts plugin;

        public ReloadCommand(CustomLoadouts plugin)
        {
            this.plugin = plugin;
        }

        public string GetCommandDescription()
        {
            return "Reloads the config of CustomLoadouts";
        }

        public string GetUsage()
        {
            return "cl_reload";
        }

        public string[] OnCall(ICommandSender sender, string[] args)
        {
            plugin.Reload();
            return new string[] { "CustomLoadouts has been reloaded." };
        }
    }
}