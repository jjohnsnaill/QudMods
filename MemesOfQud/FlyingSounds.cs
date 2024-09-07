using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.Rules;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;

namespace Mods.MemesOfQud
{
	[HarmonyPatch]
	public class FlyingSounds
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(Flying), "Apply")]
		static void Flying(GameObject Object)
		{
			Object.PlayWorldSound("Sounds/Abilities/fly");
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(Combat), "SwoopAttack")]
		static IEnumerable<CodeInstruction> Swoop(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(Flight).GetMethod("SuspendFlight")))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 2, CodeInstruction.Call(typeof(FlyingSounds), "PlaySound"));
					break;
				}
			}
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			switch (Stat.Rnd2.Next(4))
			{
				case 0:
					obj.PlayWorldSound("Sounds/Abilities/swoop1");
					break;
				case 1:
					obj.PlayWorldSound("Sounds/Abilities/swoop2");
					break;
				case 2:
					obj.PlayWorldSound("Sounds/Abilities/swoop3");
					break;
				case 3:
					obj.PlayWorldSound("Sounds/Abilities/swoop4");
					break;
			}
		}
	}
}