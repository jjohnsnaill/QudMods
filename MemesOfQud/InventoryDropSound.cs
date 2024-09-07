using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Parts;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Inventory))]
	public class InventoryDropSound
	{
		[HarmonyPatch("HandleEvent")]
		[HarmonyPatch(new Type[] { typeof(BeforeDeathRemovalEvent) })]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(Event).GetMethod("NewGameObjectList", new Type[] { })))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 1, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 2, CodeInstruction.Call(typeof(InventoryDropSound), "PlaySound"));
					return list;
				}
			}
			UnityEngine.Debug.LogError(nameof(InventoryDropSound) + " FAILED");
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			obj.PlayWorldSound("dropInventory");
		}
	}
}