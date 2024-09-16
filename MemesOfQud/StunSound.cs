using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Effects;

namespace Mods.MemesOfQud
{
	[HarmonyPatch]
	public class StunSound
	{
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(Stun), "Apply")]
		static IEnumerable<CodeInstruction> Stun(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(Stun).GetMethod("ApplyStats", BindingFlags.NonPublic | BindingFlags.Instance)))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldarg_1));
					list.Insert(i + 1, CodeInstruction.Call(typeof(StunSound), "PlaySound"));
					return list;
				}
			}
			UnityEngine.Debug.LogError(nameof(StunSound) + " FAILED");
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(Paralyzed), "Apply")]
		static IEnumerable<CodeInstruction> Paralyze(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(Paralyzed).GetMethod("ApplyStats", BindingFlags.NonPublic | BindingFlags.Instance)))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldarg_1));
					list.Insert(i + 1, CodeInstruction.Call(typeof(StunSound), "PlaySound"));
					return list;
				}
			}
			UnityEngine.Debug.LogError(nameof(StunSound) + " FAILED");
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			obj.PlayWorldSound("Sounds/StatusEffects/stun");
		}
	}
}