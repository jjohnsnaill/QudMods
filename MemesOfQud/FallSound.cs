using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(StairsDown))]
	public class FallSound
	{
		[HarmonyPatch("CheckPullDown")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldstr, "fall"))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 1, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 2, CodeInstruction.Call(typeof(FallSound), "PlaySound"));
					break;
				}
			}
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			switch (Stat.Rnd2.Next(3))
			{
				case 0:
					obj.PlayWorldSound("fall1");
					break;
				case 1:
					obj.PlayWorldSound("fall2");
					break;
				case 2:
					obj.PlayWorldSound("fall3");
					break;
			}
		}
	}
}