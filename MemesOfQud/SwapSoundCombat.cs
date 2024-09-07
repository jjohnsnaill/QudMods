using HarmonyLib;
using XRL.World;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(CombatJuice))]
	public class SwapSoundCombat
	{
		[HarmonyPatch("playWorldSound")]
		static void Prefix(ref GameObject obj, ref string clip)
		{
			ConfusionSounds.GetConfusion(obj, ref clip);
		}
	}
}