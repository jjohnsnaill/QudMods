using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using XRL;
using XRL.Sound;
using XRL.UI;
using XRL.World;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(XRLGame))]
	public class QuestSounds
	{
		[HarmonyTranspiler]
		[HarmonyPatch("FinishQuestStep")]
		[HarmonyPatch(new Type[] { typeof(Quest), typeof(string), typeof(int), typeof(bool), typeof(string) })]
		static IEnumerable<CodeInstruction> Notification(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if ((list[i].operand as MemberInfo)?.Name == "ShowBlock")
				{
					list.Insert(i, CodeInstruction.Call(typeof(QuestSounds), "PlayNotification"));
					i++;
				}
			}
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch("FinishQuest")]
		[HarmonyPatch(new Type[] { typeof(Quest) })]
		static IEnumerable<CodeInstruction> Complete(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if ((list[i].operand as MemberInfo)?.Name == "ShowBlock")
				{
					list.Insert(i, CodeInstruction.Call(typeof(QuestSounds), "PlayComplete"));
					i++;
				}
			}
			return list;
		}

		[HarmonyPrefix]
		[HarmonyPatch("PopupStartQuest")]
		static void Start()
		{
			PlayNotification();
		}

		[HarmonyPrefix]
		[HarmonyPatch("PopupFailQuest")]
		static void Fail()
		{
			if (Options.Sound)
			{
				SoundManager.PlaySound("fail", Volume: 0.5f);
				foreach (MusicSource music in SoundManager.MusicSources.Values)
				{
					music.SetAudioVolume(0);
				}
			}
		}

		private static void PlayNotification()
		{
			if (Options.Sound)
			{
				SoundManager.PlaySound("notification");
			}
		}

		private static void PlayComplete()
		{
			if (Options.Sound)
			{
				SoundManager.PlaySound("quest");
				foreach (MusicSource music in SoundManager.MusicSources.Values)
				{
					music.SetAudioVolume(0);
				}
			}
		}
	}
}