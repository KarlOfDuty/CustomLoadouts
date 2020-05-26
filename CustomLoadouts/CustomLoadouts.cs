using CustomLoadouts.Properties;
using Newtonsoft.Json.Linq;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Commands;
using Smod2.Config;
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
		version = "3.0.1",
		SmodMajor = 3,
		SmodMinor = 7,
		SmodRevision = 0
	)]
	public class CustomLoadouts : Plugin
	{
		internal JObject loadouts;
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
			try
			{
				Reload();
			}
			catch(Exception e)
			{
				this.Error("Could not load config: " + e.ToString());
			}
		}

		public override void Register()
		{
			this.AddEventHandlers(new ItemGivingHandler(this), Priority.High);
			this.AddCommand("cl_reload", new ReloadCommand(this));
			this.AddConfig(new ConfigSetting("cl_global", true, true, "Whether or not to use the global config directory, default is true"));
		}

		public void Reload()
		{
			this.Info("Loading config '" + FileManager.GetAppFolder(true, !GetConfigBool("cl_global")) + "CustomLoadouts/config.yml'...");
			if (!Directory.Exists(FileManager.GetAppFolder(true, !GetConfigBool("cl_global")) + "CustomLoadouts"))
			{
				Directory.CreateDirectory(FileManager.GetAppFolder(true, !GetConfigBool("cl_global")) + "CustomLoadouts");
			}

			if (!File.Exists(FileManager.GetAppFolder(true, !GetConfigBool("cl_global")) + "CustomLoadouts/config.yml"))
			{
				File.WriteAllText(FileManager.GetAppFolder(true, !GetConfigBool("cl_global")) + "CustomLoadouts/config.yml", Encoding.UTF8.GetString(Resources.config));
			}

			// Reads file contents into FileStream
			FileStream stream = File.OpenRead(FileManager.GetAppFolder(true, !GetConfigBool("cl_global")) + "CustomLoadouts/config.yml");

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

			loadouts = json.SelectToken("customloadouts").Value<JObject>();
			this.Info("Config loaded.");
		}

		public void GiveItems(JToken roleLoadouts, Player player)
		{
			foreach (JObject itemGroupNode in roleLoadouts.Children())
			{
				// Converts the JObject to key/value pair
				JProperty itemGroup = itemGroupNode.Properties().First();

				// Attempts to parse the percentage chance from the config
				if (float.TryParse(itemGroup.Name, out float chance))
				{
					// Rolls a D100
					float d100 = rnd.Next(1, 10000) / 100.0f;

					// Success if dice roll is lower than the percentage chance
					if (chance >= d100)
					{
						if (debug)
						{
							this.Info(itemGroupNode.Path + ": Succeded random chance. " + chance + " >= " + d100);
						}

						// Gives all items in the item bundle to the player
						foreach (string itemName in itemGroup.Value as JArray)
						{
							switch (itemName.ToUpper())
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
											this.Info("Cleared ammo of " + player.TeamRole.Role + " " + player.Name + "(" + player.UserId + ").");
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
											this.Info("Cleared inventory of " + player.TeamRole.Role + " " + player.Name + "(" + player.UserId + ").");
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
											this.Info(player.TeamRole.Role + " " + player.Name + "(" + player.UserId + ") was given a mag of 5.56mm ammo (25 shots).");
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
											this.Info(player.TeamRole.Role + " " + player.Name + "(" + player.UserId + ") was given a mag of 7.62mm ammo (35 shots).");
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
											this.Info(player.TeamRole.Role + " " + player.Name + "(" + player.UserId + ") was given a clip of 9mm ammo (15 shots).");
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
										player.GiveItem((Smod2.API.ItemType)Enum.Parse(typeof(Smod2.API.ItemType), itemName));
										if (verbose)
										{
											this.Info(player.TeamRole.Role + " " + player.Name + "(" + player.UserId + ") was given item " + itemName + ".");
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
						this.Info(itemGroupNode.Path + ": Failed random chance. " + chance + " < " + d100);
					}
				}
				else
				{
					this.Error("Invalid chance: " + itemGroup.Name);
				}
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
            if (!plugin.spawning.Contains(ev.Player.UserId))
            {
                plugin.spawning.Add(ev.Player.UserId);
                new Task(async () =>
                {
                    // Delays execution until smod has created the object
                    await Task.Delay(500);
                    await Task.Delay(plugin.delay);

                    Player player = plugin.Server.GetPlayers(ev.Player.UserId)[0];
                    if (player == null)
                    {
                        plugin.Warn("Could not find spawning player '" + ev.Player.Name + "', did they disconnect?");
                        plugin.spawning.Remove(ev.Player.UserId);
                        return;
                    }
                    try
                    {
						// Check each registered permission node
						JProperty[] permissionNodes = plugin.loadouts.Properties().ToArray();
						foreach (JProperty permissionNode in permissionNodes)
						{
							if (player.HasPermission("customloadouts." + permissionNode.Name))
							{
								try
								{
									// Check if their role has a custom loadout registered
									JProperty[] roles = permissionNode.Value.Value<JObject>().Properties().ToArray();
									foreach (JProperty role in roles)
									{
										if (player.TeamRole.Role.ToString() == role.Name || role.Name == "all")
										{
											try
											{
												plugin.GiveItems(role.Value, ev.Player);
											}
											catch(Exception e)
											{
												plugin.Error("Error giving items: " + e.ToString());
											}
										}
									}
								}
								catch(Exception e)
								{
									plugin.Error("Error checking role: " + e.ToString());
								}
							}
						}
                    }
                    catch (Exception e)
                    {
                        plugin.Error("Error checking permission: " + e.ToString());
                    }
                    plugin.spawning.Remove(ev.Player.UserId);
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
			if (sender is Player player)
			{
				if (!player.HasPermission("customloadouts.reload"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}
			try
			{
				plugin.Reload();
			}
			catch (Exception e)
			{
				plugin.Error("Could not load config: " + e.ToString());
			}
			return new[] { "CustomLoadouts has been reloaded." };
        }
    }
}