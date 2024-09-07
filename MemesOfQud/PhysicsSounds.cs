using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Parts;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Physics))]
	public class PhysicsSounds
	{
		[HarmonyTranspiler]
		[HarmonyPatch("ApplyExplosion")]
		static IEnumerable<CodeInstruction> ApplyExplosion(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldc_I4_S, 20) && list[i + 1].opcode == OpCodes.Ldarg_1 && list[i + 2].Is(OpCodes.Ldc_I4, 1000))
				{
					list.Insert(i + 6, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 7, new CodeInstruction(OpCodes.Ldarg_1));
					list.Insert(i + 8, new CodeInstruction(OpCodes.Ldarg_S, 9));
					list.Insert(i + 9, CodeInstruction.Call(typeof(PhysicsSounds), "PlayExplosion"));
					break;
				}
			}
			return list;
		}

		private static void PlayExplosion(Cell cell, int force, bool neutron)
		{
			cell?.PlayWorldSound(force >= 1000000 || neutron ? "nuke" : "boom");
		}

		[HarmonyTranspiler]
		[HarmonyPatch("HandleEvent", new Type[] { typeof(ObjectEnteringCellEvent) })]
		static IEnumerable<CodeInstruction> Collision(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].opcode == OpCodes.Ldc_I4_0 && list[i + 1].opcode == OpCodes.Ret)
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldarg_1));
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldfld, typeof(IObjectCellInteractionEvent).GetField("Object")));
					list.Insert(i + 2, CodeInstruction.Call(typeof(PhysicsSounds), "PlayCollision"));
					break;
				}
			}
			return list;
		}

		private static void PlayCollision(GameObject obj)
		{
			obj.PlayWorldSound("collision");
		}
	}
}