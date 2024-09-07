using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.UI;
using XRL.World.Biomes;
using XRL.World.Parts;
using XRL.World.WorldBuilders;
using XRL.World;

namespace Mods.Randomizer
{
	[HarmonyPatch]
	public class Hooks
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(GameObjectFactory), "GetFactionMembers")]
		static void FactionFix(GameObjectFactory __instance, ref string Faction, ref List<GameObjectBlueprint> __result)
		{
			// include unique characters in the list if it is empty, or the game will fail to start
			if (__result.Count < 1)
			{
				foreach (GameObjectBlueprint blueprint in __instance.BlueprintList)
				{
					if (blueprint.Tags.ContainsKey("BaseObject"))
						continue;

					GamePartBlueprint part = blueprint.GetPart("Brain");
					if (part == null || !part.TryGetParameter("Factions", out string value) || !value.Contains(Faction))
						continue;

					foreach (string item in value.CachedCommaExpansion())
					{
						if (item.StartsWith(Faction) && item[Faction.Length] == '-')
						{
							__result.Add(blueprint);
							break;
						}
					}
				}
			}
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(CherubimSpawner), "HandleEvent")]
		// only affects random cherubim
		static IEnumerable<CodeInstruction> CherubFix(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Callvirt, typeof(GameObject).GetMethod("HasTag")))
				{
					list.Insert(++i, CodeInstruction.Call(typeof(Hooks), "CheckRandomizeCherubim"));
				}
				else if (list[i].Is(OpCodes.Callvirt, typeof(GameObject).GetMethod("GetTag")))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Pop));
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldstr, "being"));
					i += 2;
				}
			}
			return list;
		}

		private static bool CheckRandomizeCherubim(bool alt)
		{
			if (Options.GetOption("RandomizeCherubim") == "Yes")
				return true;

			return alt;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PsychicBiome), "CreateSeedMap")]
		// only affects random world map
		static IEnumerable<CodeInstruction> PsychicBiomeFix(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(GameObject).GetMethod("get_CurrentCell")))
				{
					list.Insert(i + 1, CodeInstruction.Call(typeof(Hooks), "CheckRandomizeWorld"));
					break;
				}
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(JoppaWorldBuilder), "BuildWorld")]
		// only affects random world map
		static IEnumerable<CodeInstruction> BuildWorldFix(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldstr, "Yes"))
				{
					list.Insert(i, CodeInstruction.Call(typeof(Hooks), "CheckRandomizeWorld"));
					break;
				}
			}
			return list;
		}

		private static object CheckRandomizeWorld(object obj)
		{
			if (Options.GetOption("RandomizeWorld") == "Yes")
				return null;

			return obj;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(Options), "UpdateFlags")]
		static IEnumerable<CodeInstruction> Achievements(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(Options).GetMethod("get_DisableAchievements")))
				{
					list.Insert(i + 3, new CodeInstruction(OpCodes.Pop));
					list.Insert(i + 4, new CodeInstruction(OpCodes.Ldc_I4_0));
					break;
				}
			}
			return list;
		}
	}
}