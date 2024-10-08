﻿using ConsoleLib.Console;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation
{
	public class Collapse : BaseMutation
	{
		public Collapse()
		{
			DisplayName = "Collapse";
			base.Type = "Mental";
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
			return "You collapse an area in a neutronic explosion with your mind.";
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
			stats.Set("Range", 12);
			stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
		}

		public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
		{
			if (E.Distance <= 12 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.Add("CommandCollapse");
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CommandEvent E)
		{
			if (E.Command == "CommandCollapse")
			{
				Cell cell = PickDestinationCell(12, RequireCombat: true, Label: "Collapse", Snap: true);
				if (cell == null)
				{
					return false;
				}

				GameObject widget = GameObjectFactory.Factory.CreateObject("Widget");
				widget.AddPart(new DelayedCollapse(ParentObject, 3));

				cell.AddObject(widget);
				cell.PlayWorldSound("Sounds/Creatures/Ability/sfx_creature_girshNephilim_irisdualBeam_windup", CostMultiplier: 0.2f, CostMaximum: 20);
				if (Options.UseParticleVFX && cell.IsVisible())
				{
					CombatJuice.playPrefabAnimation(cell.Location, "Impacts/ImpactVFXNeutronImpact", null, null, async: true);
				}

				if (!ParentObject.IsPlayer())
				{
					ParentObject.Brain.RemoveGoalsDescendedFrom<IMovementGoal>();
					ParentObject.Brain.PushGoal(new FleeLocation(cell, (200 - ParentObject.Stat("MoveSpeed", 100)) * 3 / 100));
				}

				UseEnergy(1000, "Mental Mutation Collapse");
				CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
			}
			return base.HandleEvent(E);
		}

		public override bool Mutate(GameObject GO, int Level)
		{
			ActivatedAbilityID = AddMyActivatedAbility("Collapse", "CommandCollapse", "Mental Mutations");
			return base.Mutate(GO, Level);
		}

		public override bool Unmutate(GameObject GO)
		{
			RemoveMyActivatedAbility(ref ActivatedAbilityID);
			return base.Unmutate(GO);
		}
	}

	public class DelayedCollapse : IPart
	{
		public GameObject owner;
		public int turns;

		public DelayedCollapse(GameObject owner, int turns)
		{
			this.owner = owner;
			this.turns = turns;
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int Cascade)
		{
			return ID == EndTurnEvent.ID;
		}

		public override bool FinalRender(RenderEvent E, bool bAlt)
		{
			E.WantsToPaint = true;
			return base.FinalRender(E, bAlt);
		}

		public override void OnPaint(ScreenBuffer buffer)
		{
			Cell cell = ParentObject.CurrentCell;
			ConsoleChar consoleChar = buffer.get(cell.X, cell.Y);
			if (consoleChar != null && XRLCore.CurrentFrame % 12 < 6)
			{
				consoleChar.Tile = null;
				consoleChar.Char = '\u0009';
				consoleChar.Foreground = The.Color.B;
			}
		}

		public override bool HandleEvent(EndTurnEvent E)
		{
			if (--turns <= 0)
			{
				List<GameObject> hit = Event.NewGameObjectList();
				hit.Add(ParentObject);
				Physics.ApplyExplosion(ParentObject.CurrentCell, 20000, Hit: hit, Local: false, Owner: owner, BonusDamage: "10d10+100", Phase: 3, Neutron: true);
				ParentObject.Obliterate();
			}
			else
			{
				foreach (Cell cell in ParentObject.CurrentCell.GetAdjacentCells(4))
				{
					foreach (GameObject obj in cell.Objects)
					{
						if (!obj.IsCombatObject())
						{
							continue;
						}
						if (obj.IsPlayer())
						{
							AutoAct.Interrupt("you are in the area of " + owner.poss("collapse"));
							continue;
						}
						if (!obj.IsPotentiallyMobile() || obj.Brain.Goals.Peek() is FleeLocation)
						{
							continue;
						}
						obj.Brain.RemoveGoalsDescendedFrom<IMovementGoal>();
						obj.Brain.PushGoal(new FleeLocation(ParentObject.CurrentCell, (200 - obj.Stat("MoveSpeed", 100)) * turns / 100));
					}
				}
			}
			return base.HandleEvent(E);
		}
	}
}