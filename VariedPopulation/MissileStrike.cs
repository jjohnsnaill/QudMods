using ConsoleLib.Console;
using Genkit;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation
{
	public class MissileStrike : BaseMutation
	{
		public MissileStrike()
		{
			DisplayName = "Missile Strike";
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
			return "You launch a missile at a location.";
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
			if (E.Distance > 3 && E.Distance <= 12 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.Add("CommandMissileStrike");
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CommandEvent E)
		{
			if (E.Command == "CommandMissileStrike")
			{
				Cell cell = PickDestinationCell(12, Locked: false, Label: "Launch Missile");
				if (cell == null)
				{
					return false;
				}

				GameObject widget = GameObjectFactory.Factory.CreateObject("Widget");
				widget.AddPart(new DelayedMissileStrike(ParentObject, 4, ParentObject.GetPhase()));

				cell.AddObject(widget);
				PlayWorldSound("Sounds/Missile/Fires/Heavy Weapons/sfx_missile_missileLauncher_fire");

				MissileWeaponVFXConfiguration vfx = MissileWeaponVFXConfiguration.next();
				CombatJuiceManager.startDelay();
				vfx.addStep(0, ParentObject.CurrentCell.Location);
				vfx.addStep(0, cell.Location);
				vfx.setPathProjectileVFX(0, "MissileWeaponsEffects/vls_laser", "duration::1;;beamColor0::#FFFFFF;;beamColor1::#FFFFFF");
				CombatJuiceManager.endDelay();
				CombatJuice.missileWeaponVFX(vfx);

				if (!ParentObject.IsPlayer())
				{
					ParentObject.Brain.RemoveGoalsDescendedFrom<IMovementGoal>();
					ParentObject.Brain.PushGoal(new FleeLocation(cell, (200 - ParentObject.Stat("MoveSpeed", 100)) * 3 / 100));
				}

				UseEnergy(1000, "Physical Mutation MissileStrike");
				CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
			}
			return base.HandleEvent(E);
		}

		public override bool Mutate(GameObject GO, int Level)
		{
			ActivatedAbilityID = AddMyActivatedAbility("Launch Missile", "CommandMissileStrike", "Physical Mutations");
			return base.Mutate(GO, Level);
		}

		public override bool Unmutate(GameObject GO)
		{
			RemoveMyActivatedAbility(ref ActivatedAbilityID);
			return base.Unmutate(GO);
		}
	}

	public class DelayedMissileStrike : IPart
	{
		public GameObject owner;
		public int turns;
		public int phase;

		public DelayedMissileStrike(GameObject owner, int turns, int phase)
		{
			this.owner = owner;
			this.turns = turns;
			this.phase = phase;
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
			int num = XRLCore.CurrentFrame % 60;
			Location2D cell = ParentObject.CurrentCell.Location;
			ConsoleChar consoleChar;
			if (num < 30)
			{
				int radius = num / 10;
				if (cell != null)
				{
					for (int i = -radius; i <= radius; i++)
					{
						for (int j = -radius; j <= radius; j++)
						{
							Location2D loc = Location2D.Get(cell.X + i, cell.Y + j);
							if (loc != null && loc.Distance(cell) == radius)
							{
								consoleChar = buffer.get(loc.X, loc.Y);
								if (consoleChar != null)
								{
									consoleChar.Tile = null;
									consoleChar.Char = '!';
									consoleChar.Foreground = The.Color.R;
								}
							}
						}
					}
				}
			}
			consoleChar = buffer.get(cell.X, cell.Y);
			if (consoleChar != null)
			{
				consoleChar.Tile = null;
				consoleChar.Char = 'X';
				consoleChar.Foreground = The.Color.R;
			}
		}

		public override bool HandleEvent(EndTurnEvent E)
		{
			if (--turns <= 0)
			{
				if (Options.UseParticleVFX && ParentObject.CurrentZone != null & ParentObject.CurrentZone.IsActive())
				{
					CombatJuice.playPrefabAnimation(ParentObject.CurrentCell.Location, "MissileWeaponsEffects/vls_impact");
					CombatJuiceWait(0.5f);
				}
				List<GameObject> hit = Event.NewGameObjectList();
				hit.Add(ParentObject);
				Physics.ApplyExplosion(ParentObject.CurrentCell, 30000, Hit: hit, Local: false, Owner: owner, Phase: phase);
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
							AutoAct.Interrupt("you are in the area of " + owner.poss("missile strike"));
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