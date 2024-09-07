using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts.Mutation;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(GasGeneration))]
	public class Gas
	{
		[HarmonyPatch("PumpGas")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(Phase).GetMethod("carryOverPrep")))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 2, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 3, CodeInstruction.Call(typeof(Gas), "PlaySound"));
					return list;
				}
			}
			UnityEngine.Debug.LogError(nameof(Gas) + " FAILED");
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			obj.PlayWorldSound("Sounds/Abilities/gas");
		}
	}
}