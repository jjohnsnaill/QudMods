using HarmonyLib;
using System.Collections.Generic;
using XRL.World;
using XRL.World.Parts;

namespace Mods.UnitedFactions
{
	[HarmonyPatch]
	public class MergeFactions
	{
		public static Dictionary<string, string> MergedFactions = new Dictionary<string, string>()
		{
			["Baboons"] = "Apes",
			["Antelopes"] = "Prey",
			["Equines"] = "Prey",
			["Winged Mammals"] = "Birds",
			["Urchins"] = "Fish",
			["Trees"] = "Roots",
			["Vines"] = "Roots",
			["Succulents"] = "Flowers",
			["Trolls"] = "Hermits"
		};

		//TODO: fix plant-based cooking effects adding reputation multiple times
		//TODO: pick random cherubim

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Factions), "AddNewFaction")]
		static bool TryAddFactionFeeling(Faction NewFaction)
		{
			return !MergedFactions.ContainsKey(NewFaction.Name);
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Factions), "Get")]
		static void Get(ref string Name)
		{
			if (MergedFactions.TryGetValue(Name, out string replaced))
			{
				Name = replaced;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Factions), "GetIfExists")]
		static void GetIfExists(ref string Name)
		{
			if (MergedFactions.TryGetValue(Name, out string replaced))
			{
				Name = replaced;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(GameObjectFactory), "GetFactionMembers")]
		static void GetFactionMembers(ref string Faction)
		{
			if (MergedFactions.TryGetValue(Faction, out string replaced))
			{
				Faction = replaced;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Faction), "TryAddFactionFeeling")]
		static bool TryAddFactionFeeling(string Faction, ref bool __result)
		{
			if (MergedFactions.ContainsKey(Faction))
			{
				__result = true;
				return false;
			}
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Brain), "FillFactionMembership")]
		static void FillFactionMembership(IDictionary<string, int> Map)
		{
			foreach (var faction in MergedFactions)
			{
				ReplaceFaction(faction.Key, faction.Value, Map);
			}
		}

		public static void ReplaceFaction(string faction, string replacement, IDictionary<string, int> map)
		{
			if (!map.TryGetValue(faction, out int rep))
			{
				return;
			}

			map.Remove(faction);
			if (!map.TryGetValue(replacement, out _))
			{
				map.Add(replacement, rep);
			}
		}
	}
}