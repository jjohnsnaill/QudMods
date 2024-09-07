using HarmonyLib;
using XRL.World;
using XRL.World.Parts.Mutation;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(StunningForce))]
	public class StunningForceSound
	{
		[HarmonyPatch("Concussion")]
		static void Prefix(ref Cell StartCell)
		{
			StartCell.PlayWorldSound("Sounds/Abilities/stunningForce");
		}
	}
}