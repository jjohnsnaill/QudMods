using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Effects;

namespace Mods.MemesOfQud
{
	[HarmonyPatch]
	public class JetSound
	{
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(Dashing), "Apply")]
		static IEnumerable<CodeInstruction> Apply(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldstr, "Sounds/StatusEffects/sfx_statusEffect_movementBuff_big"))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Pop));
					list.Insert(i + 2, new CodeInstruction(OpCodes.Ldstr, "Sounds/Abilities/jetStart"));
					return list;
				}
			}
			UnityEngine.Debug.LogError(nameof(JetSound) + " FAILED");
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			obj.PlayWorldSound("Sounds/Abilities/jet");
		}
	}
}