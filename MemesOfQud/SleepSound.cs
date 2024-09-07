using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.Rules;
using XRL.World;
using XRL.World.Effects;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Asleep))]
	public class SleepSound
	{
		[HarmonyTranspiler]
		[HarmonyPatch("Apply")]
		static IEnumerable<CodeInstruction> Apply(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Callvirt, typeof(GameObject).GetMethod("ForfeitTurn")))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldarg_1));
					list.Insert(i + 1, CodeInstruction.Call(typeof(SleepSound), "PlaySleep"));
					break;
				}
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch("HandleEvent")]
		[HarmonyPatch(new Type[] { typeof(EndTurnEvent) })]
		static IEnumerable<CodeInstruction> HandleEvent(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(Asleep).GetMethod("CheckAsleepOn")))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 1, CodeInstruction.Call(typeof(Effect), "get_Object"));
					list.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 3, new CodeInstruction(OpCodes.Ldfld, typeof(Effect).GetField("Duration")));
					list.Insert(i + 4, CodeInstruction.Call(typeof(SleepSound), "PlaySleeping"));
					break;
				}
			}
			return list;
		}

		private static void PlaySleep(GameObject obj)
		{
			obj.PlayWorldSound(Stat.Rnd2.Next(2) == 0 ? "Sounds/StatusEffects/sleep1" : "Sounds/StatusEffects/sleep2");
		}

		private static void PlaySleeping(GameObject obj, int duration)
		{
			if (duration % 4 == 0)
			{
				obj.PlayWorldSound("Sounds/StatusEffects/sleeping1");
			}
			else if (duration % 4 == 2)
			{
				obj.PlayWorldSound("Sounds/StatusEffects/sleeping2");
			}
		}
	}
}