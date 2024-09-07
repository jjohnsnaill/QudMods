using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL;
using XRL.Core;
using XRL.Sound;
using XRL.UI;

namespace Mods.MemesOfQud
{
	[HarmonyPatch]
	public class DeathMusic
	{
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(CheckpointingSystem), "ShowDeathMessage")]
		static IEnumerable<CodeInstruction> ShowDeathMessage(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(Popup).GetMethod("ShowSpace")))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldnull));
					list.Insert(i + 1, CodeInstruction.Call(typeof(DeathMusic), "SetMusic"));
					i += 2;
				}
			}
			return list;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(XRLCore), "BuildScore")]
		static void BuildScore(bool Real)
		{
			if (Real)
			{
				SetMusic("dead");
			}
		}

		private static void SetMusic(string track)
		{
			SoundManager.PlayMusic(track, Crossfade: false);

			if (track == null)
			{
				foreach (MusicSource music in SoundManager.MusicSources.Values)
				{
					music.SetAudioVolume(0);
				}
			}
		}
	}
}