using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.Sound;
using XRL.UI;
using XRL.World;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(GameObject))]
	public class CompanionDeathSound
	{
		[HarmonyPatch("Destroy")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i].Is(OpCodes.Call, typeof(Popup).GetMethod("Show")))
				{
					list.Insert(i, CodeInstruction.Call(typeof(CompanionDeathSound), "PlaySound"));
					break;
				}
			}
			return list;
		}

		private static void PlaySound()
		{
			if (Options.Sound)
			{
				SoundManager.PlaySound("fail", Volume: 0.5f);
				foreach (MusicSource music in SoundManager.MusicSources.Values)
				{
					music.SetAudioVolume(0);
				}
			}
		}
	}
}