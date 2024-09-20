using ConsoleLib.Console;
using System;
using System.Collections.Generic;
using System.Threading;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation
{
	public class PhasingTongue : BaseMutation
	{
		public PhasingTongue()
		{
			DisplayName = "Phasing Tongue";
			Type = "Physical";
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade))
			{
				return ID == AIGetOffensiveAbilityListEvent.ID || ID == PooledEvent<CommandEvent>.ID;
			}
			return true;
		}

		public override string GetDescription()
		{
			return "You phase out targets with your tongue.";
		}

		public override string GetLevelText(int Level)
		{
			return "";
		}

		public int GetCooldown(int Level)
		{
			return 25; //TODO: scale with level once unique mutations can be boosted by rapid advancements and ego
		}

		public override void CollectStats(Templates.StatCollector stats, int Level)
		{
			stats.Set("Range", 10);
			stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
		}

		public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
		{
			if (E.Distance <= 10 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.Add("CommandPhasingTongue");
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CommandEvent E)
		{
			if (E.Command == "CommandPhasingTongue")
			{
				List<Cell> cells = PickLine(10, AllowVis.Any, RequireCombat: true, Label: "Phasing Tongue", Snap: true);
				if (cells == null)
				{
					return false;
				}

				// why
				cells.Remove(ParentObject.CurrentCell);

				if (cells.Count < 1)
				{
					return false;
				}

				ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_tonguePull");
				ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_physical_generic_activate");

				int dist = Math.Min(cells.Count, 10);
				int moved = 0;
				for (int i = 0; i < dist; i++)
				{
					bool movedCell = false;
					foreach (GameObject obj in cells[i].GetObjects())
					{
						if (!obj.IsCombatObject())
						{
							continue;
						}

						if (!ParentObject.HasEffect<Phased>())
						{
							if (obj.HasEffect<Phased>())
							{
								//TODO: apply RealityStabilized?
								obj.RemoveEffect<Phased>();
							}
						}
						else if (!obj.HasEffect<Phased>())
						{
							obj.ApplyEffect(new Phased(6));
						}

						movedCell = true;
						for (int j = i - 1; j >= moved; j--)
						{
							obj.Move(obj.CurrentCell.GetDirectionFromCell(cells[j]), true, IgnoreGravity: true, Dragging: ParentObject);
						}
					}
					if (movedCell)
					{
						moved++;
					}
				}

				ScreenBuffer buffer = ScreenBuffer.GetScrapBuffer1();
				for (int i = dist - 1; i >= 0; i--)
				{
					buffer.RenderBase();
					for (int j = 0; j <= i; j++)
					{
						if (cells[j].IsVisible())
						{
							buffer.Goto(cells[j].X, cells[j].Y);
							buffer.Write(j == i ? "&b\u0009" : "&B\u0007");
						}
					}
					buffer.Draw();
					Thread.Sleep(50);
				}

				if (!ParentObject.IsPlayer())
				{
					ParentObject.Brain.RemoveGoalsDescendedFrom<IMovementGoal>();
					ParentObject.Brain.PushGoal(new Flee(ParentObject.Target, 3));
				}

				UseEnergy(1000, "Physical Mutation PhasingTongue");
				CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
			}
			return base.HandleEvent(E);
		}

		public override bool Mutate(GameObject GO, int Level)
		{
			ActivatedAbilityID = AddMyActivatedAbility("Phasing Tongue", "CommandPhasingTongue", "Physical Mutations");
			return base.Mutate(GO, Level);
		}

		public override bool Unmutate(GameObject GO)
		{
			RemoveMyActivatedAbility(ref ActivatedAbilityID);
			return base.Unmutate(GO);
		}
	}
}