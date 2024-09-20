using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders
{
	[HasGameBasedStaticCache]
	public class SkyEncounters : ZoneBuilderSandbox
	{
		public string Table = "Aleksh_SkyEncounters";
		public string TableHigh = "Aleksh_HighSkyEncounters";

		public bool BuildZone(Zone Z)
		{
			PlacePopulationInRegion(Z, Z.area, Z.Z < 8 ? TableHigh : Table, customFabricator: SpawnFlying);
			return true;
		}

		[GameBasedCacheInit]
		public static void AddBuilder()
		{
			AddBuilderToCell("DefaultJoppaCell", null, null);

			AddBuilderToCell("Mountains", null, null);

			AddBuilderToCell("SaltDesertCell", "Aleksh_SaltDesertSkyEncounters", null);
			AddBuilderToCell("SaltDesertCell2", "Aleksh_SaltDesertSkyEncounters", null);

			AddBuilderToCell("Jungle", null, null);
			AddBuilderToCell("RuinsCell", "Aleksh_RuinsSkyEncounters", null);

			AddBuilderToCell("DeepJungle", "Aleksh_DeepJungleSkyEncounters", null);

			AddBuilderToCell("BananaGrove", "Aleksh_BananaGroveSkyEncounters", null);
			AddBuilderToCell("Omonporch", "Aleksh_BananaGroveSkyEncounters", null);
			AddBuilderToCell("BananaGroveSpindleShadow", "Aleksh_BananaGroveSkyEncounters", null);
			AddBuilderToCell("BananaGroveSpindleShadow2", "Aleksh_BananaGroveSkyEncounters", null);

			AddBuilderToCell("Golgotha", "Aleksh_GolgothaSkyEncounters", null);

			AddBuilderToCell("LakeHinnomCell", "Aleksh_LakeHinnomSkyEncounters", null);
			AddBuilderToCell("PalladiumReefCell", "Aleksh_LakeHinnomSkyEncounters", null);

			AddBuilderToCell("MoonStairCell", "Aleksh_MoonStairSkyEncounters", "Aleksh_MoonStairSkyEncounters");
		}

		public static void AddBuilderToCell(string name, string table, string tableHigh)
		{
			CellBlueprint cell = WorldFactory.Factory.getWorld("JoppaWorld").CellBlueprintsByName[name];

			OrderedBuilderBlueprint bp;
			if (table != null)
			{
				if (tableHigh != null)
				{
					bp = new OrderedBuilderBlueprint(new ZoneBuilderBlueprint("SkyEncounters")
					{
						Parameters = new Dictionary<string, object>()
						{
							["Table"] = table,
							["TableHigh"] = tableHigh
						}
					}, 3999);
				}
				else
				{
					bp = new OrderedBuilderBlueprint(new ZoneBuilderBlueprint("SkyEncounters")
					{
						Parameters = new Dictionary<string, object>()
						{
							["Table"] = table
						}
					}, 3999);
				}
			}
			else if (tableHigh != null)
			{
				bp = new OrderedBuilderBlueprint(new ZoneBuilderBlueprint("SkyEncounters")
				{
					Parameters = new Dictionary<string, object>()
					{
						["TableHigh"] = tableHigh
					}
				}, 3999);
			}
			else
			{
				bp = new OrderedBuilderBlueprint(new ZoneBuilderBlueprint("SkyEncounters"), 3999);
			}

			for (int i = 0; i < 10; i++)
			{
				bool add = true;
				foreach (ZoneBuilderBlueprint bp2 in cell.LevelBlueprint[1, 1, i].Builders)
				{
					if (bp2.Class == "SkyEncounters")
					{
						add = false;
					}
				}
				if (add)
				{
					cell.LevelBlueprint[1, 1, i].Builders.Add(bp);
				}
			}
		}

		public GameObject SpawnFlying(string bp)
		{
			GameObject obj = GameObject.Create(bp);
			obj.AddPart(new SpawnFlying());

			return obj;
		}
	}
}