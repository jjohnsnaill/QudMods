using HarmonyLib;
using XRL.Rules;
using XRL.Sound;
using XRL.UI;
using XRL.World.Conversations;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(IConversationElement))]
	public class ConversationSounds
	{
		[HarmonyPrefix]
		[HarmonyPatch("Entered")]
		static void Entered(IConversationElement __instance)
		{
			if (!Options.Sound)
			{
				return;
			}

			if (__instance is Conversation)
			{
				SoundManager.PlaySound("convoStart");
				return;
			}

			if (__instance.ID == "Hair")
			{
				SoundManager.PlaySound("hair", Volume: 0.75f);
				foreach (MusicSource music in SoundManager.MusicSources.Values)
				{
					music.SetAudioVolume(0);
				}
				return;
			}

			if (__instance is Choice)
			{
				switch (Stat.Rnd2.Next(3))
				{
					case 0:
						SoundManager.PlaySound("convo1");
						break;
					case 1:
						SoundManager.PlaySound("convo2");
						break;
					case 2:
						SoundManager.PlaySound("convo3");
						break;
				}
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch("Left")]
		static void Left(IConversationElement __instance)
		{
			if (Options.Sound && __instance is Conversation)
			{
				SoundManager.PlaySound("convoEnd");
			}
		}
	}
}