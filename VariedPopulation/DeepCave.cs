using HarmonyLib;
using System.Collections.Generic;
using XRL.World;
using XRL.World.ZoneFactories;

namespace Mods.VariedPopulation
{
	[HarmonyPatch]
	public class DeepCave
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(JoppaWorldZoneFactory), "AddBlueprintsFor")]
		static bool AddBlueprintsFor(ZoneRequest Request)
		{
			if (Request.Z < 61)
			{
				return true;
			}

			ZoneBlueprint bp = new ZoneBlueprint(WorldFactory.Factory.getWorld("JoppaWorld").CellBlueprintsByName["DefaultJoppaCell"].LevelBlueprint[1, 1, 49]);
			bp.Builders.Remove("ZoneTemplate:Caves");

			ZoneBuilderBlueprint template = new ZoneBuilderBlueprint("Population")
			{
				Parameters = new Dictionary<string, object>()
				{
					["Table"] = "Aleksh_DeepCaves"
				}
			};
			bp.Builders.Add(new OrderedBuilderBlueprint(template, 3999));

			Request.Blueprints.Add(bp);

			return false;
		}
	}
}