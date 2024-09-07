using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Parts.Skill;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Acrobatics_Jump))]
	public class JumpSound
	{
		[HarmonyPatch("Jump")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Callvirt, typeof(GameObject).GetMethod("BodyPositionChanged", new Type[] { typeof(string), typeof(bool) })))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 2, CodeInstruction.Call(typeof(JumpSound), "PlaySound"));
					break;
				}
			}
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			obj.PlayWorldSound("Sounds/Abilities/jump");
		}
	}
}