using System.Collections.Generic;
using System;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;
using Qud.API;
using System.Text;
using XRL.World.Quests.GolemQuest;
using XRL.World.Units;

namespace Mods.Gauntlet
{
	public static class GauntletUtils
	{
		public static void AddMutations(GameObject unit, List<MutationDefinition> mutations)
		{
			if (mutations == null)
			{
				return;
			}

			for (int i = 0; i < mutations.Count; i++)
			{
				unit.GetPart<Mutations>().AddMutation(BaseMutation.Create(mutations[i].type, mutations[i].variant), mutations[i].level);
			}
		}

		public static void AddSkills(GameObject unit, List<Type> skills)
		{
			if (skills == null)
			{
				return;
			}

			for (int i = 0; i < skills.Count; i++)
			{
				unit.GetPart<Skills>().AddSkill((BaseSkill)Activator.CreateInstance(skills[i]));
			}
		}

		public static void AddParts(GameObject unit, List<Type> parts)
		{
			if (parts == null)
			{
				return;
			}

			for (int i = 0; i < parts.Count; i++)
			{
				unit.AddPart((IPart)Activator.CreateInstance(parts[i]), Creation: true);
			}
		}

		public static void RestockAmmo(GameObject obj, GameObject item, int tier)
		{
			for (int i = 0; i < item.PartsList.Count; i++)
			{
				if (item.PartsList[i] is MagazineAmmoLoader loader)
				{
					if (loader.AmmoPart == "AmmoArrow")
					{
						string arrow;
						switch (tier)
						{
							default: arrow = "Wooden Arrow"; break;
							case 2: arrow = "Steel Arrow"; break;
							case 3: arrow = "Carbide Arrow"; break;
							case 4: arrow = "Folded Carbide Arrow"; break;
							case 5: arrow = "Fullerite Arrow"; break;
							case 6: arrow = "Crysteel Arrow"; break;
							case 7: arrow = "Flawless Crysteel Arrow"; break;
							case 8: arrow = "Zetachrome Arrow"; break;
						}
						RestockAmmo(obj, arrow, 5 + 5 * tier);
					}
					else if (loader.AmmoPart == "AmmoSlug")
					{
						RestockAmmo(obj, "Lead Slug", tier > 1 ? (int)(10 * tier * (1 + tier / 8f)) : 10);
					}
					else if (loader.AmmoPart == "AmmoShotgunShell")
					{
						RestockAmmo(obj, "Shotgun Shell", 5 * tier);
					}
					else if (loader.AmmoPart == "AmmoMissile")
					{
						RestockAmmo(obj, "HE Missile", 2 * tier);
					}
				}
			}
		}

		private static void RestockAmmo(GameObject obj, string blueprint, int amount)
		{
			GameObject ammo = GameObjectFactory.Factory.CreateObject(blueprint);
			ammo.GetPart<Stacker>().StackCount = amount;
			// currently doesn't get removed properly
			//ammo.IntProperty["GauntletObject"] = 0;
			obj.ReceiveObject(ammo);
		}

		public static void AddCatalyst(GameObject golem, StringBuilder desc, RulesDescription rules, Random rand)
		{
			var liquid = GolemMaterialSelection<string, string>.Units.GetRandomElement(rand);
			foreach (GameObjectUnit unit in liquid.Value(liquid.Key))
			{
				unit.Apply(golem);
				if (unit.CanInscribe())
				{
					rules.Text += '\n' + unit.GetDescription(true);
				}
			}
			desc.Replace("=catalyst=", LiquidVolume.GetLiquid(liquid.Key)?.Name ?? liquid.Key);
		}

		public static GameObjectBlueprint GetValidAtzmus(Random rand)
		{
			List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
			List<string> aggregate = new List<string>();
			foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
			{
				if ((!EncountersAPI.IsEligibleForDynamicEncounters(blueprint) && blueprint.Name != "0lam" && blueprint.Inherits != "Troll King" && blueprint.Inherits != "BaseNephal")
				|| !blueprint.HasTag("Creature")
				|| !blueprint.HasPart("Body"))
				{
					continue;
				}
				if (blueprint.HasTag("AggregateWith"))
				{
					string tag = blueprint.GetTag("AggregateWith");
					if (aggregate.Contains(tag))
					{
						continue;
					}
					aggregate.Add(tag);
				}
				list.Add(blueprint);
			}

			return list.GetRandomElement(rand);
		}

