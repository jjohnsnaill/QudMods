using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Effects;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(FungalSporeInfection))]
	public class InfectionSound
	{
		[HarmonyPatch("Apply")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Callvirt, typeof(GameObject).GetMethod("FireEvent", new Type[] { typeof(string) })))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldarg_1));
					list.Insert(i + 1, CodeInstruction.Call(typeof(InfectionSound), "PlaySound"));
					return list;
				}
			}
			UnityEngine.Debug.LogError(nameof(InfectionSound) + " FAILED");
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			obj.PlayWorldSound("Sounds/StatusEffects/bruh");
		}
	}
}