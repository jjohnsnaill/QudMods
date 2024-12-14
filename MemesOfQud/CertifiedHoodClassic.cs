using HarmonyLib;
using XRL;
using XRL.Sound;
using XRL.UI;
using XRL.World.Quests;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(SoundManager))]
	public class CertifiedHoodClassic
	{
		[HarmonyPatch("SetChannelTrack")]
		static void Prefix(string Track, string Channel)
		{
			if (The.Game?.GetSystem<AscensionSystem>()?.Stage > 1)
			{
				return;
			}

			if (Options.Music && !Track.IsNullOrEmpty() && !Channel.StartsWith("ambient_bed") && SoundManager.MusicSources.TryGetValue(Channel, out MusicSource music) && music.Track != Track)
			{
				SoundManager.PlaySound("certifiedHoodClassic");
			}
		}
	}
}