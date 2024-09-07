using XRL;
using XRL.World;
using XRL.World.Parts;
using System.Collections.Generic;
using SimpleJSON;
using XRL.EditorFormats.Map;
using System;
using System.IO;
using XRL.Core;
using Genkit;
using XRL.World.Tinkering;
using XRL.World.Parts.Skill;
using XRL.World.Parts.Mutation;
using XRL.World.Effects;
using XRL.World.Skills.Cooking;
using XRL.UI;
using System.Reflection;
using XRL.Messages;
using XRL.World.Quests.GolemQuest;
using System.Text;
using XRL.World.AI;

namespace Mods.Gauntlet
{
	public class GauntletSystem : IGameSystem
	{
		[NonSerialized]
		public Zone zone;
		[NonSerialized]
		public int wave;
		[NonSerialized]
		public bool complete;

		[NonSerialized]
		public List<WaveDefinition> waves;
		[NonSerialized]
		public List<Layout> layouts;

		public GauntletSystem()
		{
			waves = new List<WaveDefinition>();
			layouts = new List<Layout>();

			ModManager.ForEachFile("Gauntlet.json", (path, mod) =>
			{
				using (StreamReader streamReader = DataManager.GetStreamingAssetsStreamReader(path))
				{
					JSONNode root = JSON.Parse(streamReader.ReadToEnd());
					foreach (KeyValuePair<string, JSONNode> node in (root["Waves"] as JSONClass).ChildNodes)
					{
						if (node.Value is JSONClass parent)
						{
							AddWave(parent);
						}
					}
					foreach (KeyValuePair<string, JSONNode> node in (root["Layouts"] as JSONClass).ChildNodes)
					{
						if (node.Value is JSONClass parent)
						{
							AddLayout(parent);
						}
					}
				}
			});
		}

		public override void Register(XRLGame Game, IEventRegistrar Registrar)
		{
			Registrar.Register(ZoneActivatedEvent.ID);
			Registrar.Register(EndTurnEvent.ID);
		}

		public override void Write(SerializationWriter Writer)
		{
			Writer.Write(complete ? (short)(wave + short.MinValue) : (short)wave);
		}

		public override void Read(SerializationReader Reader)
		{
			wave = Reader.ReadInt16();
			if (wave < 0)
			{
				wave -= short.MinValue;
				complete = true;
			}
		}

		public override bool HandleEvent(ZoneActivatedEvent E)
		{
			if (this.zone == null)
			{
				this.zone = XRLCore.Core?.Game?.ZoneManager?.GetZone("Gauntlet.40.12.1.1.10");
			}

			foreach (GameObject obj in zone.GetObjects())
			{
				GauntletCell.SetGauntletProperties(obj);
			}

			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EndTurnEvent E)
		{
			if (complete)
			{
				return base.HandleEvent(E);
			}

			foreach (GameObject obj in zone.GetObjects())
			{
				if (!obj.IsPlayer() && !obj.IsPlayerLed() && !obj.IsTemporary && obj.GetIntProperty("GauntletObject") == 1)
				{
					return base.HandleEvent(E);
				}
			}

			complete = true;

			if (Player.Level < 30)
			{
				Player.AwardXP(Leveler.GetXPForLevel(Player.Level + 1) - Player.GetStatValue("XP"));
			}

			if (wave >= 24)
			{
				SoundManager.PlayMusic("Imminent II", CrossfadeDuration: 1);
			}

			foreach (GameObject obj in zone.GetObjects())
			{
				if (!obj.IsPlayer() && (!obj.IsPlayerLed() || obj.IsTemporary) && obj.GetIntProperty("GauntletObject") < 2)
				{
					obj.Obliterate();
					continue;
				}

				Clear(obj);

				obj.RestorePristineHealth();
				RestoreItems(obj);

				if (obj.GetPart("ActivatedAbilities") is ActivatedAbilities abilities && abilities.AbilityByGuid != null)
				{
					foreach (ActivatedAbilityEntry ability in abilities.AbilityByGuid.Values)
					{
						ability.Refresh();
					}
				}
			}

			WaveEnd();

			return base.HandleEvent(E);
		}

