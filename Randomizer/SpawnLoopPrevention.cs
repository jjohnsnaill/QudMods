using HarmonyLib;
using System.Reflection;
using XRL.World;

namespace Mods.Randomizer
{
	[HarmonyPatch]
	public class SpawnLoopPrevention
	{
		private static int layer;

		static MethodBase TargetMethod()
		{
			MethodInfo[] methods = typeof(Cell).GetMethods();
			for (int i = 0; i < methods.Length; i++)
			{
				if (methods[i].Name == "AddObject" && methods[i].GetParameters()[0].ParameterType == typeof(GameObject))
				{
					return methods[i];
				}
			}
			return null;
		}

		static bool Prefix()
		{
			if (++layer > 2)
				return false;

			return true;
		}

		static void Postfix()
		{
			layer = 0;
		}
	}
}