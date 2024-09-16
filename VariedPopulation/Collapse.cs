using System.Collections.Generic;
using XRL.Core;
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
				return ID == AIGetOffensiveAbilityListEvent.ID;
			}
			return true;
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("CommandCollapse");
			base.Register(Object, Registrar);
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
			stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
		}

		public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
		{
			if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.Add("CommandCollapse");
			}
			return base.HandleEvent(E);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "CommandCollapse")
			{
				Cell cell = PickDestinationCell(Locked: false, Label: "Collapse");

				GameObject widget = GameObjectFactory.Factory.CreateObject("Widget");
				widget.AddPart(new DelayedCollapse(ParentObject, 3));

				cell.AddObject(widget);
				PlayWorldSound("Sounds/Creatures/Ability/sfx_creature_girshNephilim_irisdualBeam_windup", CostMultiplier: 0.2f, CostMaximum: 20);
				if (cell.IsVisible())
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
			return base.FireEvent(E);
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