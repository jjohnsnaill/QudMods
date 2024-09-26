using HarmonyLib;
using Qud.API;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using XRL;
using XRL.CharacterBuilds;
using XRL.CharacterBuilds.Qud;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Quests.GolemQuest;
using XRL.World.Units;

namespace Mods.PlayableGolem
{
	public class PlayableGolemModuleData : AbstractEmbarkBuilderModuleData
	{
		public string selection;
	}

	[HarmonyPatch]
	public class PlayableGolemModule : EmbarkBuilderModule<PlayableGolemModuleData>
	{
		public override bool shouldBeEnabled()
		{
			return builder?.GetModule<QudGenotypeModule>()?.data?.Genotype == "Golem" && builder.GetModule<QudSubtypeModule>()?.data?.Subtype != null;
		}

		public override string DataErrors()
		{
			if (data.selection.IsNullOrEmpty())
			{
				return "You have not selected a body.";
			}
			return null;
		}

		public override void assembleWindowDescriptors(List<EmbarkBuilderModuleWindowDescriptor> windows)
		{
			for (int i = 0; i < windows.Count; i++)
			{
				if (windows[i].viewID == "Chargen/ChooseSubtypes")
				{
					windows.InsertRange(i + 1, this.windows.Values);
					return;
				}
			}
			windows.InsertRange(7, this.windows.Values);
		}

		public override void InitFromSeed(string seed)
		{

		}

