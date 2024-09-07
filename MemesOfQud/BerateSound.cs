using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using XRL.Rules;
using XRL.World;
using XRL.World.Effects;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Shamed))]
	public class BerateSound
	{
		[HarmonyPatch("Apply")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(Shamed).GetMethod("ApplyStats", BindingFlags.NonPublic | BindingFlags.Instance)))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_1));
					list.Insert(i + 2, CodeInstruction.Call(typeof(BerateSound), "PlaySound"));
					break;
				}
			}
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			switch (Stat.Rnd2.Next(4))
			{
				case 0:
					obj.PlayWorldSound("Sounds/Abilities/berate1");
					break;
				case 1:
					obj.PlayWorldSound("Sounds/Abilities/berate2");
					break;
				case 2:
					obj.PlayWorldSound("Sounds/Abilities/berate3");
					break;
				case 3:
					obj.PlayWorldSound("Sounds/Abilities/berate4");
					break;
			}
		}
	}
}