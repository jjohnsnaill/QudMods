using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts.Skill;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(Tactics_Charge))]
	public class ChargeSound
	{
		[HarmonyPatch("PerformCharge")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldfld, typeof(Tactics_Charge).GetField("ActivatedAbilityID")))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 1, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 2, CodeInstruction.Call(typeof(ChargeSound), "PlaySound"));
					return list;
				}
			}
			UnityEngine.Debug.LogError(nameof(ChargeSound) + " FAILED");
			return list;
		}

		private static void PlaySound(GameObject obj)
		{
			switch (Stat.Rnd2.Next(3))
			{
				case 0:
					obj.PlayWorldSound("Sounds/Abilities/charge1");
					break;
				case 1:
					obj.PlayWorldSound("Sounds/Abilities/charge2");
					break;
				case 2:
					obj.PlayWorldSound("Sounds/Abilities/charge3");
					break;
			}
		}
	}
}