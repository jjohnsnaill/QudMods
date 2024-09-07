using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World.Effects;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Running))]
	public class SprintSound
	{
		[HarmonyPatch("Apply")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldstr, "Sounds/StatusEffects/sfx_statusEffect_movementBuff"))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Pop));
					list.Insert(i + 2, new CodeInstruction(OpCodes.Ldstr, "Sounds/Abilities/sprint"));
					return list;
				}
			}
			UnityEngine.Debug.LogError(nameof(SprintSound) + " FAILED");
			return list;
		}
	}
}