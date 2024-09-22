using ConsoleLib.Console;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Parts.Mutation;

namespace XRL.CharacterBuilds.Qud.UI
{
	[UIView("CharacterCreation:Conjoined", NavCategory: "Menu", UICanvas: "Chargen/Conjoined", UICanvasHost: 1)]
	public class QudConjoinedModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudConjoinedModule, CategoryMenusScroller>
	{
		public EmbarkBuilderModuleWindowDescriptor windowDescriptor;
		private List<CategoryMenuData> categoryMenus = new List<CategoryMenuData>();

		public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
		{
			if (descriptor != null)
			{
				windowDescriptor = descriptor;
			}
			prefabComponent.onSelected.RemoveAllListeners();
			prefabComponent.onSelected.AddListener(SelectCreature);
			UpdateControls();
		}

		public void SelectCreature(FrameworkDataElement dataElement)
		{
			if (module.data.selections.Count < module.data.max)
			{
				module.data.selections.Add(dataElement.Id);
			}
			else
			{
				module.data.selections.RemoveAll(i => i == dataElement.Id);
			}
			UpdateControls();
		}

		public override void ResetSelection()
		{
			module.setData(new QudConjoinedModuleData());
			UpdateControls();
		}

		public override void RandomSelection()
		{
			while (module.data.selections.Count < module.data.max)
			{
				List<GameObjectBlueprint> creatures = Conjoined.GetAvailableCreatures(true);
				module.data.selections.Add(creatures[Stat.Rand.Next(creatures.Count)].Name);
			}
			UpdateControls();
		}

		public void UpdateControls()
		{
			categoryMenus = new List<CategoryMenuData>();

			CategoryMenuData menu = new CategoryMenuData()
			{
				Title = "Creatures"
			};
			menu.menuOptions = new List<PrefixMenuOption>();

			List<GameObjectBlueprint> creatures = Conjoined.GetAvailableCreatures(true);

			for (int i = 0; i < creatures.Count; i++)
			{
				int amount = 0;

				for (int j = 0; j < module.data.selections.Count; j++)
				{
					if (module.data.selections[j] == creatures[i].Name)
					{
						amount++;
					}
				}

				int level = creatures[i].GetStat("Level", new Statistic()).Value;

				GamePartBlueprint cherub = creatures[i].GetPart("CherubimSpawner");
				if (cherub != null)
				{
					int num = int.Parse(cherub.GetParameterString("Period"));
					bool inorganic = creatures[i].HasProperty("Inorganic");
					menu.menuOptions.Add(new PrefixMenuOption()
					{
						Id = creatures[i].Name,
						Prefix = amount > 0 ? "[" + amount + "]" : "[ ]",
						Description = (inorganic ? "mechanical cherub (" : "cherub (") + num + cherub.GetParameterString("Group") + ") [Lv." + level + "]",
						LongDescription = "?????\n\n{{R|??? HP}} {{b|?? AV}} {{O|+? DV}}\n{{G|??? QN}} {{c|??? MS}}\n\n{{y|?? Strength}} {{W|?? Agility}}\n{{r|?? Toughness}} {{B|?? Intelligence}}\n{{g|?? Willpower}} {{M|?? Ego}}",
						Renderable = new Renderable()
						{
							Tile = "Terrain/sw_sultanstatue_" + num + ".bmp",
							TileColor = "&Y",
							DetailColor = 'y'
						}
					});
				}
				else
				{
					string role = creatures[i].GetProp("Role");

					string displayName = creatures[i].DisplayName() + " [Lv." + level + "]";

					StringBuilder description = new StringBuilder();
					description.Append(GameText.VariableReplace(creatures[i].GetPartParameter<string>("Description", "Short"), ExplicitSubject: displayName));

					description.Append("\n\n{{R|");
					description.Append(Math.Min(creatures[i].GetStat("Hitpoints", new Statistic()).Value, 15 + 3 * level));
					description.Append(" HP}} {{b|");

					description.Append(creatures[i].GetStat("AV", new Statistic()).Value);
					description.Append(" AV}}");

					int dv = creatures[i].GetStat("DV", new Statistic()).Value;
					if (dv != 0)
					{
						description.Append(" {{O|");

						if (dv > 0)
						{
							description.Append('+');
						}

						description.Append(dv);
						description.Append(" DV}}");
					}

					description.Append("\n{{G|");

					description.Append(creatures[i].GetStat("Speed", new Statistic()).Value);
					description.Append(" QN}} {{c|");

					description.Append(200 - creatures[i].GetStat("MoveSpeed", new Statistic()).Value);
					description.Append(" MS}}\n\n{{y|");

					description.Append(GetStatRange(creatures[i].GetStat("Strength", new Statistic()), level, role));
					description.Append(" Strength}} {{W|");

					description.Append(GetStatRange(creatures[i].GetStat("Agility", new Statistic()), level, role));
					description.Append(" Agility}}\n{{r|");

					description.Append(GetStatRange(creatures[i].GetStat("Toughness", new Statistic()), level, role));
					description.Append(" Toughness}} {{B|");

					description.Append(GetStatRange(creatures[i].GetStat("Intelligence", new Statistic()), level, role));
					description.Append(" Intelligence}}\n{{g|");

					description.Append(GetStatRange(creatures[i].GetStat("Willpower", new Statistic()), level, role));
					description.Append(" Willpower}} {{M|");

					description.Append(GetStatRange(creatures[i].GetStat("Ego", new Statistic()), level, role));
					description.Append(" Ego}}");

					menu.menuOptions.Add(new PrefixMenuOption()
					{
						Id = creatures[i].Name,
						Prefix = amount > 0 ? "[" + amount + "]" : "[ ]",
						Description = displayName,
						LongDescription = description.ToString(),
						Renderable = new Renderable()
						{
							Tile = creatures[i].GetPartParameter<string>("Render", "Tile"),
							TileColor = creatures[i].GetPartParameter<string>("Render", "TileColor", "&y"),
							DetailColor = creatures[i].GetPartParameter<string>("Render", "DetailColor", "y")[0]
						}
					});
				}
			}

			categoryMenus.Add(menu);

			prefabComponent.BeforeShow(windowDescriptor, categoryMenus);
			GetOverlayWindow().UpdateMenuBars(windowDescriptor);
		}

