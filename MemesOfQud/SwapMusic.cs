using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Music))]
	public class SwapMusic
	{
		[HarmonyPatch("TryMusic")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldstr, "music"))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 1, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 2, CodeInstruction.Call(typeof(SwapMusic), "Swap"));
					break;
				}
			}
			return list;
		}

		private static string Swap(string track, GameObject obj)
		{
			if (Options.GetOption("SwapMusic") == "No")
			{
				return track;
			}

			if (track == "Golgotha (Graveyard)")
			{
				return "golgotha";
			}

			if (Options.GetOption("SwapMusic") == "Yes")
			{
				if (track == "MoghrayiRemembrance")
				{
					return "desert";
				}
				if (track == "Caves1")
				{
					return "caves";
				}
				if (track == "StoicPorridge")
				{
					return "gritGate";
				}
				if (track == "Lazarus")
				{
					return "lab";
				}
				foreach (CellBlueprint bp in The.ZoneManager.GetCellBlueprints(obj.CurrentZone.ZoneID))
				{
					if (bp.Name == "Hills" || bp.Inherits == "Hills" || bp.Name == "Mountains" || bp.Inherits == "Mountains")
					{
						return "hills";
					}
					if ((bp.Name == "LakeHinnomCell" || bp.Inherits == "LakeHinnomCell" || bp.Name == "PalladiumReefCell" || bp.Inherits == "PalladiumReefCell") && track == "Substrate")
					{
						return "reef";
					}
				}
			}
			return track;
		}
	}
}