		public void AddWave(JSONClass root)
		{
			List<ObjectDefinition> objects = new List<ObjectDefinition>();

			if (root["Objects"] is JSONArray list)
			{
				AddObject(list, objects);
			}

			if (objects.Count < 1)
			{
				return;
			}

			waves.Add(new WaveDefinition(root["MinWave"].AsInt, root["Wave"].AsInt, objects, root["Layout"] is JSONData ? root["Layout"].Value : null));
		}

		public void AddObject(JSONArray list, List<ObjectDefinition> objects)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is JSONClass obj)
				{
					ObjectDefinition definition = new ObjectDefinition();

					definition.blueprint = obj["Object"].Value;
					definition.count = Math.Max(obj["Count"].AsInt, 1);

					definition.x = obj["X"] is JSONData x ? x.AsInt : -1;
					definition.y = obj["Y"] is JSONData y ? y.AsInt : -1;

					ObjectTraits traits = new ObjectTraits();
					if (obj["Traits"] is JSONArray traitList)
					{
						for (int j = 0; j < traitList.Count; j++)
						{
							switch (traitList[j].Value)
							{
								case "Special": traits.special = true; break;
								case "Optional": traits.optional = true; break;
								case "Neutral": traits.neutral = true; break;
								case "Animate": traits.animate = true; break;
								case "Bomb": traits.bomb = true; break;
								case "Naked": traits.naked = true; break;
								case "Phased": traits.phased = true; break;
								case "Omniphase": traits.omniphase = true; break;
								case "MaxWillpower": traits.maxWillpower = true; break;
								case "Hologram": traits.hologram = true; break;
							}
						}
					}
					definition.traits = traits;

					List<MutationDefinition> mutations = null;
					if (obj["Mutations"] is JSONArray mutationList)
					{
						mutations = new List<MutationDefinition>();
						for (int j = 0; j < mutationList.Count; j++)
						{
							JSONNode entry = mutationList[j];

							MutationDefinition mutation = new MutationDefinition();
							mutation.type = entry["Type"];
							mutation.level = entry["Level"] is JSONData ? entry["Level"].AsInt : 1;
							mutation.variant = entry["Variant"] is JSONData ? entry["Variant"].Value : null;
							mutations.Add(mutation);
						}
					}
					definition.mutations = mutations;

					List<Type> skills = null;
					if (obj["Skills"] is JSONArray skillList)
					{
						skills = new List<Type>();
						for (int j = 0; j < skillList.Count; j++)
						{
							skills.Add(ModManager.ResolveType("XRL.World.Parts.Skill." + skillList[j].Value));
						}
					}
					definition.skills = skills;

					List<Type> parts = null;
					if (obj["Parts"] is JSONArray partList)
					{
						parts = new List<Type>();
						for (int j = 0; j < partList.Count; j++)
						{
							parts.Add(ModManager.ResolveType("XRL.World.Parts." + partList[j].Value));
						}
					}
					definition.parts = parts;

					List<Type> mods = null;
					if (obj["Mods"] is JSONArray modList)
					{
						mods = new List<Type>();
						for (int j = 0; j < modList.Count; j++)
						{
							mods.Add(ModManager.ResolveType("XRL.World.Parts." + modList[j].Value));
						}
					}
					definition.mods = mods;

					List<ObjectDefinition> inventory = null;
					if (obj["Objects"] is JSONArray inventoryList)
					{
						inventory = new List<ObjectDefinition>();
						AddObject(inventoryList, inventory);

						if (inventory.Count < 1)
						{
							inventory = null;
						}
					}
					definition.objects = inventory;

					if (!traits.special && !GameObjectFactory.Factory.HasBlueprint(obj["Object"].Value))
					{
						continue;
					}
					objects.Add(definition);
				}
			}
		}

		public void AddLayout(JSONClass root)
		{
			Layout layout = new Layout();
			layout.map = MapFile.LoadWithMods(root["Map"].Value);
			layout.minWave = root["MinWave"].AsInt;
			layout.maxWave = root["MaxWave"].AsInt;
			layout.tag = root["Tag"] is JSONData tag ? tag.Value : null;

			List<ObjectDefinition> objects = new List<ObjectDefinition>();
			if (root["Objects"] is JSONArray list)
			{
				AddObject(list, objects);
			}
			layout.objects = objects;

			layouts.Add(layout);
		}

		public void Clear(GameObject obj)
		{
			foreach (GameObject item in obj.GetContents())
			{
				if (item.GetIntProperty("GauntletObject", 2) < 2) // prevents starting items from being lost
				{
					item.Obliterate();
				}
			}
		}

		public void RestoreItems(GameObject obj)
		{
			RestoreItem(obj);

			List<GameObject> list = obj.GetInventoryAndEquipment();
			for (int i = 0; i < list.Count; i++)
			{
				RestoreItem(list[i]);
			}
		}

		public void RestoreItem(GameObject obj)
		{
			RepairedEvent.Send(Subject: obj);

			for (int i = 0; i < obj.PartsList.Count; i++)
			{
				if (obj.PartsList[i] is IEnergyCell cell)
				{
					cell.MaximizeCharge();
				}
			}

			EnergyCellSocket socket = obj.GetPart<EnergyCellSocket>();
			if (socket != null && socket.Cell != null)
			{
				RestoreItem(socket.Cell);
			}
		}

		public void Advance()
		{
			foreach (GameObject obj in zone.GetObjects())
			{
				if (!obj.IsPlayer() && (!obj.IsPlayerLed() || obj.IsTemporary))
				{
					if (obj.GetIntProperty("GauntletObject") < 3)
					{
						obj.Obliterate();
					}
					continue;
				}

				Clear(obj);

				List<GameObject> list = obj.Body?.GetEquippedObjects();
				for (int i = 0; i < list.Count; i++)
				{
					GauntletUtils.RestockAmmo(obj, list[i], zone.NewTier);
				}
			}

			if (The.Player.HasSkill("CookingAndGathering"))
			{
				List<string> recipeNames = new List<string>();
				List<CookingRecipe> recipes = new List<CookingRecipe>();
				foreach (CookingRecipe recipe in CookingGameState.instance.knownRecipies)
				{
					if (recipe.Hidden || (The.Player.HasPart("Carnivorous") && (recipe.HasPlants() || recipe.HasFungi())))
					{
						continue;
					}

					recipes.Add(recipe);
					recipeNames.Add(recipe.GetDisplayName() + "\n" + recipe.GetDescription());
				}

				if (recipes.Count > 0)
				{
					int index = Popup.PickOption("Choose a recipe", Options: recipeNames, RespectOptionNewlines: true, AllowEscape: true);
					if (index >= 0 && index < recipes.Count)
					{
						The.Player.FireEvent("ClearFoodEffects");
						The.Player.CleanEffects();
						recipes[index].ApplyEffectsTo(The.Player);
					}
				}
			}

			if (The.Player.HasSkill("Tinkering_Disassemble"))
			{
				Random bitRand = new Random(Hash.String(XRLCore.Core.Game.GetWorldSeed() + "GauntletBit" + wave));

				char[] chars = new char[3];
				chars[0] = BitType.LevelMap[0][bitRand.Next(BitType.LevelMap[0].Count)].Color;
				chars[1] = BitType.LevelMap[0][bitRand.Next(BitType.LevelMap[0].Count)].Color;
				chars[2] = BitType.LevelMap[zone.NewTier][bitRand.Next(BitType.LevelMap[zone.NewTier].Count)].Color;

				The.Player.GetPart<BitLocker>().AddBits(new string(chars));
			}

			zone.Built = false;

			wave++;
			complete = false;

			Random rand = new Random(Hash.String(XRLCore.Core.Game.GetWorldSeed() + "Gauntlet" + wave));

			WaveDefinition waveDefinition = GetWave(wave, rand);

			List<Layout> layoutSelection = new List<Layout>();
			for (int i = 0; i < layouts.Count; i++)
			{
				if (layouts[i].tag == waveDefinition.layout && layouts[i].minWave <= wave && (layouts[i].maxWave < 1 || layouts[i].maxWave >= wave))
				{
					layoutSelection.Add(layouts[i]);
				}
			}
			if (layoutSelection.Count > 0)
			{
				Layout layout = layoutSelection[rand.Next(layoutSelection.Count)];
				for (int j = 0; j < layout.map.height; j++)
				{
					for (int i = 0; i < layout.map.width; i++)
					{
						List<MapFileObjectBlueprint> objects = layout.map.Cells[i, j].Objects;
						for (int k = 0; k < objects.Count; k++)
						{
							GameObject obj = GameObjectFactory.Factory.CreateObject(objects[k].Name);
							obj.IntProperty["GauntletObject"] = 0;
							zone.GetCell(i, j).AddObject(obj);
						}
					}
				}

				for (int i = 0; i < layout.objects.Count; i++)
				{
					Spawn(layout.objects[i], rand);
				}
			}

			if (wave == 1)
			{
				MessageQueue.AddPlayerMessage("{{G|Tip}}:\nNot claiming any items from reliquaries earns you tokens, which you can spend at the baetyl.");
			}

			List<Cell> cells = GetCells(Player);
			if (cells.Count < 1)
			{
				Player.TeleportTo(zone.GetCell(40, 12));
			}
			else
			{
				Player.TeleportTo(cells[rand.Next(cells.Count)]);
			}

			for (int i = 0; i < waveDefinition.objects.Count; i++)
			{
				Spawn(waveDefinition.objects[i], rand);
			}

			zone.Built = true;
			zone.DisplayName = "The Gauntlet, wave " + wave;

			ZoneManager.PaintWalls(zone);
			ZoneManager.PaintWater(zone);

			if (wave >= 25)
			{
				SoundManager.PlayMusic(wave >= 40 ? "Arrival of the Official Party" : "Battle at Grit Gate", CrossfadeDuration: 1);
			}
		}

		public WaveDefinition GetWave(int wave, Random rand)
		{
			List<WaveDefinition> waveSelection = new List<WaveDefinition>();

			int highest = 0;
			bool set = false;

			for (int i = 0; i < waves.Count; i++)
			{
				if (waves[i].minWave > wave)
				{
					continue;
				}
				if (waves[i].wave == wave)
				{
					set = true;
					break;
				}
				if (waves[i].minWave > highest)
				{
					highest = waves[i].minWave;
				}
			}

			if (set)
			{
				for (int i = 0; i < waves.Count; i++)
				{
					if (waves[i].minWave <= wave && waves[i].wave == wave)
					{
						waveSelection.Add(waves[i]);
					}
				}

				return waveSelection[rand.Next(waveSelection.Count)];
			}

			for (int i = 0; i < waves.Count; i++)
			{
				if (waves[i].minWave == highest && waves[i].wave == 0)
				{
					waveSelection.Add(waves[i]);
				}
			}

			return waveSelection[rand.Next(waveSelection.Count)];
		}

		public GameObject CreateAndAddParts(string blueprint, ObjectDefinition obj)
		{
			return GameObjectFactory.Factory.CreateObject(blueprint, (i) => { GauntletUtils.AddParts(i, obj.parts); });
		}

		public void Spawn(ObjectDefinition obj, Random rand)
		{
			for (int i = 0; i < obj.count; i++)
			{
				GameObject unit = null;

				if (obj.traits.special)
				{
					if (obj.blueprint == "EvilTwin")
					{
						EvilTwin.CreateEvilTwin(The.Player, EvilTwin.DEFECT_PREFIX);
						continue;
					}
					else if (obj.blueprint == "Nephilim")
					{
						List<string> nephilim = new List<string>(NephalProperties.Nephilim.Length);
						for (int j = 0; j < NephalProperties.Nephilim.Length; j++)
						{
							if (!The.Game.HasDelimitedGameState(NephalProperties.Nephilim[j], ',', "Dead"))
							{
								nephilim.Add(NephalProperties.Nephilim[j]);
							}
						}

						if (nephilim.Count > 0)
						{
							unit = CreateAndAddParts(nephilim[rand.Next(nephilim.Count)], obj);
						}
						else
						{
							unit = CreateAndAddParts(NephalProperties.Nephilim[rand.Next(NephalProperties.Nephilim.Length)], obj);
						}
					}
					else if (obj.blueprint == "Golem")
					{
						GolemQuestSystem.Require();

						unit = CreateAndAddParts(GolemBodySelection.GetBodyBySpecies().GetRandomElement(rand).Value, obj);

						unit.RemovePart("Interior");
						unit.RemovePart("Vehicle");
						unit.RemoveEffect<Unpiloted>();

						Description desc = unit.GetPart<Description>();
						StringBuilder descSB = Event.NewStringBuilder(desc._Short);
						RulesDescription rules = unit.RequirePart<RulesDescription>();

						GolemBodySelection body = new GolemBodySelection() { Material = unit };
						body.Apply(unit);
						body.VariableReplace(unit, descSB);

						// initialize Units
						new GolemCatalystSelection();

						GauntletUtils.AddCatalyst(unit, descSB, rules, rand);
						GauntletUtils.AddAtzmus(unit, descSB, rules, rand);

						string[] zetachrome = new string[]
						{
							"Cudgel8",
							"Dagger8",
							"Battle Axe8",
							"Long Sword8"
						};

						GolemArmamentSelection armament = new GolemArmamentSelection() { Material = GameObjectFactory.Factory.CreateObject(zetachrome.GetRandomElement(rand)) };
						armament.Apply(unit);
						armament.VariableReplace(unit, descSB);

						GauntletUtils.AddIncantation(unit, descSB, rules, rand);
						GauntletUtils.AddHamsa(unit, descSB, rules, rand);

						desc._Short = descSB.ToString();
					}

					if (unit == null)
					{
						continue;
					}
				}
				else
				{
					unit = CreateAndAddParts(obj.blueprint, obj);
				}

				if (obj.traits.bomb)
				{
					unit = Tinkering_LayMine.CreateBomb(unit, Countdown: obj.blueprint == "HandENuke" ? 10 : 3);
				}

				if (obj.traits.animate)
				{
					AnimateObject.Animate(unit);
				}

				if (obj.traits.hologram)
				{
					unit.AddPart<HologramMaterial>();
					unit.AddPart<HologramInvulnerability>();
				}

				if (obj.traits.naked)
				{
					unit.StripContents(true, true);
				}

				if (obj.traits.maxWillpower)
				{
					Statistic willpower = unit.GetStat("Willpower");
					if (willpower.BaseValue < 29)
					{
						willpower.BaseValue = 32;
					}
					else
					{
						willpower.BaseValue += 4;
					}
				}

				if (obj.traits.phased)
				{
					unit.ApplyEffect(new Phased(9999));
				}
				if (obj.traits.omniphase)
				{
					unit.ApplyEffect(new Omniphase(9999));
				}

				unit.IntProperty["GauntletObject"] = obj.traits.Ignore ? 0 : 1;

				GauntletUtils.AddMutations(unit, obj.mutations);
				GauntletUtils.AddSkills(unit, obj.skills);
				GiveItems(unit, obj.objects);

				unit.AddPart<GauntletPart>();
				unit.RemovePart<GivesRep>();

				if (!obj.traits.neutral && unit.Brain != null)
				{
					unit.Brain.Factions = "Mean-100,Playerhater-99";
					unit.Brain.AddOpinion<OpinionInscrutable>(The.Player);
				}

				if (unit.Statistics.TryGetValue("XPValue", out Statistic xp))
				{
					xp.BaseValue = 0;
				}

				if (obj.x > -1 && obj.y > -1)
				{
					zone.GetCell(obj.x, obj.y).AddObject(unit);
					unit.TeleportSwirl();

					continue;
				}

				List<Cell> cells = GetCells(unit);
				if (cells.Count < 1)
				{
					continue;
				}

				cells[rand.Next(cells.Count)].AddObject(unit);
				unit.TeleportSwirl();
			}
		}

		public List<Cell> GetCells(GameObject unit)
		{
			List<Cell> cells = new List<Cell>();
			for (int j = 0; j < zone.Height; j++)
			{
				for (int i = 0; i < zone.Width; i++)
				{
					Cell cell = zone.Map[i][j];

					if ((unit.Brain != null && unit.Brain.Aquatic) ? !cell.HasWadingDepthLiquid() : cell.HasWadingDepthLiquid())
					{
						continue;
					}

					if (unit.Brain != null && unit.Brain.LivesOnWalls)
					{
						bool valid = false;
						for (int k = 0; k < cell.Objects.Count; k++)
						{
							if (cell.Objects[k].GetIntProperty("GauntletObject") > 2)
							{
								continue;
							}

							if (cell.Objects[k].IsWall())
							{
								valid = true;
								continue;
							}

							valid = false;
							break;
						}
						if (valid && cell.GetFirstEmptyAdjacentCell() != null)
						{
							cells.Add(cell);
						}
					}
					else if (cell.IsEmptyOfSolid())
					{
						cells.Add(cell);
					}
				}
			}
			return cells;
		}

		public void GiveItems(GameObject unit, List<ObjectDefinition> items)
		{
			if (items == null)
			{
				return;
			}

			for (int i = 0; i < items.Count; i++)
			{
				for (int j = 0; j < items[i].count; j++)
				{
					GameObject item;
					if (items[i].mods != null)
					{
						item = GameObjectFactory.Factory.CreateUnmodifiedObject(items[i].blueprint);
						for (int k = 0; k < items[i].mods.Count; k++)
						{
							ItemModding.ApplyModification(item, (IModification)Activator.CreateInstance(items[i].mods[k], zone.NewTier), Creation: true);
						}
					}
					else
					{
						item = GameObjectFactory.Factory.CreateObject(items[i].blueprint);
					}

					item.IntProperty["GauntletObject"] = 0;

					if (items[i].objects != null)
					{
						GiveItems(item, items[i].objects);
					}

					unit.GetPart<Inventory>().AddObject(item);
				}
			}

			unit.Brain?.PerformReequip(true, false);
		}

		public void WaveEnd()
		{
			int tier = (wave + 1) / 5;
			if (tier > 8)
			{
				tier = 8;
			}
			else if (tier < 1)
			{
				tier = 1;
			}
			typeof(Zone).GetField("_NewTier", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(zone, tier); // bruh
			ZoneManager.zoneGenerationContextTier = tier;

			bool spawnTerminal = Player.IsTrueKin();

			GameObject baetyl = GameObjectFactory.Factory.CreateObject("Baetyl");
			baetyl.IntProperty["GauntletObject"] = 0;
			baetyl.AddPart<NoDamage>();
			baetyl.AddPart<GauntletBaetyl>();
			zone.GetCell(spawnTerminal ? 39 : 40, 11).AddObject(baetyl);
			baetyl.TeleportSwirl();

			GameObject reliquary = GameObjectFactory.Factory.CreateObject("Reliquary");
			reliquary.IntProperty["GauntletObject"] = 0;
			reliquary.RemovePart<Container>();
			reliquary.RemovePart<AICryptHelpBroadcaster>();

			reliquary.AddPart<NoDamage>();
			reliquary.AddPart<GauntletContainer>();

			zone.GetCell(40, 13).AddObject(reliquary);
			reliquary.TeleportSwirl();

			if (spawnTerminal)
			{
				GameObject terminal = GameObjectFactory.Factory.CreateObject("CyberneticsTerminal2");
				terminal.IntProperty["GauntletObject"] = 0;
				terminal.AddPart<NoDamage>();
				zone.GetCell(41, 11).AddObject(terminal);
				terminal.TeleportSwirl();
			}
		}
	}

	public struct WaveDefinition
	{
		public int minWave;
		public int wave;
		public List<ObjectDefinition> objects;
		public string layout;

		public WaveDefinition(int minWave, int wave, List<ObjectDefinition> objects, string layout)
		{
			this.minWave = minWave;
			this.wave = wave;
			this.objects = objects;
			this.layout = layout;
		}
	}

	public struct ObjectDefinition
	{
		public string blueprint;
		public int count;
		public int x;
		public int y;
		public ObjectTraits traits;
		public List<MutationDefinition> mutations;
		public List<Type> skills;
		public List<Type> parts;
		public List<Type> mods;
		public List<ObjectDefinition> objects;
	}

	public struct MutationDefinition
	{
		public string type;
		public int level;
		public string variant;
	}

	public struct ObjectTraits
	{
		public bool special;
		public bool optional;
		public bool neutral;
		public bool animate;
		public bool bomb;
		public bool naked;
		public bool phased;
		public bool omniphase;
		public bool maxWillpower;
		public bool hologram;

		public bool Ignore => optional || neutral || hologram;
	}

	public struct Layout
	{
		public MapFile map;
		public int minWave;
		public int maxWave;
		public string tag;
		public List<ObjectDefinition> objects;
	}
}