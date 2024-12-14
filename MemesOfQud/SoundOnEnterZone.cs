using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.World;
using XRL.World.Parts;

namespace Mods.MemesOfQud
{
	[HarmonyPatch]
	public class SoundOnEnterZone
	{
		[HarmonyPatch(typeof(Physics))]
		[HarmonyPatch("EnterCell")]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Call, typeof(Physics).GetMethod("set_CurrentCell")))
				{
					list.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 1, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_1));
					list.Insert(i + 3, CodeInstruction.Call(typeof(SoundOnEnterZone), "PlaySound"));

					break;
				}
			}
			return list;
		}

		private static void PlaySound(GameObject obj, Cell cell)
		{
			string species = obj.GetPropertyOrTag("Species");
			if (species == "cat")
			{
				if (cell.ParentZone.ZoneID != obj.GetStringProperty("LastPlayedSoundIn"))
				{
					cell.PlayWorldSound("pantherk");
					obj.SetStringProperty("LastPlayedSoundIn", cell.ParentZone.ZoneID);
				}
			}
			else if (species == "crab")
			{
				if (cell.ParentZone.ZoneID != obj.GetStringProperty("LastPlayedSoundIn"))
				{
					cell.PlayWorldSound("crab");
					obj.SetStringProperty("LastPlayedSoundIn", cell.ParentZone.ZoneID);
				}
			}
			else if (species == "pig")
			{
				if (cell.ParentZone.ZoneID != obj.GetStringProperty("LastPlayedSoundIn"))
				{
					cell.PlayWorldSound("swine");
					obj.SetStringProperty("LastPlayedSoundIn", cell.ParentZone.ZoneID);
				}
			}
			else if (species == "tortoise")
			{
				if (cell.ParentZone.ZoneID != obj.GetStringProperty("LastPlayedSoundIn"))
				{
					cell.PlayWorldSound("testudine");
					obj.SetStringProperty("LastPlayedSoundIn", cell.ParentZone.ZoneID);
				}
			}
		}
	}
}