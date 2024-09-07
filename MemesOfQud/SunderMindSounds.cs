using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Parts.Mutation;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(SunderMind))]
	public class SunderMindSounds
	{
		[HarmonyTranspiler]
		[HarmonyPatch("Blast")]
		static IEnumerable<CodeInstruction> Success(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldfld, typeof(SunderMind).GetField("SuccessSound")))
				{
					list.Insert(i + 2, new CodeInstruction(OpCodes.Pop));
					list.Insert(i + 3, new CodeInstruction(OpCodes.Pop));
					list.Insert(i + 4, new CodeInstruction(OpCodes.Ldstr, "Sounds/Abilities/sunderMind"));
					list.Insert(i + 5, new CodeInstruction(OpCodes.Ldc_R4, 1f));
					i += 5;
				}
				else if (list[i].Is(OpCodes.Ldfld, typeof(SunderMind).GetField("FailureSound")))
				{
					list.Insert(i + 2, new CodeInstruction(OpCodes.Pop));
					list.Insert(i + 3, new CodeInstruction(OpCodes.Pop));
					list.Insert(i + 4, new CodeInstruction(OpCodes.Ldstr, "Sounds/Abilities/sunderMindFail"));
					list.Insert(i + 5, new CodeInstruction(OpCodes.Ldc_R4, 1f));
					i += 5;
				}
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch("PenetrationFailure")]
		static IEnumerable<CodeInstruction> Failure(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldfld, typeof(SunderMind).GetField("FailureSound")))
				{
					list.Insert(i + 2, new CodeInstruction(OpCodes.Pop));
					list.Insert(i + 3, new CodeInstruction(OpCodes.Pop));
					list.Insert(i + 4, new CodeInstruction(OpCodes.Ldstr, "Sounds/Abilities/sunderMindFail"));
					list.Insert(i + 5, new CodeInstruction(OpCodes.Ldc_R4, 1f));
					break;
				}
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch("Tick")]
		static IEnumerable<CodeInstruction> End(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if ((list[i].operand as MemberInfo)?.Name == "TakeDamage")
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldloc_1));
					list.Insert(i + 1, CodeInstruction.Call(typeof(SunderMindSounds), "PlaySound"));
					break;
				}
			}
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			obj.PlayWorldSound("Sounds/Abilities/sunderMindEnd", 1);
		}
	}
}