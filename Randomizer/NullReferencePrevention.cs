using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using XRL;
using XRL.Annals;
using XRL.Liquids;
using XRL.UI;
using XRL.World;

namespace Mods.Randomizer
{
	// this class contains the most vile code you will ever see!
	[HarmonyPatch]
	public class NullReferencePrevention
	{
		static IEnumerable<MethodBase> TargetMethods()
		{
			Type[] types = typeof(IPart).Assembly.GetTypes();
			for (int i = 0; i < types.Length; i++)
			{
				// skip generic and compiler-generated types, those can't be patched and don't need patching anyway
				if (types[i].IsGenericType || types[i].Name[0] == '<')
					continue;

				if (!types[i].FullName.StartsWith("XRL.World") && types[i] != typeof(PsychicHunterSystem) && types[i] != typeof(LiquidPutrescence) && types[i] != typeof(ImportedFoodorDrink))
					continue;

				MethodInfo[] methods = types[i].GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				for (int j = 0; j < methods.Length; j++)
				{
					if (methods[j].HasMethodBody() && methods[j].GetGenericArguments().Length < 1 && methods[j].DeclaringType == types[i])
					{
						foreach (KeyValuePair<OpCode, object> instr in PatchProcessor.ReadMethodBody(methods[j]))
						{
							if (instr.Key == OpCodes.Callvirt)
							{
								MethodInfo method = (MethodInfo)instr.Value;
								if (method.DeclaringType == typeof(GameObject) && method.Name == "GetPart")
								{
									yield return methods[j];
								}
							}
						}
					}
				}
			}
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			List<GetPartCall> calls = new List<GetPartCall>(16);
			List<string> hasParts = new List<string>(16) { "Physics", "Render", "Description" };

			if (Options.GetOption("FixGasInheritance") == "Yes")
				hasParts.Add("Gas");

			for (int i = 0; i < list.Count; i++)
			{
				if (calls.Count > 0 && i < list.Count - 1)
				{
					if (!IsChecked(LoadLocal(list[i]), false, calls, list[i + 1]))
					{
						IsChecked(LoadArg(list[i]), true, calls, list[i + 1]);
					}
				}

				if (list[i].opcode != OpCodes.Callvirt)
					continue;

				MethodInfo method = (MethodInfo)list[i].operand;
				if (method.DeclaringType != typeof(GameObject))
					continue;

				if (method.Name == "HasPart")
				{
					if (list[i - 1].operand is string name)
						hasParts.Add(name);

					else if (list[i - 1].operand is Type type)
						hasParts.Add(type.Name);

					continue;
				}

				if (method.Name != "GetPart")
					continue;

				Type partType = method.GetGenericArguments().Length > 0 ? method.GetGenericArguments()[0] : null;

				if (hasParts.Contains(partType != null ? partType.Name : list[i - 1].operand as string))
					continue;

				GetPartCall call = new GetPartCall(i, partType, partType != null);
				calls.Add(call);

				if (list[i + 1].opcode == OpCodes.Isinst || list[i + 1].opcode == OpCodes.Castclass)
				{
					if (partType == null)
					{
						call.type = (Type)list[i + 1].operand;
					}
					i++;
				}

				// is the part only checked (AnimatedMaterialSaltDunes) or removed (GameObjectSkillUnit)?
				if (IsChecked(list[i + 1]))
				{
					call.variableIndex = int.MaxValue;
					continue;
				}

				// treat unstored parts as unchecked
				int arg = StoreArg(list[i + 1]);
				if (arg > -1)
				{
					call.variableIndex = arg;
					call.variableIsArgument = true;
					continue;
				}
				call.variableIndex = StoreLocal(list[i + 1]);
			}

			int offset = 0;
			for (int i = 0; i < calls.Count; i++)
			{
				if (calls[i].variableIndex < int.MaxValue)
				{
					int index = calls[i].index + offset;
					list[index].operand = typeof(NullReferencePrevention).GetMethod("RequirePart");

					if (calls[i].type != null)
					{
						string fullName = calls[i].type.FullName;
						list.Insert(index, new CodeInstruction(OpCodes.Ldstr, fullName.Substring(0, fullName.LastIndexOf('.'))));
					}
					else
					{
						list.Insert(index, new CodeInstruction(OpCodes.Ldnull));
					}
					offset++;

					if (calls[i].loadName)
					{
						list.Insert(index, new CodeInstruction(OpCodes.Ldstr, calls[i].type.Name));
						offset++;
					}
				}
			}

			return list;
		}

