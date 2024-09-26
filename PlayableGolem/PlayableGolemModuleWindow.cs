using ConsoleLib.Console;
using System;
using System.Collections.Generic;
using System.Text;
using XRL;
using XRL.CharacterBuilds;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Parts.Mutation;
using XRL.World.Quests.GolemQuest;

namespace Mods.PlayableGolem
{
	[UIView("CharacterCreation:PlayableGolem", NavCategory: "Chargen", UICanvas: "Chargen/PlayableGolem", UICanvasHost: 1)]
	public class PlayableGolemModuleWindow : EmbarkBuilderModuleWindowPrefabBase<PlayableGolemModule, HorizontalScroller>
	{
		public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
		{
			prefabComponent.onSelected.RemoveAllListeners();
			prefabComponent.onSelected.AddListener(SelectBody);
			prefabComponent.BeforeShow(descriptor, GetBodies());
			base.BeforeShow(descriptor);
		}

		public override void ResetSelection()
		{
			module.setData(new PlayableGolemModuleData());
		}

		public void SelectBody(FrameworkDataElement choice)
		{
			module.data.selection = choice.Id;
			module.builder.advance();
		}

		public override void RandomSelection()
		{
			int index = Stat.Rnd.Next(prefabComponent.choices.Count - 1);
			prefabComponent.scrollContext.GetContextAt(index).ActivateAndEnable();
			SelectBody(prefabComponent.choices[index]);
		}

		public IEnumerable<ChoiceWithColorIcon> GetBodies()
		{
			foreach (string bp in GolemBodySelection.GetBodyBySpecies().Values)
			{
				GameObjectBlueprint body = GameObjectFactory.Factory.Blueprints[bp];

				string displayName = body.DisplayName();

				StringBuilder description = new StringBuilder();

				description.Append("{{b|");

				int av = body.GetStat("AV", new Statistic()).Value;
				if (av > 4)
				{
					av = 4;
				}
				description.Append(av);
				description.Append(" AV}} {{G|");

				description.Append(body.GetStat("Speed", new Statistic()).Value);
				description.Append(" QN}} {{c|");

				description.Append(200 - body.GetStat("MoveSpeed", new Statistic()).Value);
				description.Append(" MS}}\n{{y|");

				description.Append(GetStatRange(body.GetStat("Strength", new Statistic()), 1));
				description.Append(" STR}} {{W|");

				description.Append(GetStatRange(body.GetStat("Agility", new Statistic()), 1));
				description.Append(" AGI}} {{r|");

				description.Append(GetStatRange(body.GetStat("Toughness", new Statistic()), 1, body.GetStat("Hitpoints", new Statistic()).BaseValue / 500 * 2 - 2));
				description.Append(" TOU}}\n{{B|");

				description.Append(GetStatRange(body.GetStat("Intelligence", new Statistic()), 1));
				description.Append(" INT}} {{g|");

				description.Append(GetStatRange(body.GetStat("Willpower", new Statistic()), 1));
				description.Append(" WIL}} {{M|");

				description.Append(GetStatRange(body.GetStat("Ego", new Statistic()), 1));
				description.Append(" EGO}}");

				bool more = false;
				foreach (var mutation in body.Mutations)
				{
					if (mutation.Key == "DarkVision" || mutation.Key == "NightVision")
						continue;

					if (!more)
					{
						description.Append('\n');
						more = true;
					}
					else
					{
						description.Append(", ");
					}
					description.Append(((BaseMutation)mutation.Value.Reflector?.GetNewInstance()).DisplayName);
				}

				yield return new ChoiceWithColorIcon()
				{
					Id = bp,
					Title = displayName,
					Description = description.ToString(),
					IconPath = body.GetPartParameter<string>("Render", "Tile"),
					IconForegroundColor = ColorUtility.ColorMap[body.GetPartParameter("Render", "TileColor", "&y")[1]],
					IconDetailColor = ColorUtility.ColorMap[body.GetPartParameter("Render", "DetailColor", "y")[0]],
					Chosen = (choice) => module.data?.selection == choice.Id
				};
			}
		}

		private string GetStatRange(Statistic stat, int level, int add = 0)
		{
			int min = add;
			int max = add;
			int boost = stat.Boost;

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
				Title = "Body",
				IconPath = "Creatures/sw_golem_mound.bmp",
				IconDetailColor = UnityEngine.Color.clear,
				IconForegroundColor = ColorUtility.ColorMap['y']
			};
		}
	}
}