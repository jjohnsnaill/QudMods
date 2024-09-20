using HarmonyLib;
using System.Collections.Generic;
using XRL;
using XRL.World;
using XRL.World.WorldBuilders;
using XRL.World.ZoneFactories;

namespace Mods.VariedPopulation
{
	[HarmonyPatch]
	[JoppaWorldBuilderExtension]
	public class WorldAdditions : IJoppaWorldBuilderExtension
	{
		public override void OnAfterBuild(JoppaWorldBuilder Builder)
		{
			var location = Builder.popMutableLocationOfTerrain("Saltdunes", centerOnly: false);
			var id = Builder.ZoneIDFromXY("JoppaWorld", location.X, location.Y);

			var secret = Builder.AddSecret(id, "the location of Lad", new string[2] { "lair", "oddity" }, "Lairs", "$aleksh_lad");

			GameObject obj = GameObject.Create("Aleksh_Lad");
			The.ZoneManager.AddZonePostBuilder(id, "AddObjectBuilder", "Object", The.ZoneManager.CacheObject(obj));

			The.ZoneManager.AddZonePostBuilder(ZoneID.Assemble("JoppaWorld", 76, 5, 1, 1, 7), "OsefPortal");
		}

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
			bp.Builders.Remove("Music");

			ZoneBuilderBlueprint template = new ZoneBuilderBlueprint("Population")
			{
				Parameters = new Dictionary<string, object>()
				{
					["Table"] = "Aleksh_DeepCaves"
				}
			};
			bp.Builders.Add(new OrderedBuilderBlueprint(template, 3999));
			bp.Builders.Add(new OrderedBuilderBlueprint(new ZoneBuilderBlueprint("Music"), 3999));

			Request.Blueprints.Add(bp);

			return false;
		}
	}
}