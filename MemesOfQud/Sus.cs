using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.UI;
using XRL.World.Capabilities;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(PsychicGlimmer))]
	public class Sus
	{
		[HarmonyPatch("Update")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].opcode == OpCodes.Bge && list[i + 1].opcode == OpCodes.Ldstr)
				{
					list.Insert(i + 1, CodeInstruction.Call(typeof(Sus), "PlaySound"));
					i++;
				}
			}
			return list;
		}

		private static void PlaySound()
		{
			if (Options.Sound)
			{
				SoundManager.PlaySound("sus");
			}
		}
	}
}