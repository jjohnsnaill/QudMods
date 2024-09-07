using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using XRL.Core;

namespace Mods.Randomizer
{
	[HarmonyPatch]
	public class HighScoreSeparator
	{
		static IEnumerable<MethodBase> TargetMethods()
		{
			yield return typeof(Scoreboard2).GetMethod("Load");
			yield return typeof(Scoreboard2).GetMethod("Save");
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldstr, "HighScores.json"))
				{
					list.Insert(++i, new CodeInstruction(OpCodes.Pop));
					list.Insert(++i, new CodeInstruction(OpCodes.Ldstr, "HighScoresRandomizer.json"));
				}
				else if (list[i].Is(OpCodes.Ldstr, "HighScores.dat"))
				{
					list.Insert(++i, new CodeInstruction(OpCodes.Pop));
					list.Insert(++i, new CodeInstruction(OpCodes.Ldstr, "HighScoresRandomizer.dat"));
				}
			}
			return list;
		}
	}
}