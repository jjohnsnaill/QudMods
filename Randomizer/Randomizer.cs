using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using XRL;
using XRL.CharacterBuilds;
using XRL.CharacterBuilds.Qud;
using XRL.Rules;
using XRL.UI;
using XRL.World;

namespace Mods.Randomizer
{
	[HarmonyPatch]
	[HasCallAfterGameLoaded]
	public class Randomizer : AbstractEmbarkBuilderModule
	{
		private static Dictionary<GameObjectBlueprint, GameObjectBlueprint> swap;

		public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
		{
			if (id == QudGameBootModule.BOOTEVENT_AFTERGENERATESEEDS)
			{
				Swap();
			}
			return base.handleBootEvent(id, game, info, element);
		}

		[CallAfterGameLoaded]
		private static void Swap()
		{
			if (swap == null)
			{
				swap = new Dictionary<GameObjectBlueprint, GameObjectBlueprint>();
			}
			else
			{
				swap.Clear();
			}

			List<GameObjectBlueprint> creatures = new List<GameObjectBlueprint>(512);
			List<GameObjectBlueprint> cherubim = new List<GameObjectBlueprint>(32);
			List<GameObjectBlueprint> spawners = new List<GameObjectBlueprint>(4);
			List<GameObjectBlueprint> uniqueCreatures = Options.GetOption("RandomizeUniqueCreatures") == "Yes" ? creatures : new List<GameObjectBlueprint>(128);
			List<GameObjectBlueprint> items = new List<GameObjectBlueprint>(512);
			List<GameObjectBlueprint> uniqueItems = Options.GetOption("RandomizeUniqueItems") == "Yes" ? items : new List<GameObjectBlueprint>(128);
			List<GameObjectBlueprint> innateItems = new List<GameObjectBlueprint>(256);
			List<GameObjectBlueprint> food = new List<GameObjectBlueprint>(256);
			List<GameObjectBlueprint> cybernetics = new List<GameObjectBlueprint>(128);
			List<GameObjectBlueprint> walls = new List<GameObjectBlueprint>(256);
			List<GameObjectBlueprint> furniture = Options.GetOption("MixWallsFurniture") == "Yes" ? walls : new List<GameObjectBlueprint>(256);
			List<GameObjectBlueprint> obstacles = Options.GetOption("MixFurnitureObstacles") == "Yes" ? furniture : new List<GameObjectBlueprint>(256);
			List<GameObjectBlueprint> pools = new List<GameObjectBlueprint>(256);
			List<GameObjectBlueprint> gas = new List<GameObjectBlueprint>(128);
			List<GameObjectBlueprint> world = new List<GameObjectBlueprint>(256);

			List<GameObjectBlueprint> boringInnateItems = new List<GameObjectBlueprint>(128);
			List<GameObjectBlueprint> itemsOnly = new List<GameObjectBlueprint>(512);

			foreach (GameObjectBlueprint bp in GameObjectFactory.Factory.BlueprintList)
			{
				if ((bp.HasTag("BaseObject") && bp.Inherits != "NaturalWeapon") || bp.HasTag("Golem"))
					continue;

				string name = bp.DisplayName();
				if (name != null && name.StartsWith('[')
					&& !bp.HasTag("Book")
					&& !bp.HasPart("CultistSpawner")
					&& !bp.HasPart("ConvertSpawner")
					&& !bp.HasPart("PariahSpawner")
					&& !bp.HasPart("GuardSpawner")
					&& !bp.HasPart("CherubimSpawner"))
				{
					continue;
				}

				if (bp.InheritsFrom("Creature"))
				{
					if (bp.Name.EndsWith("Cherub"))
					{
						cherubim.Add(bp);
						continue;
					}

					if (Options.GetOption("AllCultists") == "Yes" && bp.HasPart("CultistSpawner"))
					{
						spawners.Add(bp);
						continue;
					}

					if (bp.HasTag("ExcludeFromDynamicEncounters"))
					{
						if (bp.HasPart("CherubimSpawner") || Options.GetOption("RandomizeUniqueCreatures") == "No")
							continue;

						uniqueCreatures.Add(bp);
					}

					if (Options.GetOption("AllCherubim") == "Yes" && bp.HasPart("CherubimSpawner"))
						spawners.Add(bp);

					if (Options.GetOption("RandomizeCreatures") == "Yes")
						creatures.Add(bp);
				}
				else if (bp.InheritsFrom("Food") || bp.Name.EndsWith("_Ingredient"))
				{
					if (Options.GetOption("RandomizeFood") == "Yes")
						food.Add(bp);
				}
				else if (bp.Name == "Campfire" || bp.InheritsFrom("Campfire") || bp.Name == "Campfire Remains" || bp.InheritsFrom("Campfire Remains"))
				{
					if (Options.GetOption("RandomizeFurniture") == "Yes")
						furniture.Add(bp);
				}
				else if (bp.InheritsFrom("Item"))
				{
					// DataDisk is required by Tinker I-III
					if (bp.Name == "DataDisk"
						|| bp.InheritsFrom("Projectile")
						|| bp.HasPart("EnergyCell")
						|| bp.HasPart("PointedAsteriskBuilder"))
					{
						continue;
					}

					if (bp.InheritsFrom("BaseCyberneticsEquipment"))
					{
						cybernetics.Add(bp);
						continue;
					}

					if (bp.HasProperty("Natural"))
					{
						if (bp.Name.StartsWith("Cherubic ") || bp.Name.StartsWith("Mechanical Cherubic ") || bp.Name.StartsWith("Metachrome "))
						{
							boringInnateItems.Add(bp);
							continue;
						}
						switch (Options.GetOption("RandomizeInnateItems"))
						{
							case "Yes":
								items.Add(bp);
								continue;
							case "Separate":
								innateItems.Add(bp);
								continue;
							default:
								continue;
						}
					}
					else if (bp.HasTag("ExcludeFromDynamicEncounters"))
					{
						if (Options.GetOption("RandomizeUniqueItems") != "No")
						{
							uniqueItems.Add(bp);
						}
						continue;
					}

					if (Options.GetOption("RandomizeItems") == "Yes")
						items.Add(bp);
				}

				else if (Options.GetOption("RandomizeWalls") == "Yes" && bp.InheritsFrom("Wall"))
					walls.Add(bp);

				else if (Options.GetOption("RandomizeFurniture") != "No" && bp.InheritsFrom("Furniture"))
					furniture.Add(bp);

				else if (Options.GetOption("RandomizeObstacles") == "Yes" && (bp.InheritsFrom("Plant")
					|| bp.InheritsFrom("Fungus")
					|| bp.InheritsFrom("Coral")
					|| bp.InheritsFrom("Palladium Strut")
					|| bp.InheritsFrom("Sponge")
					|| bp.InheritsFrom("Crystal")
					|| bp.InheritsFrom("Web")))
				{
					obstacles.Add(bp);
				}

				else if (Options.GetOption("RandomizePools") == "Yes" && bp.InheritsFrom("Water"))
					pools.Add(bp);

				else if (Options.GetOption("RandomizeGas") == "Yes" && bp.InheritsFrom("Gas"))
					gas.Add(bp);

				else if (Options.GetOption("RandomizeWorld") == "Yes" && bp.InheritsFrom("Terrain"))
					world.Add(bp);
			}

			itemsOnly.AddRange(items);

			if (Options.GetOption("AddFoodToItemPool") == "Yes")
				items.AddRange(food);

			if (Options.GetOption("AddCyberneticsToItemPool") == "Yes")
				items.AddRange(cybernetics);

			if (Options.GetOption("AddCreaturesToItemPool") == "Yes")
				items.AddRange(creatures);

			Random rand = Stat.GetSeededRandomGenerator("Randomizer");

			Swap(creatures, spawners.Count > 0 ? spawners : creatures, rand);

			if (uniqueCreatures != creatures)
				Swap(uniqueCreatures, Options.GetOption("RandomizeUniqueCreatures") == "Separate" ? uniqueCreatures : creatures, rand);

			if (Options.GetOption("RandomizeCherubim") == "Yes")
				Swap(cherubim, creatures.Count > 0 ? creatures : cherubim, rand);

			Swap(itemsOnly, items, rand);

			if (uniqueItems != items)
				Swap(uniqueItems, Options.GetOption("RandomizeUniqueItems") == "Separate" ? uniqueItems : items, rand);

			if (Options.GetOption("RandomizeInnateItems") == "Separate")
			{
				Swap(innateItems, innateItems, rand);
				Swap(boringInnateItems, innateItems, rand);
			}
			else
			{
				Swap(innateItems, items, rand);
				Swap(boringInnateItems, items, rand);
			}

			Swap(food, rand);
			Swap(cybernetics, rand);

			Swap(walls, rand);
			if (furniture != walls)
				Swap(furniture, rand);
			if (obstacles != furniture)
				Swap(obstacles, rand);

			Swap(pools, rand);
			Swap(gas, rand);
			Swap(world, rand);
		}

		private static void Swap(List<GameObjectBlueprint> list, Random rand)
		{
			Swap(list, list, rand);
		}

		private static void Swap(List<GameObjectBlueprint> listA, List<GameObjectBlueprint> listB, Random rand)
		{
			for (int i = 0; i < listA.Count; i++)
			{
				swap[listA[i]] = listB[rand.Next(listB.Count)];
			}
		}

		static MethodBase TargetMethod()
		{
			MethodInfo[] methods = typeof(GameObjectFactory).GetMethods();
			for (int i = 0; i < methods.Length; i++)
			{
				if (methods[i].Name == "CreateObject" && methods[i].GetParameters()[0].ParameterType == typeof(GameObjectBlueprint))
				{
					return methods[i];
				}
			}
			return null;
		}

		[HarmonyPrefix]
		static void Swap(ref GameObjectBlueprint Blueprint)
		{
			if (Blueprint != null && swap != null && swap.ContainsKey(Blueprint))
			{
				Blueprint = swap[Blueprint];
			}
		}
	}
}