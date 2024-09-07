using HarmonyLib;
using XRL.Sound;
using XRL.World;
using XRL.World.Parts.Skill;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Persuasion_MenacingStare))]
	public class MenacingStareSound
	{
		[HarmonyPatch("ApplyStare")]
		static void Postfix(Cell Cell, int __result)
		{
			if (__result < 1 || !Cell.IsVisible())
			{
				return;
			}

			SoundManager.PlaySound("Sounds/Abilities/stare", Volume: 0.75f);

			foreach (MusicSource music in SoundManager.MusicSources.Values)
			{
				if (music.Track == "circus")
					continue;

				music.SetAudioVolume(0);
			}
		}
	}
}