using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.Rules;
using XRL.World.Effects;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Bleeding))]
	public class BleedSound
	{
		[HarmonyPatch("StartMessage")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldstr, "Sounds/StatusEffects/sfx_statusEffect_physicalRupture"))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Pop));
					list.Insert(i + 2, CodeInstruction.Call(typeof(BleedSound), "GetSound"));
					return list;
				}
			}
			UnityEngine.Debug.LogError(nameof(BleedSound) + " FAILED");
			return list;
		}

		private static string GetSound()
		{
			switch (Stat.Rnd2.Next(3))
			{
				default:
					return "Sounds/StatusEffects/bleed1";
				case 1:
					return "Sounds/StatusEffects/bleed2";
				case 2:
					return "Sounds/StatusEffects/bleed3";
			}
		}
	}
}