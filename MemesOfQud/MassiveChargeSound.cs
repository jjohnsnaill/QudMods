using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace Mods.MemesOfQud
{
	[HarmonyPatch]
	public class MassiveChargeSound
	{
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(MultiHorns))]
		[HarmonyPatch("FireEvent")]
		static IEnumerable<CodeInstruction> MultiHorns(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(MultiHorns).GetMethod("GetTurnsToCharge")))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 2, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 3, CodeInstruction.Call(typeof(MassiveChargeSound), "PlayWindupSound"));
					i += 3;
				}
				else if (list[i].Is(OpCodes.Call, typeof(MultiHorns).GetMethod("PerformCharge")))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 2, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 3, CodeInstruction.Call(typeof(MassiveChargeSound), "PlayChargeSound"));
					i += 3;
				}
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(RunOver))]
		[HarmonyPatch("HandleEvent", new Type[] { typeof(CommandEvent) })]
		static IEnumerable<CodeInstruction> RunOverWindup(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(RunOver).GetMethod("GetTurnsToCharge")))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 2, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 3, CodeInstruction.Call(typeof(MassiveChargeSound), "PlayWindupSound"));
					break;
				}
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(RunOver))]
		[HarmonyPatch("HandleEvent", new Type[] { typeof(BeginTakeActionEvent) })]
		static IEnumerable<CodeInstruction> RunOver(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(RunOver).GetMethod("PerformCharge")))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 2, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 3, CodeInstruction.Call(typeof(MassiveChargeSound), "PlayChargeSound"));
					break;
				}
			}
			return list;
		}

		private static void PlayWindupSound(GameObject obj)
		{
			obj.PlayWorldSound("Sounds/Abilities/massiveChargeWindup", 1);
		}

		private static void PlayChargeSound(GameObject obj)
		{
			obj.PlayWorldSound("Sounds/Abilities/massiveCharge", 1);
		}
	}
}