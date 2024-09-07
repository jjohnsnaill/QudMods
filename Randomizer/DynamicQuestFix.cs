using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using XRL.World;

namespace Mods.Randomizer
{
	[HarmonyPatch]
	public class DynamicQuestFix
	{
		static IEnumerable<MethodBase> TargetMethods()
		{
			yield return typeof(VillageDynamicQuestContext).GetMethod("getQuestGenericRemoteInteractable");
			yield return typeof(VillageDynamicQuestContext).GetMethod("getQuestRemoteInteractable");
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Callvirt, typeof(GameObject).GetMethod("GetTag")))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Pop));
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldstr, (string)list[i - 2].operand == "QuestableVerb" ? "look at" : "LookedAt"));
					i += 2;
				}
			}
			return list;
		}
	}
}