using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Clairvoyance))]
	public class ClairvoyanceSound
	{
		[HarmonyPatch("FireEvent")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldfld, typeof(Clairvoyance).GetField("Sound")))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Pop));
					list.Insert(i + 2, CodeInstruction.Call(typeof(ClairvoyanceSound), "GetSound"));
					break;
				}
			}
			return list;
		}

		private static string GetSound()
		{
			return Stat.Rnd2.Next(2) == 0 ? "Sounds/Abilities/clairvoyance1" : "Sounds/Abilities/clairvoyance2";
		}
	}
}