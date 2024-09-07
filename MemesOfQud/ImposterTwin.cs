using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Parts.Mutation;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(EvilTwin))]
	public class ImposterTwin
	{
		[HarmonyPatch("CreateEvilTwin")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Callvirt, typeof(GameObject).GetMethod("MakeActive")))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_S, 5));
					list.Insert(i + 2, CodeInstruction.Call(typeof(ImposterTwin), "PlaySound"));
					break;
				}
			}
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			obj.PlayWorldSound("imposter", 0.75f);
		}
	}
}