using HarmonyLib;
using System.Reflection;
using XRL.World;

namespace Mods.Randomizer
{
	[HarmonyPatch]
	public class CreateLoopPrevention
	{
		private static int layer;

		static MethodBase TargetMethod()
		{
			MethodInfo[] methods = typeof(GameObjectFactory).GetMethods();
			for (int i = 0; i < methods.Length; i++)
			{
				if (methods[i].Name == "CreateObject" && methods[i].GetParameters()[0].Name == "Blueprint")
				{
					return methods[i];
				}
			}
			return null;
		}

		static bool Prefix()
		{
			if (++layer > 4)
				return false;

			return true;
		}

		static void Postfix()
		{
			layer = 0;
		}
	}
}