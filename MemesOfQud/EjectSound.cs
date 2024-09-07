using ConsoleLib.Console;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.ZoneParts;

namespace Mods.MemesOfQud
{
	[HarmonyPatch]
	public class EjectSound
	{
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(EjectionSeat))]
		[HarmonyPatch("Eject")]
		static IEnumerable<CodeInstruction> Eject(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldfld, typeof(EjectionSeat).GetField("Sound")))
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Pop));
					list.Insert(i + 2, new CodeInstruction(OpCodes.Ldstr, "eject"));
					i += 2;
				}
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(ScriptCallToArms))]
		[HarmonyPatch("spawnParties")]
		static IEnumerable<CodeInstruction> CallToArms(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Callvirt, typeof(ScreenBuffer).GetMethod("Shake")))
				{
					list.Insert(i, CodeInstruction.Call(typeof(EjectSound), "PlaySound"));
					list.Insert(i + 1 /*2*/, CodeInstruction.Call(typeof(EjectSound), "PlayMusic"));

					list.Insert(i - 2, new CodeInstruction(OpCodes.Pop));
					list.Insert(i - 1, new CodeInstruction(OpCodes.Ldc_I4, 2500));

					break;
				}
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(ScriptCallToArms))]
		[HarmonyPatch("HandleEvent")]
		static IEnumerable<CodeInstruction> CircusMusic(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			bool first = true;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldstr, "Battle at Grit Gate"))
				{
					list.Insert(++i, first ? CodeInstruction.Call(typeof(EjectSound), "StopMusic") : CodeInstruction.Call(typeof(EjectSound), "GetMusic"));
					first = false;
				}
			}
			return list;
		}

		private static void PlaySound()
		{
			if (Options.Sound)
			{
				SoundManager.PlaySound("eject");
			}
		}

		private static void PlayMusic()
		{
			if (Options.GetOption("SwapMusic") != "No")
				SoundManager.PlayMusic("circus", Crossfade: false);
		}

		private static string GetMusic(string track)
		{
			return Options.GetOption("SwapMusic") == "No" ? track : "circus";
		}

		private static string StopMusic(string track)
		{
			return Options.GetOption("SwapMusic") == "No" ? track : null;
		}
	}
}