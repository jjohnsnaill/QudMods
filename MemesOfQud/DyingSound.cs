using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(XRLCore))]
	public class DyingSound
	{
		[HarmonyPatch("PlayerTurn")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].opcode == OpCodes.Ldc_I4_0 && list[i + 1].Is(OpCodes.Stfld, typeof(XRLCore).GetField("HPWarning")))
				{
					list.Insert(i + 2, CodeInstruction.Call(typeof(DyingSound), "PlaySound"));
					break;
				}
			}
			return list;
		}

		private static void PlaySound()
		{
			if (!Options.Sound)
			{
				return;
			}

			switch (Stat.Rnd2.Next(3))
			{
				case 0:
					SoundManager.PlaySound("Sounds/StatusEffects/dying1", Volume: 0.15f);
					break;
				case 1:
					SoundManager.PlaySound("Sounds/StatusEffects/dying2", Volume: 0.15f);
					break;
				case 2:
					SoundManager.PlaySound("Sounds/StatusEffects/dying3", Volume: 0.15f);
					break;
			}
		}
	}
}