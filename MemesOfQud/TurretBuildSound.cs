using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using XRL.Rules;
using XRL.World;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Parts.Skill;

namespace Mods.MemesOfQud
{
	[HarmonyPatch]
	public class TurretBuildSound
	{
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PlaceTurretGoal), "TakeAction")]
		static IEnumerable<CodeInstruction> TakeAction(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if ((list[i].operand as MemberInfo)?.Name == "AddObject")
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 2, CodeInstruction.Call(typeof(GoalHandler), "get_ParentObject"));
					list.Insert(i + 3, CodeInstruction.Call(typeof(TurretBuildSound), "PlaySound"));
					break;
				}
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(Tinkering_DeployTurret), "FireEvent")]
		static IEnumerable<CodeInstruction> FireEvent(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if ((list[i].operand as MemberInfo)?.Name == "AddObject")
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 2, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 3, CodeInstruction.Call(typeof(TurretBuildSound), "PlaySound"));
					break;
				}
			}
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			obj.PlayWorldSound(Stat.Rnd2.Next(2) == 0 ? "turretBuild1" : "turretBuild2");
		}
	}
}