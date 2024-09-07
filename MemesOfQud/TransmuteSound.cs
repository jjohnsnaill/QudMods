using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Capabilities;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Transmutation))]
	public class TransmuteSound
	{
		[HarmonyPatch("TransmuteObject")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Callvirt, typeof(Cell).GetMethod("IsVisible")))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Dup));
					list.Insert(i + 1, CodeInstruction.Call(typeof(TransmuteSound), "PlaySound"));
					break;
				}
			}
			return list;
		}

		private static void PlaySound(Cell cell)
		{
			cell.PlayWorldSound("Sounds/Abilities/transmute", 1);
		}
	}
}