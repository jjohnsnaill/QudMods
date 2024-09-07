using HarmonyLib;
using XRL.UI;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(TradeUI))]
	public class TradeSound
	{
		[HarmonyPatch("ShowTradeScreen")]
		static void Prefix()
		{
			SoundManager.PlayMusic("trade", "ambient_bed_funny", false);
		}

		[HarmonyPatch("ShowTradeScreen")]
		static void Postfix()
		{
			SoundManager.PlayMusic(null, "ambient_bed_funny", false);
		}
	}
}