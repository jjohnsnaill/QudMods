using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Parts;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(LiquidVolume))]
	public class PourSound
	{
		[HarmonyPatch("Pour",
		new Type[] { typeof(bool), typeof(GameObject), typeof(Cell), typeof(bool), typeof(bool), typeof(int), typeof(bool) },
		new ArgumentType[] { ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].opcode == OpCodes.Ldc_I4_1 && list[i + 1].opcode == OpCodes.Ret)
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_2));
					list.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_3));
					list.Insert(i + 3, CodeInstruction.Call(typeof(PourSound), "PlaySound"));
					i += 3;
				}
			}
			return list;
		}

		/*[HarmonyPatch("PourIntoCellInternal")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].opcode == OpCodes.Ldc_I4_1 && list[i + 1].opcode == OpCodes.Ret)
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldarg_2));
					list.Insert(i + 1, CodeInstruction.Call(typeof(PourSound), "PlaySound"));
					i += 2;
				}
			}
			return list;
		}*/

		private static void PlaySound(GameObject obj, Cell cell)
		{
			if (cell != null)
			{
				cell.PlayWorldSound("pour");
				return;
			}
			obj?.PlayWorldSound("pour");
		}
	}
}