using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace Mods.MemesOfQud
{
	[HarmonyPatch]
	public class ReflectSound
	{
		static IEnumerable<MethodBase> TargetMethods()
		{
			MethodInfo[] methods = typeof(MissileWeapon).GetMethods();
			for (int i = 0; i < methods.Length; i++)
			{
				if (methods[i].Name == "CalculateBulletTrajectory" && methods[i].GetParameters()[0].ParameterType == typeof(bool).MakeByRefType())
				{
					yield return methods[i];
				}
			}

			methods = typeof(LightManipulation).GetMethods();
			for (int i = 0; i < methods.Length; i++)
			{
				if (methods[i].Name == "HandleEvent" && methods[i].GetParameters()[0].ParameterType == typeof(CommandEvent))
				{
					yield return methods[i];
				}
			}
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			CodeInstruction cell = null;

			for (int i = 0; i < list.Count; i++)
			{
				if (cell != null)
				{
					if (list[i].Is(OpCodes.Callvirt, typeof(Cell).GetMethod("FireEvent", new Type[] { typeof(Event) })))
					{
						list.Insert(i + 5, cell);
						list.Insert(i + 6, CodeInstruction.Call(typeof(ReflectSound), "PlaySound"));
						break;
					}
					continue;
				}
				if (list[i].Is(OpCodes.Ldstr, "RefractLight") && list[i + 1].opcode == OpCodes.Ldc_I4_0)
				{
					cell = list[i - 2].Is(OpCodes.Callvirt, typeof(Cell).GetMethod("HasObjectWithRegisteredEvent", new Type[] { typeof(string) })) ? list[i - 4] : list[i - 8];
				}
			}

			cell = null;
			for (int i = 0; i < list.Count; i++)
			{
				if (cell != null)
				{
					if (list[i].Is(OpCodes.Callvirt, typeof(Cell).GetMethod("FireEvent", new Type[] { typeof(Event) })))
					{
						list.Insert(i + 5, cell);
						list.Insert(i + 6, CodeInstruction.Call(typeof(ReflectSound), "PlaySound"));
						break;
					}
					continue;
				}
				if (list[i].Is(OpCodes.Ldstr, "ReflectProjectile") && list[i + 1].opcode == OpCodes.Ldc_I4_0)
				{
					cell = list[i - 4];
				}
			}

			return list;
		}

		private static void PlaySound(Cell cell)
		{
			cell.PlayWorldSound("no", 1);
		}
	}

	[HarmonyPatch(typeof(MentalMirror))]
	public class MentalMirrorSound
	{
		[HarmonyPatch("ReflectMessage")]
		static void Prefix(MentalMirror __instance)
		{
			__instance.ParentObject.PlayWorldSound("no", 1);
		}
	}
}