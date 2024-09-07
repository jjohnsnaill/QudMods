using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using XRL.Rules;
using XRL.World;
using XRL.World.Effects;

namespace Mods.MemesOfQud
{
	[HarmonyPatch]
	public class ConfusionSounds
	{
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(Confused), "Apply")]
		static IEnumerable<CodeInstruction> Apply(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(Confused).GetMethod("ApplyChanges", BindingFlags.NonPublic | BindingFlags.Instance)))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_1));
					list.Insert(i + 2, CodeInstruction.Call(typeof(ConfusionSounds), "PlaySound"));
					return list;
				}
			}
			UnityEngine.Debug.LogError(nameof(ConfusionSounds) + " FAILED");
			return list;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(GameObject), "PlayWorldSound")]
		static void Swap(GameObject __instance, ref string Clip)
		{
			GetConfusion(__instance, ref Clip);
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CombatJuice), "playWorldSound")]
		static void SwapCombat(ref GameObject obj, ref string clip)
		{
			GetConfusion(obj, ref clip);
		}

		public static void GetConfusion(GameObject obj, ref string clip)
		{
			if (clip == null)
			{
				return;
			}

			GetConfusion(obj, out bool confused, out bool furious);
			if (furious && clip[0] != '!')
			{
				clip = clip.Insert(0, "!");
			}
			else if (confused)
			{
				clip = "Sounds/StatusEffects/confused";
			}
		}

		public static void GetConfusion(GameObject obj, out bool confused, out bool furious)
		{
			confused = false;
			furious = false;

			foreach (Effect effect in obj.Effects)
			{
				if (effect is Confused)
				{
					confused = true;
					continue;
				}
				if (effect is FuriouslyConfused || effect is HulkHoney_Tonic_Allergy)
				{
					confused = true;
					furious = true;
				}
			}
		}

		private static void PlaySound(GameObject obj)
		{
			switch (Stat.Rnd2.Next(3))
			{
				case 0:
					obj.PlayWorldSound("Sounds/StatusEffects/confusion1");
					break;
				case 1:
					obj.PlayWorldSound("Sounds/StatusEffects/confusion2");
					break;
				case 2:
					obj.PlayWorldSound("Sounds/StatusEffects/confusion3");
					break;
			}
		}
	}
}