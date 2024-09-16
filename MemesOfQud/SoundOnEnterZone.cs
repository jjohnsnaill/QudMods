using HarmonyLib;
using System;
using System.Reflection;
using XRL.World;

namespace Mods.MemesOfQud
{
	public class SoundOnEnterZone : IPart
	{
		[NonSerialized]
		public string sound;
		[NonSerialized]
		public Zone previousZone;

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == EnteredCellEvent.ID;
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool HandleEvent(EnteredCellEvent E)
		{
			if (previousZone != E.Cell.ParentZone)
			{
				E.Cell.PlayWorldSound(sound);
				previousZone = E.Cell.ParentZone;
			}
			return true;
		}

		public override void Write(GameObject Basis, SerializationWriter Writer)
		{
			//TODO: don't save this part at all
		}

		public override void Read(GameObject Basis, SerializationReader Reader)
		{
			string species = ParentObject.GetPropertyOrTag("Species");
			if (species == "cat")
			{
				sound = "pantherk";
			}
			else if (species == "crab")
			{
				sound = "crab";
			}
			else if (species == "pig")
			{
				sound = "swine";
			}
			else if (species == "tortoise")
			{
				sound = "testudine";
			}
			previousZone = ParentObject.CurrentZone;
		}
	}

	[HarmonyPatch]
	public class AddPartOnCreate
	{
		static MethodBase TargetMethod()
		{
			MethodInfo[] methods = typeof(GameObjectFactory).GetMethods();
			for (int i = 0; i < methods.Length; i++)
			{
				if (methods[i].Name == "CreateObject" && methods[i].GetParameters()[0].ParameterType == typeof(GameObjectBlueprint))
				{
					return methods[i];
				}
			}
			return null;
		}

		static void Postfix(GameObjectBlueprint Blueprint, GameObject __result)
		{
			if (Blueprint == null || __result == null)
			{
				return;
			}

			string species = Blueprint.GetTag("Species");
			if (species == "cat")
			{
				__result.AddPart<SoundOnEnterZone>().sound = "pantherk";
			}
			else if (species == "crab")
			{
				__result.AddPart<SoundOnEnterZone>().sound = "crab";
			}
			else if (species == "pig")
			{
				__result.AddPart<SoundOnEnterZone>().sound = "swine";
			}
			else if (species == "tortoise")
			{
				__result.AddPart<SoundOnEnterZone>().sound = "testudine";
			}
		}
	}
}