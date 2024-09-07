using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using XRL.World;

namespace Mods.MemesOfQud
{
	[HarmonyPatch]
	public class MoveSounds
	{
		static MethodBase TargetMethod()
		{
			MethodInfo[] methods = typeof(GameObject).GetMethods();
			for (int i = 0; i < methods.Length; i++)
			{
				if (methods[i].Name == "Move" && methods[i].GetParameters()[1].ParameterType == typeof(GameObject).MakeByRefType())
				{
					return methods[i];
				}
			}
			return null;
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldstr, "Dashing") && list[i + 1].Is(OpCodes.Call, typeof(GameObject).GetMethod("HasEffect", new Type[] { typeof(string) })))
				{
					list.Insert(i + 6, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 7, CodeInstruction.Call(typeof(JetSound), "PlaySound"));
					i += 7;
				}
				else if (list[i].Is(OpCodes.Call, typeof(GameObject).GetMethod("ProcessMoveEvent", BindingFlags.NonPublic | BindingFlags.Instance)))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_1));
					list.Insert(i + 3, CodeInstruction.Call(typeof(MoveSounds), "PlaySound"));
					i += 3;
				}
			}
			return list;
		}

		private static void PlaySound(GameObject obj, string direction)
		{
			if (direction == "D")
			{
				obj.PlayWorldSound("stairsDown");
			}
		}
	}
}