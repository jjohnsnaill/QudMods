using HarmonyLib;
using XRL.Sound;
using XRL.UI;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(SoundManager))]
	public class CertifiedHoodClassic
	{
		[HarmonyPatch("SetChannelTrack")]
		static void Prefix(string Track, string Channel)
		{
			if (Options.Music && Track != null && !Channel.StartsWith("ambient_bed") && SoundManager.MusicSources.TryGetValue(Channel, out MusicSource music) && music.Track != Track)
			{
				SoundManager.PlaySound("certifiedHoodClassic");
			}
		}
	}
}