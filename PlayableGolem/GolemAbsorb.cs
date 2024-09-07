using ConsoleLib.Console;
using Genkit;
using Qud.API;
using Qud.UI;
using System;
using System.Collections.Generic;
using System.Text;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.AI;
using XRL.World.Parts;
using XRL.World.Quests.GolemQuest;
using XRL.World.Units;

namespace Mods.PlayableGolem
{
	[Serializable]
	public class GolemAbsorb : IPart
	{
		public int cost;

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("UsedMP");
			base.Register(Object, Registrar);
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == GetLevelUpPointsEvent.ID; // || ID == WasReplicatedEvent.ID;
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "UsedMP" && E.GetParameter<string>("Context") != "BuyNew")
			{
				ParentObject.ModIntProperty("SpentMP", E.GetParameter<int>("Amount"));
			}
			return base.FireEvent(E);
		}

		public override bool HandleEvent(GetLevelUpPointsEvent E)
		{
			E.MutationPoints++;

			if ((E.Level + 5) % 10 == 0 && !E.Actor.IsEsper())
			{
				E.RapidAdvancement += 3;
			}

			return base.HandleEvent(E);
		}

		// fixes GameObjectCloneUnit creating an unallied golem with a duplicated inventory
		/*public override bool HandleEvent(WasReplicatedEvent E)
		{
			if (E.Context == "Unit")
			{
				E.Replica.StripContents(true, true);
				E.Replica.RemoveStringProperty("OriginalPlayerBody");
				E.Replica.SetAlliedLeader<AllyClone>(ParentObject);
			}
			return base.HandleEvent(E);
		}*/

		public static bool Absorb(GameObject golem, int cost)
		{
			List<string> list = new List<string>
			{
				"Catalyst",
				"Atzmus",
				"Incantation",
				"Hamsa"
			};

			string desc = null;
			RulesDescription rules = golem.GetPart<RulesDescription>();
			StringBuilder result = Event.NewStringBuilder();

			while (true)
			{
				int option = Popup.ShowOptionList("Choose a power", list, AllowEscape: true);

				if (option < 0)
				{
					return false;
				}
				else if (option == 0)
				{
					var mat = Pick(GetValidCatalysts(golem), golem, "You do not have 3 drams of any compatible liquid.", DisplayCatalyst);
					if (mat == null)
					{
						continue;
					}

					foreach (GameObjectUnit unit in GolemMaterialSelection<string, string>.Units[mat](mat))
					{
						PlayableGolemModule.AddEffect(golem, unit, rules, result);
					}
					desc = "\n=pronouns.Possessive= veins course with " + (LiquidVolume.GetLiquid(mat)?.Name ?? mat) + ".";

					golem.UseDrams(3, mat);
				}
				else if (option == 1)
				{
					List<GameObject> valid = new List<GameObject>();
					foreach (Cell c in golem.CurrentCell.GetLocalAdjacentCells())
					{
						IList<GameObject> objects = c.IsSolidFor(golem) ? c.GetCanInteractInCellWithSolidObjectsFor(golem) : c.Objects;
						for (int i = 0; i < objects.Count; i++)
						{
							GameObject obj = objects[i];
							if (obj.Physics == null || !obj.Physics.IsReal)
							{
								continue;
							}
							if (!obj.IsVisible() || !obj.HasTag("Creature") || !obj.HasPart("Body"))
							{
								continue;
							}
							valid.Add(obj);
						}
					}

					var mat = Pick(valid, golem, "You are not next to any compatible units to copy.", DisplayAtzmus);
					if (mat == null)
					{
						continue;
					}

					PlayableGolemModule.AddEffect(golem, PlayableGolemModule.GetAtzmus(mat).GetRandomElement(), rules, result);
					desc = "\n=pronouns.Possessive= posture evokes the presence of " + mat.an(AsIfKnown: true, NoConfusion: true) + ".";
				}
				else if (option == 2)
				{
					var mat = Pick(GetValidIncantations(), golem, "You have no compatible entries in your chronology.", DisplayIncantation);
					if (mat == null)
					{
						continue;
					}

					foreach (GameObjectUnit unit in GolemMaterialSelection<JournalAccomplishment, MuralCategory>.Units[mat.MuralCategory](mat))
					{
						PlayableGolemModule.AddEffect(golem, unit, rules, result);
					}
					desc = "\n=pronouns.Possessive= ears resonate with an incantation.";

					mat.Forget();
				}
				else if (option == 3)
				{
					List<GameObject> valid = new List<GameObject>();
					golem.Inventory.GetObjects(valid, IsValidHamsa);

					var mat = Pick(valid, golem, "You have no compatible items that weigh 5 lbs. or less.", DisplayHamsa);
					if (mat == null)
					{
						continue;
					}

					PlayableGolemModule.AddEffect(golem, GetHamsa(mat).GetRandomElement(), rules, result);
					desc = "\n=pronouns.Subjective= =verb:bear:afterpronoun= the sacred amulet of " + mat.an(AsIfKnown: true, NoConfusion: true) + ".";

					mat.SplitStack(1);
					mat.Obliterate();
				}

				golem.GetPart<Description>()._Short += desc;

				if (golem.TryGetIntProperty("SpentMP", out int discount))
				{
					if (discount >= cost)
					{
						golem.ModIntProperty("SpentMP", -cost, true);
					}
					else
					{
						golem.UseMP(cost - discount, "BuyNew");
						golem.RemoveIntProperty("SpentMP");
					}
				}
				else
				{
					golem.UseMP(cost, "BuyNew");
				}
				SoundManager.PlaySound("Sounds/StatusEffects/sfx_statusEffect_neutral-weirdVitality");

				Popup.Show("{{G|Absorbed!}}\n\n" + Event.FinalizeString(result));

				return true;
			}
		}

		public static List<string> GetValidCatalysts(GameObject holder)
		{
			List<string> list = new List<string>();

			GetFreeDramsEvent getFreeDrams = GetFreeDramsEvent.FromPool();
			foreach (string liquid in GolemMaterialSelection<string, string>.Units.Keys)
			{
				getFreeDrams.Actor = holder;
				getFreeDrams.Drams = 0;
				getFreeDrams.Liquid = liquid;
				holder.HandleEvent(getFreeDrams);
				if (getFreeDrams.Drams >= 3)
				{
					list.Add(liquid);
				}
			}

			return list;
		}

		public static List<JournalAccomplishment> GetValidIncantations()
		{
			List<JournalAccomplishment> list = new List<JournalAccomplishment>();

			foreach (JournalAccomplishment entry in JournalAPI.Accomplishments)
			{
				if (entry.Revealed && !entry.Text.IsNullOrEmpty() && entry.MuralCategory != MuralCategory.Generic)
				{
					list.Add(entry);
				}
			}

			return list;
		}

		public static void DisplayCatalyst(string liquid, GameObject golem, StringBuilder sb, List<string> options, List<IRenderable> icons)
		{
			sb.Clear().Append("3 drams of ").Append(LiquidVolume.GetLiquid(liquid)?.Name ?? liquid);

			foreach (GameObjectUnit unit in GolemMaterialSelection<string, string>.Units[liquid](liquid))
			{
				sb.Append('\n').Append(PlayableGolemModule.Tweak(unit, golem, 0).GetDescription());
			}

			options.Add(sb.ToString());

			// this could be done better but this is how vanilla does it
			GameObject obj = GameObjectFactory.Factory.CreateSampleObject("DeepFreshWaterPool");
			LiquidVolume display = obj.LiquidVolume;
			display.ComponentLiquids.Clear();
			display.ComponentLiquids[liquid] = 1000;
			display.LastPaintMask = -1;
			display.RecalculatePrimary();
			display.Paint(0);
			icons.Add(new Renderable(obj.Render));
		}

		public static void DisplayAtzmus(GameObject obj, GameObject golem, StringBuilder sb, List<string> options, List<IRenderable> icons)
		{
			sb.Clear().Append(obj.DisplayName);

			bool first = true;
			foreach (GameObjectUnit unit in PlayableGolemModule.GetAtzmus(obj))
			{
				sb.Append('\n').Append(first ? "{{rules|--}} " : "{{rules|OR}} ").Append(PlayableGolemModule.Tweak(unit, golem, 0).GetDescription());
				first = false;
			}

			options.Add(sb.ToString());
			icons.Add(obj.RenderForUI());
		}

		public static void DisplayIncantation(JournalAccomplishment entry, GameObject golem, StringBuilder sb, List<string> options, List<IRenderable> icons)
		{
			sb.Clear().Append(entry.Text);

			foreach (GameObjectUnit unit in GolemMaterialSelection<JournalAccomplishment, MuralCategory>.Units[entry.MuralCategory](entry))
			{
				sb.Append('\n').Append(PlayableGolemModule.Tweak(unit, golem, 0).GetDescription());
			}

			options.Add(sb.ToString());

			Renderable render = new Renderable();
			render.ColorString = "&Y";
			render.TileColor = "&Y";

			Random rand = new Random((int)Hash.FNV1A32(entry.Text));
			render.Tile = rand.Next(4) switch
			{
				0 => "items/sw_unfurled_scroll1.bmp",
				1 => "items/sw_unfurled_scroll2.bmp",
				2 => "items/sw_scroll1.bmp",
				_ => "items/sw_scroll2.bmp",
			};
			int color = rand.Next(13);
			if (color >= 6)
			{
				color++;
			}
			render.DetailColor = Crayons.AllColors[color][0];

			icons.Add(render);
		}

		public static void DisplayHamsa(GameObject obj, GameObject golem, StringBuilder sb, List<string> options, List<IRenderable> icons)
		{
			sb.Clear().Append(obj.DisplayName);

			bool first = true;
			foreach (string tag in obj.GetBlueprint().Tags.Keys)
			{
				if (!tag.StartsWith("Semantic"))
				{
					continue;
				}
				if (!GolemMaterialSelection<GameObject, string>.Units.TryGetValue(tag.Substring(8), out var value))
				{
					continue;
				}
				foreach (GameObjectUnit unit in value(obj))
				{
					sb.Append('\n').Append(first ? "{{rules|--}} " : "{{rules|OR}} ").Append(PlayableGolemModule.Tweak(unit, golem, 0).GetDescription());
				}
				first = false;
			}

			options.Add(sb.ToString());
			icons.Add(obj.RenderForUI());
		}

		public static bool IsValidHamsa(GameObject obj)
		{
			if (obj.WeightEach > 5)
			{
				return false;
			}
			foreach (string tag in obj.GetBlueprint().Tags.Keys)
			{
				if (tag.StartsWith("Semantic"))
				{
					return true;
				}
			}
			return false;
		}

		public static List<GameObjectUnit> GetHamsa(GameObject hamsa)
		{
			List<GameObjectUnit> list = new List<GameObjectUnit>();

			foreach (string tag in hamsa.GetBlueprint().Tags.Keys)
			{
				if (!tag.StartsWith("Semantic"))
				{
					continue;
				}
				if (!GolemMaterialSelection<GameObject, string>.Units.TryGetValue(tag.Substring(8), out var value))
				{
					continue;
				}
				foreach (GameObjectUnit unit in value(hamsa))
				{
					list.Add(unit);
				}
			}

			// the item may have no valid effect, so add a failsafe
			if (list.Count < 1)
			{
				list.Add(new GameObjectAttributeUnit
				{
					Attribute = "All",
					Value = 1 //2
				});
			}
			return list;
		}

		// GolemMaterialSelection.Pick
		public static T Pick<T>(List<T> valid, GameObject golem, string fail, Action<T, GameObject, StringBuilder, List<string>, List<IRenderable>> display) where T : class
		{
			if (valid.IsNullOrEmpty())
			{
				Popup.ShowFail(fail);
				return null;
			}
			int pos = 0;
			int page = 250;
			int capacity = Math.Min(valid.Count, page);
			List<QudMenuItem> list = new List<QudMenuItem>(2);
			QudMenuItem prev = new QudMenuItem
			{
				text = "Previous Page",
				command = "option:-2"
			};
			QudMenuItem next = new QudMenuItem
			{
				text = "Next Page",
				command = "option:-3"
			};
			StringBuilder sb = Event.NewStringBuilder();
			List<string> options = new List<string>(capacity);
			List<IRenderable> icons = new List<IRenderable>(capacity);
			int option;
			while (true)
			{
				options.Clear();
				icons.Clear();
				int num3 = Math.Min(valid.Count - pos, page);
				for (int i = pos; i < pos + num3; i++)
				{
					display(valid[i], golem, sb, options, icons);
				}
				list.Clear();
				if (pos > 0)
				{
					list.Add(prev);
				}
				if (valid.Count - pos > page)
				{
					list.Add(next);
				}
				option = Popup.ShowOptionList("Choose what to absorb", options, null, 0, null, 60, RespectOptionNewlines: true, AllowEscape: true, 0, "", null, null, icons, null, list);
				if (option == -2)
				{
					pos = Math.Max(pos - page, 0);
					continue;
				}
				if (option != -3)
				{
					break;
				}
				pos += page;
			}
			if (option >= 0)
			{
				return valid[pos + option];
			}
			return null;
		}
	}
}