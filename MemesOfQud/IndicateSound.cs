using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.UI;
using XRL.World;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Cell))]
	public class IndicateSound
	{
		[HarmonyPatch("Indicate")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldstr, "Particles/AutostopNotificationThreat"))
				{
					list.Insert(i + 1, CodeInstruction.Call(typeof(IndicateSound), "PlaySound"));
					return list;
				}
			}
			UnityEngine.Debug.LogError(nameof(IndicateSound) + " FAILED");
			return list;
		}

		private static void PlaySound()
		{
			if (Options.Sound)
			{
				SoundManager.PlaySound("indicate", Volume: 0.75f);
			}
		}
	}
}