		public static IPart RequirePart(GameObject obj, string name, string location)
		{
			for (int i = 0; i < obj.PartsList.Count; i++)
			{
				if (obj.PartsList[i].Name == name)
				{
					return obj.PartsList[i];
				}
			}

			// if the part's namespace wasn't found, pray that it's the standard part namespace
			Type type = ModManager.ResolveType(location ?? "XRL.World.Parts", name);
			if (type == null)
				return null;

			IPart part = Activator.CreateInstance(type) as IPart;
			obj.AddPart(part);
			return part;
		}

		private static int LoadLocal(CodeInstruction instr)
		{
			OpCode opcode = instr.opcode;
			if (opcode == OpCodes.Ldloc_0)
				return 0;

			if (opcode == OpCodes.Ldloc_1)
				return 1;

			if (opcode == OpCodes.Ldloc_2)
				return 2;

			if (opcode == OpCodes.Ldloc_3)
				return 3;

			if (opcode == OpCodes.Ldloc || opcode == OpCodes.Ldloca || opcode == OpCodes.Ldloc_S || opcode == OpCodes.Ldloca_S)
				return ((LocalBuilder)instr.operand).LocalIndex;

			return -1;
		}

		private static int StoreLocal(CodeInstruction instr)
		{
			OpCode opcode = instr.opcode;
			if (opcode == OpCodes.Stloc_0)
				return 0;

			if (opcode == OpCodes.Stloc_1)
				return 1;

			if (opcode == OpCodes.Stloc_2)
				return 2;

			if (opcode == OpCodes.Stloc_3)
				return 3;

			if (opcode == OpCodes.Stloc || opcode == OpCodes.Stloc_S)
				return ((LocalBuilder)instr.operand).LocalIndex;

			return -1;
		}

		private static int LoadArg(CodeInstruction instr)
		{
			OpCode opcode = instr.opcode;
			if (opcode == OpCodes.Ldarg_0)
				return 0;

			if (opcode == OpCodes.Ldarg_1)
				return 1;

			if (opcode == OpCodes.Ldarg_2)
				return 2;

			if (opcode == OpCodes.Ldarg_3)
				return 3;

			if (opcode == OpCodes.Ldarg_S || opcode == OpCodes.Ldarga_S || opcode == OpCodes.Ldarg || opcode == OpCodes.Ldarga)
				return Convert.ToInt32(instr.operand);

			return -1;
		}

		private static int StoreArg(CodeInstruction instr)
		{
			if (instr.opcode == OpCodes.Starg_S || instr.opcode == OpCodes.Starg)
				return Convert.ToInt32(instr.operand);

			return -1;
		}

		private static bool IsChecked(int variable, bool isArgument, List<GetPartCall> calls, CodeInstruction next)
		{
			if (variable < 0)
				return false;

			for (int i = 0; i < calls.Count; i++)
			{
				if (calls[i].variableIsArgument == isArgument && calls[i].variableIndex == variable)
				{
					// is the part checked before it is used?
					calls[i].variableIndex = IsChecked(next) ? int.MaxValue : -1;
					return true;
				}
			}
			return false;
		}

		private static bool IsChecked(CodeInstruction instr)
		{
			OpCode opcode = instr.opcode;
			return opcode == OpCodes.Brfalse
				|| opcode == OpCodes.Brfalse_S
				|| opcode == OpCodes.Brtrue
				|| opcode == OpCodes.Brtrue_S
				|| opcode == OpCodes.Ldnull
				|| (opcode == OpCodes.Callvirt && ((MethodInfo)instr.operand).Name.StartsWith("Remove"));
		}
	}

	public class GetPartCall
	{
		public int index;
		public Type type;
		public bool loadName;
		public int variableIndex;
		public bool variableIsArgument;

		public GetPartCall(int index, Type type, bool loadName)
		{
			this.index = index;
			this.type = type;
			this.loadName = loadName;
		}
	}
}