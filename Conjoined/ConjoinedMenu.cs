using ConsoleLib.Console;
using System.Collections.Generic;
using XRL.Language;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.UI
{
	[UIView("ConjoinedMenu", NavCategory: "Menu")]
	public class ConjoinedMenu : IScreen, IWantsTextConsoleInit
	{
		private static TextConsole console;
		private static ScreenBuffer buffer;

		private const int RowWidth = 74;

		public void Init(TextConsole textConsole, ScreenBuffer screenBuffer)
		{
			console = textConsole;
			buffer = screenBuffer;
		}

		private void Draw(ConsoleChar c, GameObjectBlueprint creature)
		{
			GamePartBlueprint cherub = creature.GetPart("CherubimSpawner");
			if (cherub != null)
			{
				c.Tile = "Terrain/sw_sultanstatue_" + int.Parse(cherub.GetParameterString("Period")) + ".bmp";
				c.SetForeground('Y');
				c.SetDetail('y');
				return;
			}
			c.Tile = creature.GetPartParameter<string>("Render", "Tile");
			c.SetForeground(ColorUtility.ParseForegroundColor(creature.GetPartParameter<string>("Render", "ColorString", "&y")));
			c.SetDetail(creature.GetPartParameter<string>("Render", "DetailColor", "y")[0]);
		}

		private string GetName(GameObjectBlueprint creature)
		{
			GamePartBlueprint cherub = creature.GetPart("CherubimSpawner");
			if (cherub != null)
			{
				return (creature.HasProperty("Inorganic") ? "mechanical cherub (" : "cherub (") + int.Parse(cherub.GetParameterString("Period")) + cherub.GetParameterString("Group") + ")";
			}
			return creature.DisplayName();
		}

		public ScreenReturn Show(GameObject GO)
		{
			GameManager.Instance.PushGameView("ConjoinedMenu");

			List<GameObjectBlueprint> creatures = Conjoined.GetAvailableCreatures(GO.GetIntProperty("Inorganic") == 0);

			BaseMutation mutation = GO.GetPart<Mutations>().GetMutation("Conjoined");
			int selected = 0;
			int[] chosen = new int[mutation.Level];
			int index = 0;

			bool done = false;
			while (!done)
			{
				buffer.Clear();
				buffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));

				string str;
				if (chosen.Length > 1)
				{
					str = "Choose the creature conjoined to you.";
				}
				else
				{
					str = "Choose the " + Grammar.Ordinal(index + 1) + " creature conjoined to you.";

					for (int i = 0; i < chosen.Length; i++)
					{
						ConsoleChar c = buffer[40 - chosen.Length / 2 + i, 7];

						if (chosen[i] == 0)
						{
							c.Char = '.';
							c.SetForeground('K');
							continue;
						}
						Draw(c, creatures[chosen[i] - 1]);
					}
				}

				buffer.Goto(40 - ColorUtility.LengthExceptFormatting(str) / 2, 4);
				buffer.Write(str);

				str = GetName(creatures[selected]) + " [Lv." + creatures[selected].GetStat("Level", new Statistic()).Value + "]";

				buffer.Goto(40 - ColorUtility.LengthExceptFormatting(str) / 2, 8);
				buffer.Write(str);

				buffer.Goto(4, 20);
				buffer.Write("&W8&y-Up &W2&y-Down &W4&y-Left &W6&y-Right");

				buffer.Goto(46, 20);
				buffer.Write("&Wspace&y-Select &WESC&y-Undo &Wr&y-Random");

				for (int i = 0; i < creatures.Count; i++)
				{
					ConsoleChar c = buffer[3 + i % RowWidth, 11 + i / RowWidth];
					Draw(c, creatures[i]);
					if (i == selected)
					{
						c.SetForeground('K');
						c.SetBackground('Y');
						c.SetDetail('K');
					}
				}

				console.DrawBuffer(buffer);

				switch (Keyboard.getvk(Options.MapDirectionsToKeypad))
				{
					case Keys.NumPad4:
						selected--;
						if (selected < 0)
						{
							selected = creatures.Count - 1;
						}
						break;
					case Keys.NumPad6:
						selected++;
						if (selected >= creatures.Count)
						{
							selected = 0;
						}
						break;
					case Keys.NumPad8:
						if (selected - RowWidth < 0)
						{
							selected = (creatures.Count - 1 - selected % RowWidth) / RowWidth * RowWidth + selected % RowWidth;
							break;
						}
						selected -= RowWidth;
						break;
					case Keys.NumPad2:
						if (selected + RowWidth >= creatures.Count)
						{
							selected %= RowWidth;
							break;
						}
						selected += RowWidth;
						break;
					case Keys.Space:
					case Keys.Enter:
						chosen[index] = selected + 1;

						if (++index >= chosen.Length)
						{
							done = true;
						}
						break;
					case Keys.Escape:
						if (index > 0)
						{
							index--;
							chosen[index] = 0;
						}
						break;
					case Keys.R:
						int rand = Stat.Rand.Next(creatures.Count);
						chosen[index] = rand + 1;

						if (++index >= chosen.Length)
						{
							done = true;
						}
						break;
				}
			}

			Conjoined conjoined = (Conjoined)mutation;
			for (int i = 0; i < chosen.Length; i++)
			{
				conjoined.CreateCreature(i, creatures[chosen[i] - 1].Name);
				conjoined.SpawnCreature(i);
			}

			return ScreenReturn.Exit;
		}
	}
}