using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL;
using XRL.UI;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(PsychicHunterSystem))]
	public class ImposterHunter
	{
		[HarmonyPatch("CreateSeekerHunters")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].opcode == OpCodes.Ldloc_0 && list[i + 1].opcode == OpCodes.Brfalse_S) //check if the hunters actually spawned
				{
					list.Insert(i + 3, CodeInstruction.Call(typeof(ImposterHunter), "PlaySound"));
					break;
				}
			}
			return list;
		}

		[HarmonyPatch("PsychicPresenceMessage")]
		static void Prefix(ref int Number, ref bool UsePopup)
		{
			PlaySound();
		}

		private static void PlaySound()
		{
			if (Options.Sound)
			{
				SoundManager.PlaySound("imposter", Volume: 0.75f);
			}
		}
	}
}