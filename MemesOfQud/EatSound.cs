using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Parts;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Food))]
	public class EatSound
	{
		[HarmonyPatch("HandleEvent")]
		[HarmonyPatch(new Type[] { typeof(InventoryActionEvent) })]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldstr, "AddWater"))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Dup));
					list.Insert(i + 1, CodeInstruction.Call(typeof(EatSound), "PlaySound"));
					return list;
				}
			}
			UnityEngine.Debug.LogError(nameof(EatSound) + " FAILED");
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			obj.PlayWorldSound("eat");
		}
	}
}