		public static void AddAtzmus(GameObject golem, StringBuilder desc, RulesDescription rules, Random rand)
		{
			GameObject atzmus = GameObjectFactory.Factory.CreateObject(GetValidAtzmus(rand));
			desc.Replace("=atzmus.creature.an=", atzmus.an());

			List<BaseMutation> list = atzmus.GetPart<Mutations>()?.MutationList;
			if (!list.IsNullOrEmpty())
			{
				List<GameObjectMutationUnit> options = new List<GameObjectMutationUnit>();
				foreach (BaseMutation mutation in list)
				{
					if (mutation.IsDefect() || GolemAtzmusSelection.Blocklist.Contains(mutation.Name))
					{
						continue;
					}

					options.Add(new GameObjectMutationUnit
					{
						Name = mutation.DisplayName,
						Class = mutation.Name,
						Level = Math.Max(mutation.BaseLevel, 1),
						ShouldShowLevel = mutation.ShouldShowLevel()
					});
				}


				if (options.Count > 0)
				{
					GameObjectMutationUnit mutation = options.Count == 1 ? options[0] : options.GetRandomElement(rand);
					mutation.Apply(golem);
					if (mutation.CanInscribe())
					{
						rules.Text += '\n' + mutation.GetDescription(true);
					}
					return;
				}
			}

			List<string> stats = new List<string> { Statistic.Attributes[0] };
			int highest = -1;
			foreach (string attribute in Statistic.Attributes)
			{
				int stat = atzmus.GetStatValue(attribute);
				if (stat > highest)
				{
					stats.Clear();
					stats.Add(attribute);
					highest = stat;
				}
				else if (stat == highest)
				{
					stats.Add(attribute);
				}
			}

			GameObjectAttributeUnit boost = new GameObjectAttributeUnit
			{
				Attribute = (stats.Count == 1) ? stats[0] : stats.GetRandomElement(rand),
				Value = 5
			};
			boost.Apply(golem);
			if (boost.CanInscribe())
			{
				rules.Text += '\n' + boost.GetDescription(true);
			}
		}

		public static void AddIncantation(GameObject golem, StringBuilder desc, RulesDescription rules, Random rand)
		{
			var journal = GolemMaterialSelection<JournalAccomplishment, MuralCategory>.Units.GetRandomElement(rand);

			// no need to pass in an actual journal entry
			foreach (GameObjectUnit unit in journal.Value(null))
			{
				unit.Apply(golem);
				if (unit.CanInscribe())
				{
					rules.Text += '\n' + unit.GetDescription(true);
				}
			}
		}

		public static GameObjectBlueprint GetValidHamsa(string semantic, Random rand)
		{
			List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(64);
			List<string> aggregate = new List<string>();
			foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
			{
				if (blueprint.IsBaseBlueprint()
				|| blueprint.IsNatural()
				|| !blueprint.GetPartParameter("Physics", "IsReal", true)
				|| !blueprint.HasTag("Semantic" + semantic))
				{
					continue;
				}

				string name = blueprint.DisplayName();
				if (name == null || name.StartsWith('['))
				{
					continue;
				}

				if (blueprint.HasTag("AggregateWith"))
				{
					string tag = blueprint.GetTag("AggregateWith");
					if (aggregate.Contains(tag))
					{
						continue;
					}
					aggregate.Add(tag);
				}
				list.Add(blueprint);
			}

			// no object has the specified semantic tag
			if (list.Count < 1)
			{
				return null;
			}
			return list.GetRandomElement(rand);
		}

		public static void AddHamsa(GameObject golem, StringBuilder desc, RulesDescription rules, Random rand)
		{
			// initialize Units
			new GolemHamsaSelection();

			var trait = GolemMaterialSelection<GameObject, string>.Units.GetRandomElement(rand);
			GameObject hamsa = GameObjectFactory.Factory.CreateObject(GetValidHamsa(trait.Key, rand));
			// may be an unobtainable trait
			if (hamsa != null)
			{
				desc.Replace("=hamsa.an=", hamsa.an(AsIfKnown: true, NoConfusion: true, BaseOnly: true));
			}
			else
			{
				desc.Replace("=hamsa.an=", "something otherworldly");
			}

			GameObjectUnit unit = trait.Value(hamsa ?? golem).GetRandomElement(rand);
			unit.Apply(golem);
			if (unit.CanInscribe())
			{
				rules.Text += '\n' + unit.GetDescription(true);
			}
		}
	}
}