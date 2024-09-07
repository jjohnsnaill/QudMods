using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using XRL.Liquids;
using XRL.World;
using XRL.World.Parts;

namespace Mods.MemesOfQud
{
	[HarmonyPatch]
	public class SlipSound
	{
		/*static IEnumerable<MethodBase> TargetMethods()
		{
			Type[] types = AccessTools.GetTypesFromAssembly(typeof(BaseLiquid).Assembly);
			for (int i = 0; i < types.Length; i++)
			{
				if (types[i] == typeof(LiquidSlime) || types[i] == typeof(LiquidWater) || types[i] == typeof(LiquidOil) || types[i] == typeof(LiquidInk) || types[i] == typeof(LiquidGel))
				{
					yield return types[i].GetMethod("ObjectEnteredCell", new Type[] { typeof(LiquidVolume), typeof(IObjectCellInteractionEvent) });
				}
			}
		}*/

		[HarmonyPatch(typeof(BaseLiquid), "ObjectEnteredCell", new Type[] { typeof(LiquidVolume), typeof(IObjectCellInteractionEvent) })]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if ((list[i].operand as MemberInfo)?.Name == "Move")
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_2));
					list.Insert(i + 2, new CodeInstruction(OpCodes.Ldfld, typeof(IObjectCellInteractionEvent).GetField("Object")));
					list.Insert(i + 3, CodeInstruction.Call(typeof(SlipSound), "PlaySound"));
					return list;
				}
			}
			UnityEngine.Debug.LogError(nameof(SlipSound) + " FAILED");
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			obj.PlayWorldSound("slip");
		}
	}
}