using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.UI;
using XRL.World;
using XRL.World.Parts.Mutation;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Precognition))]
	public class PrecognitionSound
	{
		[HarmonyPatch("FireEvent")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(Precognition).GetMethod("Save", new Type[] { })))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 1, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 2, CodeInstruction.Call(typeof(PrecognitionSound), "PlayStart"));
					i += 3;
				}
				else if (list[i].Is(OpCodes.Call, typeof(Popup).GetMethod("ShowYesNo", new Type[] { typeof(string), typeof(bool), typeof(DialogResult) })) || (list[i].Is(OpCodes.Call, typeof(Precognition).GetMethod("Load")) && list[i + 1].opcode == OpCodes.Ldarg_0 && list[i + 2].opcode == OpCodes.Ldarg_0))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 1, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 2, CodeInstruction.Call(typeof(PrecognitionSound), "PlayEnd"));
					i += 3;
				}
				else if (list[i].Is(OpCodes.Callvirt, typeof(GameObject).GetMethod("DilationSplat")))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 2, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 3, CodeInstruction.Call(typeof(PrecognitionSound), "PlayEnd"));
					i += 3;
				}
			}
			return list;
		}

		private static void PlayStart(GameObject obj)
		{
			obj.PlayWorldSound("Sounds/Abilities/precognitionStart");
		}

		private static void PlayEnd(GameObject obj)
		{
			obj.PlayWorldSound("Sounds/Abilities/precognitionEnd");
		}
	}
}