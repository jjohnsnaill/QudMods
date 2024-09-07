using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Parts;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Tonic))]
	public class InjectorSound
	{
		[HarmonyPatch("HandleEvent")]
		[HarmonyPatch(new Type[] { typeof(InventoryActionEvent) })]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Callvirt, typeof(GameObject).GetMethod("GetTonicEffects")))
				{
					list.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_1));
					list.Insert(i + 3, new CodeInstruction(OpCodes.Ldfld, typeof(IActOnItemEvent).GetField("Actor")));
					list.Insert(i + 4, CodeInstruction.Call(typeof(InjectorSound), "PlaySound"));
					return list;
				}
			}
			UnityEngine.Debug.LogError(nameof(InjectorSound) + " FAILED");
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			obj.PlayWorldSound("injector");
		}
	}
}