		public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
		{
			if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECTBLUEPRINT)
			{
				return data.selection;
			}
			else if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYERTILE)
			{
				if (data?.selection != null)
				{
					return GameObjectFactory.Factory.Blueprints[data.selection].GetPartParameter<string>("Render", "Tile");
				}
			}
			else if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT)
			{
				GameObject player = (GameObject)element;
				player.RemovePart("Interior");
				player.RemovePart("Vehicle");
				player.RemovePart("Unreplicable");
				player.RemovePart("TetheredOnboardRecoilerTeleporter");
				player.RemovePart("CannotBeInfluenced");
				player.RemovePart("CarryBonus");
				player.RemoveEffect<Unpiloted>();

				List<BodyPart> parts = player.Body.GetParts();
				for (int i = 0; i < parts.Count; i++)
				{
					if (parts[i].Primary && parts[i].DefaultBehavior == null)
					{
						string natural;
						if (player.IsOrganic)
						{
							natural = "Bite";
						}
						else
						{
							natural = parts[i].Type == "Face" ? "MetalManipulator" : "Blunt End";
						}
						parts[i].DefaultBehavior = GameObject.CreateUnmodified(natural);
						parts[i].DefaultBehaviorBlueprint = natural;
					}
				}

				//player.IsGiganticCreature = false;

				// fixes the golem's hitpoints being set below 1, and its highest intelligence being tracked too early
				player.RemovePart("Leveler");

				int bonusToughness = player.Statistics["Hitpoints"].BaseValue / 500 * 2 - 2;

				player.Statistics["Level"].BaseValue = 1;
				player.FinalizeStats();

				if (bonusToughness > 0)
				{
					player.Statistics["Toughness"].BaseValue += bonusToughness;
				}
				player.Statistics["Hitpoints"].BaseValue = player.Statistics["Toughness"].BaseValue + 8;
				if (player.Statistics["AV"].BaseValue > 4)
				{
					player.Statistics["AV"].BaseValue = 4;
				}

				foreach (BaseMutation mutation in player.GetPart<Mutations>().MutationList)
				{
					if (mutation.BaseLevel > 1)
					{
						mutation.Level = 1;
					}
				}

				string subtype = builder.GetModule<QudSubtypeModule>()?.data?.Subtype;

				player.AddPart(new GolemAbsorb { cost = subtype == "Unformed" ? 8 : 12 });
				player.AddPart<Leveler>();

				// required for randomly picked effects to work, and to initialize Units
				GolemQuestSystem.Require();

				Description desc = player.GetPart<Description>();
				RulesDescription rules = player.RequirePart<RulesDescription>();

				int[] options;
				if (subtype == "Unformed")
				{
					options = new int[0];

					player.SetIntProperty("SpentMP", 4);
				}
				else if (subtype == "Experimental")
				{
					options = new int[]
					{
						Stat.Rand.Next(4),
						Stat.Rand.Next(4),
						Stat.Rand.Next(4)
					};
				}
				else
				{
					options = new int[]
					{
						0,
						1,
						3
					};
				}

				for (int i = 0; i < options.Length; i++)
				{
					if (options[i] == 0)
					{
						var liquid = GolemMaterialSelection<string, string>.Units.GetRandomElement();
						foreach (GameObjectUnit unit in liquid.Value(liquid.Key))
						{
							AddEffect(player, unit, rules);
						}
						desc._Short += "\n=pronouns.Possessive= veins course with " + (LiquidVolume.GetLiquid(liquid.Key)?.Name ?? liquid.Key) + ".";
					}
					else if (options[i] == 1)
					{
						GameObject atzmus = GameObjectFactory.Factory.CreateObject(GetValidAtzmus());
						AddEffect(player, GetAtzmus(atzmus).GetRandomElement(), rules);
						desc._Short += "\n=pronouns.Possessive= posture evokes the presence of " + atzmus.an(AsIfKnown: true, NoConfusion: true) + ".";
					}
					else if (options[i] == 2)
					{
						var journal = GolemMaterialSelection<JournalAccomplishment, MuralCategory>.Units.GetRandomElement();

						// no need to pass in an actual journal entry
						foreach (GameObjectUnit unit in journal.Value(null))
						{
							AddEffect(player, unit, rules);
						}
						desc._Short += "\n=pronouns.Possessive= ears resonate with an incantation.";
					}
					else if (options[i] == 3)
					{
						var effect = GolemMaterialSelection<GameObject, string>.Units.GetRandomElement();
						GameObject hamsa = GameObjectFactory.Factory.CreateObject(GetValidHamsa(effect.Key));

						// the effect may not belong to any item, so add failsafes
						AddEffect(player, effect.Value(hamsa ?? player).GetRandomElement(), rules);
						if (hamsa != null)
						{
							desc._Short += "\n=pronouns.Subjective= =verb:bear:afterpronoun= the sacred amulet of " + hamsa.an(AsIfKnown: true, NoConfusion: true) + ".";
						}
						else
						{
							desc._Short += "\n=pronouns.Subjective= =verb:bear:afterpronoun= the sacred amulet of something otherworldly.";
						}
					}
				}

				player.ReceiveObject(GameObjectFactory.Factory.CreateObject("HalfFullWaterskin"));
			}
			return base.handleBootEvent(id, game, info, element);
		}

		public static void AddEffect(GameObject golem, GameObjectUnit unit, RulesDescription rules)
		{
			unit = Tweak(unit, golem, 0);
			unit.Apply(golem);

			if (unit.CanInscribe())
			{
				StringBuilder sb = Event.NewStringBuilder(rules.Text);

				if (sb.Length > 0)
				{
					sb.Append('\n');
				}
				sb.Append(unit.GetDescription(true));

				rules.Text = Event.FinalizeString(sb);
			}
		}

		public static void AddEffect(GameObject golem, GameObjectUnit unit, RulesDescription rules, StringBuilder result)
		{
			unit = Tweak(unit, golem, 0);
			unit.Apply(golem);

			if (result.Length > 0)
			{
				result.Append('\n');
			}
			result.Append(unit.GetDescription());

			if (unit.CanInscribe())
			{
				StringBuilder sb = Event.NewStringBuilder(rules.Text);

				if (sb.Length > 0)
				{
					sb.Append('\n');
				}
				sb.Append(unit.GetDescription(true));

				rules.Text = Event.FinalizeString(sb);
			}
		}

		//TODO: really messy, clean this up
		public static GameObjectUnit Tweak(GameObjectUnit unit, GameObject golem, int aggregate)
		{
			if (unit is GameObjectUnitAggregate units)
			{
				for (int i = 0; i < units.Units.Count; i++)
				{
					units.Units[i] = Tweak(units.Units[i], golem, aggregate + 1);
				}
			}
			else if (unit is GameObjectPartUnit part)
			{
				if (part.Part is CarryBonus carryBonus)
				{
					carryBonus.Amount = 50;
				}
				else if (part.Part is AttackerElementalAmplifier elementAmp)
				{
					if (elementAmp.Amplification > 50)
					{
						elementAmp.Amplification = 50;
					}
				}
				else if (part.Part is ThermalAmp thermalAmp)
				{
					if (thermalAmp.ColdDamage > 0 && thermalAmp.HeatDamage > 0)
					{
						thermalAmp.ColdDamage = 25;
						thermalAmp.ModifyCold = 25;
						thermalAmp.HeatDamage = 25;
						thermalAmp.ModifyHeat = 25;
					}
					else if (thermalAmp.ColdDamage > 0)
					{
						thermalAmp.ColdDamage = 40;
						thermalAmp.ModifyCold = 40;
					}
					else if (thermalAmp.HeatDamage > 0)
					{
						thermalAmp.HeatDamage = 40;
						thermalAmp.ModifyHeat = 40;
					}
				}
				else if (part.Part is StickOnHit stickOnHit)
				{
					stickOnHit.Chance = 25;
					part.Description = "25% chance for melee attacks to make enemies stuck";
				}
				else if (part.Part is RustOnHit rustOnHit)
				{
					rustOnHit.Amount = "1";
					rustOnHit.Chance = 25;
					rustOnHit.PreferPartType = null;
					part.Description = "25% chance for melee attacks to rust 1 equipped enemy item";
				}
				else if (part.Part is VehiclePairBonus)
				{
					return new GameObjectAttributeUnit
					{
						Attribute = "Speed",
						Value = 10
					};
				}
			}
			else if (unit is GameObjectCloneUnit)
			{
				return new GameObjectPartUnit
				{
					Part = new Twinner()
				};
			}
			else if (unit is GameObjectTieredArmorUnit armor)
			{
				armor.Tier = Math.Min((golem.Stat("Level") + 4) / 5, 8).ToString();
			}
			else if (unit is GameObjectRelicUnit relic)
			{
				relic.Tier = Math.Min((golem.Stat("Level") + 4) / 5, 8).ToString();
			}
			else if (unit is GameObjectBaetylUnit baetyl)
			{
				baetyl.Tier = Math.Min((golem.Stat("Level") + 4) / 5, 8).ToString();
			}
			else if (unit is GameObjectBodyPartUnit body)
			{
				body.Metachromed = false;
			}
			else if (unit is GameObjectReputationUnit rep)
			{
				rep.Value /= 10;
			}
			else if (unit is GameObjectExperienceUnit xp)
			{
				xp.Levels /= 5;
			}
			else if (unit is GameObjectAttributeUnit stat)
			{
				if (stat.Attribute.EndsWith("Resistance"))
				{
					stat.Value = aggregate > 1 ? 5 : aggregate > 0 ? 10 : 20;
				}
				else if (stat.Attribute == "SP")
				{
					stat.Value = 200;
				}
				else if (stat.Attribute == "AP")
				{
					stat.Value = 2;
				}
				else if (stat.Attribute == "AV" || stat.Attribute == "DV")
				{
					stat.Value = Math.Clamp(stat.Value, -2, 2);
				}
				else if (stat.Attribute == "MoveSpeed")
				{
					stat.Value = Math.Clamp(stat.Value, -25, 25);
				}
				else if (Statistic.Attributes.Contains(stat.Attribute))
				{
					stat.Value = Math.Clamp(stat.Value, -3, Math.Max(3 - aggregate, 1));
				}
				else if (stat.Attribute == "All")
				{
					stat.Value = Math.Clamp(stat.Value, -1, 1);
				}
			}
			else if (unit is GameObjectMutationUnit mutation)
			{
				mutation.Level = 1;
			}
			return unit;
		}

		public static GameObjectBlueprint GetValidHamsa(string semantic)
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
			return list.GetRandomElement();
		}

		public static GameObjectBlueprint GetValidAtzmus()
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

			return list.GetRandomElement();
		}

		public static List<GameObjectUnit> GetAtzmus(GameObject atzmus)
		{
			List<GameObjectUnit> list = new List<GameObjectUnit>();

			List<BaseMutation> mutations = atzmus.GetPart<Mutations>()?.MutationList;
			if (!mutations.IsNullOrEmpty())
			{
				foreach (BaseMutation mutation in mutations)
				{
					if (mutation.IsDefect() || GolemAtzmusSelection.Blocklist.Contains(mutation.Name))
					{
						continue;
					}

					list.Add(new GameObjectMutationUnit
					{
						Name = mutation.DisplayName,
						Class = mutation.Name,
						Level = 1, //Math.Max(mutation.BaseLevel, 1),
						ShouldShowLevel = mutation.ShouldShowLevel()
					});
				}

				if (list.Count > 0)
				{
					return list;
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

			for (int i = 0; i < stats.Count; i++)
			{
				list.Add(new GameObjectAttributeUnit
				{
					Attribute = stats[i],
					Value = 3 //5
				});
			}
			return list;
		}

		private static GameObjectUnit TweakEffect(GameObjectUnit unit, GameObject golem)
		{
			if (golem.GetGenotype() == "Golem")
			{
				return Tweak(unit, golem, 1); // set aggregate to 1 so the extra effects are weaker
			}
			return unit;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QudAttributesModule), "shouldBeEnabled")]
		static bool GolemNoAttributes(QudAttributesModule __instance, ref bool __result)
		{
			if (__instance.builder?.GetModule<QudGenotypeModule>()?.data?.Genotype == "Golem")
			{
				__result = false;
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MutationsAPI), "BuyRandomMutation")]
		static bool GolemBuyMutation(GameObject Object, int Cost, bool Confirm, string MutationTerm, ref bool __result)
		{
			if (Object.TryGetPart(out GolemAbsorb golem))
			{
				Cost = golem.cost;

				int finalCost;
				if (Object.TryGetIntProperty("SpentMP", out int discount))
				{
					finalCost = discount >= Cost ? 0 : Cost - discount;
				}
				else
				{
					finalCost = Cost;
				}

				if (Object.Stat("MP") < finalCost)
				{
					Object.ShowFailure("You don't have " + finalCost + " mutation points!\n\n{{rules|This cost is discounted by spending mutation points elsewhere.}}");
				}
				else if (!Confirm || !Object.IsPlayer() || Popup.ShowYesNo("Are you sure you want to spend " + finalCost + " mutation points to absorb something?\n\n{{rules|This cost is discounted by spending mutation points elsewhere.}}") == DialogResult.Yes)
				{
					__result = GolemAbsorb.Absorb(Object, Cost);
					return false;
				}

				__result = false;
				return false;
			}

			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Leveler), "RapidAdvancement")]
		static bool GolemRapidAdvancement(int Amount, GameObject ParentObject)
		{
			if (Amount > 0 && ParentObject.TryGetPart(out GolemAbsorb golem))
			{
				int cost;
				if (ParentObject.TryGetIntProperty("SpentMP", out int discount))
				{
					cost = discount >= golem.cost ? 0 : golem.cost - discount;
				}
				else
				{
					cost = golem.cost;
				}

				SyncMutationLevelsEvent.Send(ParentObject);
				bool prompt = ParentObject.IsPlayer() && ParentObject.Stat("MP") >= cost;
				bool bought = false;

				if (prompt && Popup.ShowYesNo("Your genome enters an excited state! Spend {{rules|" + cost + "}} mutation points to absorb something first?", "Sounds/UI/ui_notification_question", false) == DialogResult.Yes)
				{
					bought = GolemAbsorb.Absorb(ParentObject, cost);
				}

				var mutations = ParentObject.GetPhysicalMutations();
				if (mutations.Count > 0)
				{
					string[] options = new string[mutations.Count];
					for (int i = 0; i < mutations.Count; i++)
					{
						options[i] = mutations[i].DisplayName + " ({{C|" + mutations[i].Level + "}})";
					}

					if (ParentObject.IsPlayer())
					{
						int index = Popup.PickOption("Choose a physical " + GetMutationTermEvent.GetFor(ParentObject) + " to rapidly advance.", Sound: "Sounds/Misc/sfx_characterMod_mutation_windowPopup", Options: options);
						Popup.Show("You have rapidly advanced " + mutations[index].DisplayName + " by " + Grammar.Cardinal(Amount) + " ranks to rank {{C|" + (mutations[index].Level + Amount) + "}}!", Sound: "Sounds/Misc/sfx_characterMod_mutation_rankUp_quickSuccession");
						mutations[index].RapidLevel(Amount);
					}
					else
					{
						mutations.GetRandomElement().RapidLevel(Amount);
					}
				}
				else if (bought)
				{
					Popup.Show("You have no physical " + Grammar.Pluralize(GetMutationTermEvent.GetFor(ParentObject)) + " to rapidly advance!");
				}

				return false;
			}

			return true;
		}

		//TODO: remove
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(Stomach), "UpdateHunger")]
		static IEnumerable<CodeInstruction> HungerFix(IEnumerable<CodeInstruction> instr)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Is(OpCodes.Ldstr, "Robot"))
				{
					list.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
					list.Insert(i + 3, CodeInstruction.Call(typeof(IPart), "get_ParentObject"));
					list.Insert(i + 4, CodeInstruction.Call(typeof(PlayableGolemModule), "NotGolem"));
					break;
				}
			}
			return list;
		}
		static bool NotGolem(bool result, GameObject obj) => result && obj.GetGenotype() != "Golem";

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(GameObjectGolemQuestRandomUnit), "Apply")]
		static IEnumerable<CodeInstruction> TweakEffects(IEnumerable<CodeInstruction> instr, ILGenerator generator)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instr);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i + 2].Is(OpCodes.Callvirt, typeof(GameObjectUnit).GetMethod("Apply")))
				{
					CodeInstruction unit = list[i];
					CodeInstruction golem = list[i + 1];

					list.Insert(i, new CodeInstruction(OpCodes.Stloc_S, unit.operand));
					list.Insert(i, CodeInstruction.Call(typeof(PlayableGolemModule), "TweakEffect"));
					list.Insert(i, new CodeInstruction(golem.opcode, golem.operand));

					// change the destination of an earlier instruction that would otherwise branch over the injection
					CodeInstruction c = new CodeInstruction(unit.opcode, unit.operand);
					var label = generator.DefineLabel();
					c.labels.Add(label);
					list.Insert(i, c);
					list[i - 5].operand = label;

					break;
				}
			}
			return list;
		}
	}
}