﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Parts;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Sweeper))]
	public class BeetlebumSound
	{
		[HarmonyPatch("FireEvent")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Callvirt, typeof(GameObject).GetMethod("FireEvent", new Type[] { typeof(Event) })))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 2, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 3, CodeInstruction.Call(typeof(BeetlebumSound), "PlaySound"));
					break;
				}
			}
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			obj.PlayWorldSound("beetlebum");
		}
	}
}