		private string GetStatRange(Statistic stat, int level, string role = null)
		{
			int min = 0;
			int max = 0;
			int boost = stat.Boost;
			if (role == "Minion")
			{
				boost--;
			}

			level = level / 5 + 1;

			if (stat.sValue.Contains(","))
			{
				string[] array = stat.sValue.Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = GetStatRange(array[i]);
					min += array[i].RollMin();
					max += array[i].RollMax();
				}
			}
			else
			{
				string range = GetStatRange(stat.sValue);
				min += range.RollMin();
				max += range.RollMax();
			}

			if (boost > 0)
			{
				min += (int)Math.Ceiling(min * 0.25f * boost);
				max += (int)Math.Ceiling(max * 0.25f * boost);
			}
			else
			{
				min += (int)Math.Ceiling(min * 0.2f * boost);
				max += (int)Math.Ceiling(max * 0.2f * boost);
			}

			if (min == max)
			{
				return min.ToString();
			}

			return min + "-" + max;

			string GetStatRange(string dice)
			{
				if (dice.Contains("("))
				{
					if (dice.Contains("(t)"))
					{
						dice = dice.Replace("(t)", level.ToString());
					}
					if (dice.Contains("(t-1)"))
					{
						dice = dice.Replace("(t-1)", (level - 1).ToString());
					}
					if (dice.Contains("(t+1)"))
					{
						dice = dice.Replace("(t+1)", (level + 1).ToString());
					}
					dice = dice.Replace("(v)", stat.BaseValue.ToString());
				}
				return dice;
			}
		}

		public override UIBreadcrumb GetBreadcrumb()
		{
			return new UIBreadcrumb()
			{
				Id = GetType().FullName,
				Title = "Conjoined",
				IconPath = "Conjoined.png",
				IconDetailColor = Color.clear,
				IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap['y']
			};
		}
	}
}